#pragma once
#include <Windows.h>
#include <d3d11.h>
#include <dxgi.h>
#include <imgui.h>
#include <imgui_impl_dx11.h>
#include <imgui_impl_win32.h>
#include "deffhack_gui.hpp"

#define WM_KEYDOWN 0x0100
#define VK_INSERT 0x2D

// MinHook support - попробуем найти MinHook, если не найден - используем простой inline hook
#ifdef USE_MINHOOK
    #include "MinHook.h"
    #define HAS_MINHOOK 1
#else
    // Проверяем наличие MinHook.h в стандартных путях
    #if __has_include("MinHook.h")
        #include "MinHook.h"
        #define HAS_MINHOOK 1
    #elif __has_include("minhook/MinHook.h")
        #include "minhook/MinHook.h"
        #define HAS_MINHOOK 1
    #else
        #define HAS_MINHOOK 0
    #endif
#endif

#if HAS_MINHOOK
    inline bool HOOK_INIT() { return MH_Initialize() == MH_OK; }
    inline bool HOOK_CREATE(void* target, void* hook, void** original) { 
        return MH_CreateHook(target, hook, original) == MH_OK; 
    }
    inline bool HOOK_ENABLE(void* target) { return MH_EnableHook(target) == MH_OK; }
#else
    // Простой inline hook через патч jmp (базовая реализация)
    #include <cstring>
    constexpr bool HOOK_INIT() { return true; }
    inline void HOOK_CREATE(void* target, void* hook, void** original) { *original = target; }
    inline void HOOK_ENABLE(void* target) { /* inline hook implementation would go here */ }
#endif

extern IMGUI_IMPL_API LRESULT ImGui_ImplWin32_WndProcHandler(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

namespace Renderer {
    inline HWND window = nullptr;
    inline ID3D11Device* device = nullptr;
    inline ID3D11DeviceContext* context = nullptr;
    inline IDXGISwapChain* swapchain = nullptr;
    inline WNDPROC oWndProc = nullptr;
    inline void* oPresent = nullptr;
    inline bool initialized = false;

    inline LRESULT __stdcall WndProc(const HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
        // Обработка горячих клавиш
        if (uMsg == WM_KEYDOWN) {
            if (wParam == VK_INSERT) {
                DeftHackGUI::open = !DeftHackGUI::open;
                return true;
            }
        }
        
        if (ImGui_ImplWin32_WndProcHandler(hWnd, uMsg, wParam, lParam))
            return true;
        return CallWindowProc(oWndProc, hWnd, uMsg, wParam, lParam);
    }

    inline void InitImGui() {
        ImGui::CreateContext();
        ImGuiIO& io = ImGui::GetIO();
        io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
        io.IniFilename = nullptr;
        io.LogFilename = nullptr;

        // Применяем наш кастомный стиль
        DeftHackGUI::StyleDeffHack();

        ImGui_ImplWin32_Init(window);
        ImGui_ImplDX11_Init(device, context);
        
        initialized = true;
    }

    inline HRESULT __stdcall hkPresent(IDXGISwapChain* pSwapChain, UINT SyncInterval, UINT Flags) {
        if (!initialized) {
            if (SUCCEEDED(pSwapChain->GetDevice(__uuidof(ID3D11Device), reinterpret_cast<void**>(&device)))) {
                device->GetImmediateContext(&context);
                
                DXGI_SWAP_CHAIN_DESC sd;
                pSwapChain->GetDesc(&sd);
                window = sd.OutputWindow;
                
                oWndProc = reinterpret_cast<WNDPROC>(SetWindowLongPtr(window, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(WndProc)));
                InitImGui();
            }
        }

        if (initialized) {
            ImGui_ImplDX11_NewFrame();
            ImGui_ImplWin32_NewFrame();
            ImGui::NewFrame();

            DeftHackGUI::Render();

            ImGui::Render();
            ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());
        }

        return reinterpret_cast<HRESULT(WINAPI*)(IDXGISwapChain*, UINT, UINT)>(oPresent)(pSwapChain, SyncInterval, Flags);
    }

    inline void Init(HMODULE hModule) {
        // Ждем пока игра инициализирует DX11
        Sleep(2000);

        // Ищем окно игры
        HWND hwnd = nullptr;
        for (int i = 0; i < 50; i++) {
            hwnd = FindWindowA("UnityWndClass", nullptr);
            if (!hwnd) {
                hwnd = FindWindowA(nullptr, "Unturned");
            }
            if (hwnd) break;
            Sleep(100);
        }
        
        if (!hwnd) return;

        // Создаем временный DX11 для получения SwapChain
        D3D_FEATURE_LEVEL featureLevel = D3D_FEATURE_LEVEL_11_0;
        DXGI_SWAP_CHAIN_DESC sd{};
        sd.BufferCount = 1;
        sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
        sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
        sd.OutputWindow = hwnd;
        sd.SampleDesc.Count = 1;
        sd.Windowed = TRUE;
        sd.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
        sd.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;

        IDXGISwapChain* tempSwapChain = nullptr;
        ID3D11Device* tempDevice = nullptr;
        ID3D11DeviceContext* tempContext = nullptr;

        if (FAILED(D3D11CreateDeviceAndSwapChain(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, 0, &featureLevel, 1,
            D3D11_SDK_VERSION, &sd, &tempSwapChain, &tempDevice, nullptr, &tempContext))) {
            return;
        }

        if (!tempSwapChain) return;

        // Получаем адрес Present из vtable
        void** vtable = *reinterpret_cast<void***>(tempSwapChain);
        void* presentFunc = vtable[8]; // Present находится по индексу 8 в vtable IDXGISwapChain

        // Инициализируем MinHook для хука
#if HAS_MINHOOK
        if (HOOK_INIT()) {
            void* original = nullptr;
            if (HOOK_CREATE(presentFunc, &hkPresent, &original)) {
                oPresent = original; // Сохраняем оригинальный адрес
                HOOK_ENABLE(presentFunc);
            }
        }
#else
        // Fallback: сохраняем оригинальный адрес (для inline hook понадобится дополнительная реализация)
        oPresent = presentFunc;
        // TODO: Реализовать inline hook если MinHook недоступен
#endif

        // Очищаем временные объекты
        if (tempContext) tempContext->Release();
        if (tempDevice) tempDevice->Release();
        if (tempSwapChain) tempSwapChain->Release();

        // Теперь хук будет вызван когда игра вызовет Present
        // hkPresent получит реальный SwapChain игры
    }
}

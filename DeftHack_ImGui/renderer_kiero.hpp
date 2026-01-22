#pragma once
#include <Windows.h>
#include <d3d11.h>
#include <dxgi.h>
#include <imgui.h>
#include <imgui_impl_dx11.h>
#include <imgui_impl_win32.h>
#include "kiero-1.2.12/kiero.h"
#include "deffhack_gui.hpp"

#define WM_KEYDOWN 0x0100
#define VK_INSERT 0x2D

extern IMGUI_IMPL_API LRESULT ImGui_ImplWin32_WndProcHandler(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

namespace Renderer {
    inline HWND window = nullptr;
    inline ID3D11Device* device = nullptr;
    inline ID3D11DeviceContext* context = nullptr;
    inline ID3D11RenderTargetView* mainRenderTargetView = nullptr;
    inline WNDPROC oWndProc = nullptr;
    inline bool initialized = false;

    typedef HRESULT(__stdcall* Present)(IDXGISwapChain* pSwapChain, UINT SyncInterval, UINT Flags);
    Present oPresent = nullptr;

    inline LRESULT __stdcall WndProc(const HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
        if (uMsg == WM_KEYDOWN) {
            if (wParam == VK_INSERT) {
                DeftHackGUI::open = !DeftHackGUI::open;
                return true;
            }
        }
        
        if (DeftHackGUI::open && ImGui_ImplWin32_WndProcHandler(hWnd, uMsg, wParam, lParam))
            return true;
            
        return CallWindowProc(oWndProc, hWnd, uMsg, wParam, lParam);
    }

    inline void InitImGui(IDXGISwapChain* pSwapChain) {
        if (SUCCEEDED(pSwapChain->GetDevice(__uuidof(ID3D11Device), (void**)&device))) {
            device->GetImmediateContext(&context);
            
            DXGI_SWAP_CHAIN_DESC sd;
            pSwapChain->GetDesc(&sd);
            window = sd.OutputWindow;
            
            ID3D11Texture2D* pBackBuffer;
            pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&pBackBuffer);
            device->CreateRenderTargetView(pBackBuffer, NULL, &mainRenderTargetView);
            pBackBuffer->Release();
            
            oWndProc = (WNDPROC)SetWindowLongPtr(window, GWLP_WNDPROC, (LONG_PTR)WndProc);
            
            ImGui::CreateContext();
            ImGuiIO& io = ImGui::GetIO();
            io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
            io.IniFilename = nullptr;
            io.LogFilename = nullptr;

            DeftHackGUI::StyleDeffHack();

            ImGui_ImplWin32_Init(window);
            ImGui_ImplDX11_Init(device, context);
            
            initialized = true;
        }
    }

    inline HRESULT __stdcall hkPresent(IDXGISwapChain* pSwapChain, UINT SyncInterval, UINT Flags) {
        if (!initialized) {
            InitImGui(pSwapChain);
        }

        if (initialized) {
            ImGui_ImplDX11_NewFrame();
            ImGui_ImplWin32_NewFrame();
            ImGui::NewFrame();

            DeftHackGUI::Render();

            ImGui::Render();
            context->OMSetRenderTargets(1, &mainRenderTargetView, NULL);
            ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());
        }

        return oPresent(pSwapChain, SyncInterval, Flags);
    }

    inline void Init(HMODULE hModule) {
        // Ждем инициализации игры
        Sleep(3000);

        // Инициализируем Kiero
        if (kiero::init(kiero::RenderType::D3D11) == kiero::Status::Success) {
            // Хукаем Present (индекс 8 для IDXGISwapChain::Present)
            kiero::bind(8, (void**)&oPresent, hkPresent);
        }
    }

    inline void Shutdown() {
        if (initialized) {
            SetWindowLongPtr(window, GWLP_WNDPROC, (LONG_PTR)oWndProc);
            
            ImGui_ImplDX11_Shutdown();
            ImGui_ImplWin32_Shutdown();
            ImGui::DestroyContext();
            
            if (mainRenderTargetView) {
                mainRenderTargetView->Release();
                mainRenderTargetView = nullptr;
            }
            if (context) {
                context->Release();
                context = nullptr;
            }
            if (device) {
                device->Release();
                device = nullptr;
            }
        }
        
        kiero::shutdown();
    }
}

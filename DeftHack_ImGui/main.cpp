#include <Windows.h>
#include <d3d11.h>
#include <dxgi.h>

#include "kiero-1.2.12/kiero.h"
#include "imgui.h"
#include "imgui_impl_dx11.h"
#include "imgui_impl_win32.h"
#include "deffhack_gui.hpp"

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")

// ================= GLOBALS =================
static ID3D11Device* g_Device = nullptr;
static ID3D11DeviceContext* g_Context = nullptr;
static ID3D11RenderTargetView* g_RTV = nullptr;
static HWND                     g_Hwnd = nullptr;

using PresentFn = HRESULT(__stdcall*)(IDXGISwapChain*, UINT, UINT);
static PresentFn oPresent = nullptr;

static WNDPROC g_OrigWndProc = nullptr;

// ================= WNDPROC =================
LRESULT CALLBACK hkWndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    // Toggle menu with INSERT key
    if (msg == WM_KEYDOWN && wParam == VK_INSERT) {
        DeftHackGUI::open = !DeftHackGUI::open;
        return TRUE;
    }

    if (DeftHackGUI::open && ImGui_ImplWin32_WndProcHandler(hWnd, msg, wParam, lParam))
        return TRUE;

    return CallWindowProcW(g_OrigWndProc, hWnd, msg, wParam, lParam);
}

// ================= PRESENT =================
HRESULT __stdcall hkPresent(IDXGISwapChain* swap, UINT sync, UINT flags)
{
    static bool init = false;
    if (!init)
    {
        // Device / Context
        swap->GetDevice(__uuidof(ID3D11Device), (void**)&g_Device);
        g_Device->GetImmediateContext(&g_Context);

        // HWND
        DXGI_SWAP_CHAIN_DESC sd{};
        swap->GetDesc(&sd);
        g_Hwnd = sd.OutputWindow;

        // RTV
        ID3D11Texture2D* backBuffer = nullptr;
        swap->GetBuffer(0, __uuidof(ID3D11Texture2D), (void**)&backBuffer);
        g_Device->CreateRenderTargetView(backBuffer, nullptr, &g_RTV);
        backBuffer->Release();

        // ImGui
        ImGui::CreateContext();
        ImGuiIO& io = ImGui::GetIO();
        io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
        io.IniFilename = nullptr;
        io.LogFilename = nullptr;

        // Apply DeftHack style
        DeftHackGUI::StyleDeffHack();

        ImGui_ImplWin32_Init(g_Hwnd);
        ImGui_ImplDX11_Init(g_Device, g_Context);

        // WndProc hook
        g_OrigWndProc = (WNDPROC)GetWindowLongPtrW(g_Hwnd, GWLP_WNDPROC);
        SetWindowLongPtrW(g_Hwnd, GWLP_WNDPROC, (LONG_PTR)hkWndProc);

        init = true;
    }

    // New frame
    ImGui_ImplDX11_NewFrame();
    ImGui_ImplWin32_NewFrame();
    ImGui::NewFrame();

    // Render DeftHack GUI
    DeftHackGUI::Render();

    ImGui::Render();

    // RTV может быть инвалиден — проверяем
    if (!g_RTV)
    {
        ID3D11Texture2D* bb = nullptr;
        swap->GetBuffer(0, __uuidof(ID3D11Texture2D), (void**)&bb);
        g_Device->CreateRenderTargetView(bb, nullptr, &g_RTV);
        bb->Release();
    }

    g_Context->OMSetRenderTargets(1, &g_RTV, nullptr);
    ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());

    return oPresent(swap, sync, flags);
}

// ================= INIT =================
DWORD WINAPI Init(LPVOID)
{
    // Wait for game to initialize
    Sleep(3000);

    // Initialize Kiero with D3D11
    while (kiero::init(kiero::RenderType::D3D11) != kiero::Status::Success)
        Sleep(200);

    // Hook Present (index 8 for IDXGISwapChain::Present)
    kiero::bind(8, (void**)&oPresent, hkPresent);
    
    return 0;
}

// ================= CLEANUP =================
void Cleanup()
{
    if (g_OrigWndProc && g_Hwnd)
        SetWindowLongPtrW(g_Hwnd, GWLP_WNDPROC, (LONG_PTR)g_OrigWndProc);

    ImGui_ImplDX11_Shutdown();
    ImGui_ImplWin32_Shutdown();
    ImGui::DestroyContext();

    if (g_RTV) g_RTV->Release();
    if (g_Context) g_Context->Release();
    if (g_Device) g_Device->Release();

    kiero::shutdown();
}

// ================= DLLMAIN =================
BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        DisableThreadLibraryCalls(hModule);
        CreateThread(nullptr, 0, Init, nullptr, 0, nullptr);
    }
    else if (reason == DLL_PROCESS_DETACH)
    {
        Cleanup();
    }
    return TRUE;
}

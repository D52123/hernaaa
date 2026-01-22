#include <Windows.h>
#include <d3d11.h>
#include <dxgi.h>

#include "kiero.h"
#include "imgui.h"
#include "imgui_impl_dx11.h"
#include "imgui_impl_win32.h"

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")

// ================= GLOBALS =================
static ID3D11Device* g_Device = nullptr;
static ID3D11DeviceContext* g_Context = nullptr;
static ID3D11RenderTargetView* g_RTV = nullptr;
static HWND                     g_Hwnd = nullptr;
static bool                     g_ShowMenu = true;

using PresentFn = HRESULT(__stdcall*)(IDXGISwapChain*, UINT, UINT);
static PresentFn oPresent = nullptr;

static WNDPROC g_OrigWndProc = nullptr;

// ================= WNDPROC =================
LRESULT CALLBACK hkWndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    if (g_ShowMenu && ImGui_ImplWin32_WndProcHandler(hWnd, msg, wParam, lParam))
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
        ImGui::StyleColorsDark();
        ImGui_ImplWin32_Init(g_Hwnd);
        ImGui_ImplDX11_Init(g_Device, g_Context);

        // WndProc (только если ещё не хукнут)
        g_OrigWndProc = (WNDPROC)GetWindowLongPtrW(g_Hwnd, GWLP_WNDPROC);
        SetWindowLongPtrW(g_Hwnd, GWLP_WNDPROC, (LONG_PTR)hkWndProc);

        init = true;
    }

    // Toggle menu
    if (GetAsyncKeyState(VK_INSERT) & 1)
        g_ShowMenu = !g_ShowMenu;

    // New frame
    ImGui_ImplDX11_NewFrame();
    ImGui_ImplWin32_NewFrame();
    ImGui::NewFrame();

    if (g_ShowMenu)
    {
        ImGui::Begin("Unturned GUI", nullptr, ImGuiWindowFlags_NoCollapse);
        ImGui::Text("ImGui works");
        ImGui::Separator();
        ImGui::Text("FPS: %.1f", ImGui::GetIO().Framerate);
        ImGui::End();
    }

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
    while (kiero::init(kiero::RenderType::D3D11) != kiero::Status::Success)
        Sleep(200);

    kiero::bind(8, (void**)&oPresent, hkPresent);
    return 0;
}

// ================= DLLMAIN =================
BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        DisableThreadLibraryCalls(hModule);
        CreateThread(nullptr, 0, Init, nullptr, 0, nullptr);
    }
    return TRUE;
}

// imgui_unturned_fix_v4.cpp - всё инициализировано, компилятор можешь идти лесом
#include <windows.h>
#include <d3d11.h>
#include <dxgi.h>

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")

static ID3D11Device* g_pd3dDevice = nullptr;
static ID3D11DeviceContext* g_pd3dDeviceContext = nullptr;
static ID3D11RenderTargetView* g_mainRenderTargetView = nullptr;
static HWND g_hwnd = nullptr;

typedef HRESULT(__stdcall* Present_t)(IDXGISwapChain*, UINT, UINT);
static Present_t oPresent = nullptr;

static HRESULT __stdcall hkPresent(IDXGISwapChain* pSwapChain, UINT SyncInterval, UINT Flags) {
    if (!g_pd3dDevice && pSwapChain) {
        if (SUCCEEDED(pSwapChain->GetDevice(__uuidof(ID3D11Device), (void**)&g_pd3dDevice))) {
            g_pd3dDevice->GetImmediateContext(&g_pd3dDeviceContext);

            ID3D11Texture2D* pBackBuffer = nullptr;
            if (SUCCEEDED(pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (void**)&pBackBuffer)) && pBackBuffer) {
                g_pd3dDevice->CreateRenderTargetView(pBackBuffer, nullptr, &g_mainRenderTargetView);
                pBackBuffer->Release();
            }
        }
    }

    if (g_mainRenderTargetView && g_pd3dDeviceContext) {
        float clearColor[4] = { 1.0f, 0.0f, 0.0f, 1.0f };
        g_pd3dDeviceContext->ClearRenderTargetView(g_mainRenderTargetView, clearColor);
    }

    return oPresent(pSwapChain, SyncInterval, Flags);
}

static void HookPresent() {
    g_hwnd = FindWindowA(nullptr, "Unturned");
    if (!g_hwnd) g_hwnd = GetForegroundWindow();
    if (!g_hwnd) return;

    IDXGISwapChain* pTempSwapChain = nullptr;
    D3D_FEATURE_LEVEL featureLevel = D3D_FEATURE_LEVEL_11_0;
    DXGI_SWAP_CHAIN_DESC sd = {};
    sd.BufferCount = 1;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    sd.OutputWindow = g_hwnd;
    sd.SampleDesc.Count = 1;
    sd.Windowed = TRUE;

    ID3D11Device* pTempDevice = nullptr;
    ID3D11DeviceContext* pTempContext = nullptr;

    if (SUCCEEDED(D3D11CreateDeviceAndSwapChain(
        nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, 0, nullptr, 0,
        D3D11_SDK_VERSION, &sd, &pTempSwapChain, &pTempDevice, &featureLevel, &pTempContext))) {

        void** pVTable = *(void***)(pTempSwapChain);
        oPresent = (Present_t)pVTable[8];

        DWORD oldProtect = 0;
        VirtualProtect(&pVTable[8], sizeof(void*), PAGE_EXECUTE_READWRITE, &oldProtect);
        pVTable[8] = (void*)hkPresent;
        VirtualProtect(&pVTable[8], sizeof(void*), oldProtect, &oldProtect);

        pTempSwapChain->Release();
        pTempDevice->Release();
        pTempContext->Release();
    }
}

static BOOL APIENTRY DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpReserved) {
    if (dwReason == DLL_PROCESS_ATTACH) {
        DisableThreadLibraryCalls(hModule);
        HookPresent();
    }
    return TRUE;
}
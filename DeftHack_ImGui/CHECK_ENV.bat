@echo off
chcp 65001 >nul
title Проверка окружения для компиляции ImGui

echo ╔══════════════════════════════════════════════════════════════╗
echo ║         ПРОВЕРКА ОКРУЖЕНИЯ ДЛЯ КОМПИЛЯЦИИ IMGUI             ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

echo === 1. Проверка компиляторов ===
echo.

REM Проверка Visual Studio
set VS_FOUND=0
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" (
    echo [✓] Visual Studio 2022 Community найден
    set VS_FOUND=1
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat" (
    echo [✓] Visual Studio 2022 Professional найден
    set VS_FOUND=1
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat" (
    echo [✓] Visual Studio 2019 Community найден
    set VS_FOUND=1
) else (
    echo [✗] Visual Studio НЕ НАЙДЕН
    echo     Установите Visual Studio 2022 Community (бесплатно)
    echo     https://visualstudio.microsoft.com/downloads/
)

echo.

REM Проверка cl.exe
where cl >nul 2>&1
if errorlevel 1 (
    echo [✗] cl.exe не найден в PATH
    if %VS_FOUND%==1 (
        echo     Visual Studio установлен, но не в PATH
        echo     Запустите "Developer Command Prompt for VS"
    )
) else (
    echo [✓] cl.exe найден
    for /f "tokens=*" %%i in ('cl 2^>^&1 ^| findstr "Version"') do echo     %%i
)

echo.

REM Проверка CMake
where cmake >nul 2>&1
if errorlevel 1 (
    echo [✗] cmake не найден
    echo     Установите CMake: https://cmake.org/download/
) else (
    echo [✓] cmake найден
    for /f "tokens=*" %%i in ('cmake --version ^| findstr "version"') do echo     %%i
)

echo.

REM Проверка MinGW
where g++ >nul 2>&1
if errorlevel 1 (
    echo [✗] g++ не найден - MinGW не установлен
    echo     Альтернатива Visual Studio: https://www.mingw-w64.org/
) else (
    echo [✓] g++ найден (MinGW)
    for /f "tokens=*" %%i in ('g++ --version ^| findstr "g++"') do echo     %%i
)

echo.
echo ═══════════════════════════════════════════════════════════════
echo.

echo === 2. Проверка файлов проекта ===
echo.

if exist "imgui\imgui.cpp" (
    echo [✓] ImGui библиотека найдена
) else (
    echo [✗] ImGui НЕ НАЙДЕН
    echo     Скачайте: https://github.com/ocornut/imgui
    echo     Распакуйте в папку: DeftHack_ImGui\imgui\
)

if exist "imgui\backends\imgui_impl_dx11.cpp" (
    echo [✓] ImGui DX11 backend найден
) else (
    echo [✗] ImGui backends не найдены
)

if exist "main.cpp" (
    echo [✓] main.cpp найден
) else (
    echo [✗] main.cpp НЕ НАЙДЕН
)

if exist "renderer.hpp" (
    echo [✓] renderer.hpp найден
) else (
    echo [✗] renderer.hpp НЕ НАЙДЕН
)

if exist "deffhack_gui.hpp" (
    echo [✓] deffhack_gui.hpp найден
) else (
    echo [✗] deffhack_gui.hpp НЕ НАЙДЕН
)

echo.
echo ═══════════════════════════════════════════════════════════════
echo.

echo === 3. Проверка скомпилированных файлов ===
echo.

if exist "build\DeftHack_ImGui.dll" (
    echo [✓] DLL найдена: build\DeftHack_ImGui.dll
    for %%F in (build\DeftHack_ImGui.dll) do echo     Размер: %%~zF байт
) else (
    echo [✗] DLL не найдена в build\
)

if exist "DeftHack_ImGui.dll" (
    echo [✓] DLL найдена в корне: DeftHack_ImGui.dll
    for %%F in (DeftHack_ImGui.dll) do echo     Размер: %%~zF байт
) else (
    echo [✗] DLL не найдена в корне
)

if exist "..\DeftHack_ImGui.dll" (
    echo [✓] DLL найдена в корне проекта
    for %%F in (..\DeftHack_ImGui.dll) do echo     Размер: %%~zF байт
) else (
    echo [✗] DLL не найдена в корне проекта
)

echo.
echo ═══════════════════════════════════════════════════════════════
echo.

echo === ИТОГ ===
echo.

if %VS_FOUND%==1 (
    if exist "imgui\imgui.cpp" (
        echo [✓] ВСЕ ГОТОВО ДЛЯ КОМПИЛЯЦИИ!
        echo.
        echo Запустите: QuickCompile.bat
        echo Или из корня: COMPILE_IMGUI.bat
    ) else (
        echo [!] Visual Studio найден, но ImGui отсутствует
        echo     Скачайте ImGui и повторите проверку
    )
) else (
    echo [!] ТРЕБУЕТСЯ УСТАНОВКА VISUAL STUDIO
    echo.
    echo Шаги:
    echo 1. Скачайте Visual Studio 2022 Community (бесплатно)
    echo    https://visualstudio.microsoft.com/downloads/
    echo.
    echo 2. При установке выберите:
    echo    - Desktop development with C++
    echo    - Windows 10 SDK
    echo    - MSVC v143 build tools
    echo.
    echo 3. После установки запустите этот скрипт снова
)

echo.
echo ═══════════════════════════════════════════════════════════════
echo.
pause

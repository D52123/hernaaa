@echo off
chcp 65001 >nul
title Компиляция DeftHack ImGui с Kiero + MinHook

echo ╔══════════════════════════════════════════════════════════════╗
echo ║         КОМПИЛЯЦИЯ DEFTHACK IMGUI (KIERO + MINHOOK)         ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

REM Проверка MinHook
if not exist "kiero-1.2.12\minhook\src\hook.c" (
    echo [✗] MinHook не найден!
    echo.
    echo Запустите сначала: SETUP_MINHOOK.bat
    echo.
    pause
    exit /b 1
)

echo [✓] MinHook найден
echo.

REM Проверка Visual Studio
set VS_PATH=
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" (
    set VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat
    echo [✓] Visual Studio 2022 Community найден
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat" (
    set VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat
    echo [✓] Visual Studio 2022 Professional найден
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat" (
    set VS_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat
    echo [✓] Visual Studio 2019 Community найден
) else (
    echo [✗] Visual Studio не найден!
    echo.
    echo Установите Visual Studio 2022 Community:
    echo https://visualstudio.microsoft.com/downloads/
    echo.
    pause
    exit /b 1
)

echo.
echo [INFO] Создание build директории...
if not exist "build" mkdir build
cd build

echo.
echo [INFO] Инициализация компилятора...
call "%VS_PATH%" >nul 2>&1

echo [✓] Компилятор готов
echo.
echo [INFO] Компиляция DeftHack_ImGui.dll с Kiero + MinHook...
echo.

REM Компиляция всех файлов
cl /LD /O2 /EHsc /std:c++17 ^
   /I".." ^
   /I"..\imgui" ^
   /I"..\imgui\backends" ^
   /I"..\kiero-1.2.12" ^
   /I"..\kiero-1.2.12\minhook\include" ^
   /D"WIN32" /D"_WINDOWS" /D"_USRDLL" /D"WIN64" /D"_WIN64" ^
   /W3 ^
   "..\main.cpp" ^
   "..\imgui\imgui.cpp" ^
   "..\imgui\imgui_draw.cpp" ^
   "..\imgui\imgui_tables.cpp" ^
   "..\imgui\imgui_widgets.cpp" ^
   "..\imgui\backends\imgui_impl_dx11.cpp" ^
   "..\imgui\backends\imgui_impl_win32.cpp" ^
   "..\kiero-1.2.12\kiero.cpp" ^
   "..\kiero-1.2.12\minhook\src\buffer.c" ^
   "..\kiero-1.2.12\minhook\src\hook.c" ^
   "..\kiero-1.2.12\minhook\src\trampoline.c" ^
   /link /OUT:"version.dll" ^
   d3d11.lib dxgi.lib user32.lib gdi32.lib kernel32.lib ^
   /SUBSYSTEM:WINDOWS /MACHINE:X64 2>&1

if errorlevel 1 (
    echo.
    echo [✗] Компиляция не удалась!
    echo.
    cd ..
    pause
    exit /b 1
)

echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║                    ✅ КОМПИЛЯЦИЯ УСПЕШНА!                   ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo [✓] DLL создана: build\version.dll
echo.

REM Копируем в корень
copy /Y "version.dll" "..\version.dll" >nul 2>&1
if exist "..\version.dll" (
    echo [✓] DLL скопирована в корень проекта
)

copy /Y "version.dll" "..\..\version.dll" >nul 2>&1
if exist "..\..\version.dll" (
    echo [✓] DLL скопирована в корень репозитория
)

echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║                    📦 ГОТОВО К ИНЖЕКЦИИ!                    ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo Использование:
echo 1. Запустите Unturned
echo 2. Инжектируйте version.dll через GH Injector
echo 3. Нажмите INSERT в игре для открытия меню
echo.

cd ..
pause

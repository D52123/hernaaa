@echo off
chcp 65001 >nul
title DeftHack ImGui - Компиляция с Kiero + MinHook

echo ╔══════════════════════════════════════════════════════════════╗
echo ║     🎨 DEFTHACK IMGUI - КОМПИЛЯЦИЯ С KIERO + MINHOOK        ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

REM Проверяем Visual Studio
set "VS_PATH="
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"
    echo [✓] Visual Studio 2022 Community
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat"
    echo [✓] Visual Studio 2022 Professional
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat" (
    set "VS_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat"
    echo [✓] Visual Studio 2019 Community
) else (
    echo [✗] Visual Studio не найден!
    echo.
    echo Установите Visual Studio 2019/2022 Community
    echo https://visualstudio.microsoft.com/downloads/
    pause
    exit /b 1
)

REM Проверяем MinHook
if not exist "minhook\include\MinHook.h" (
    echo [✗] MinHook не найден!
    echo.
    echo Запустите: SETUP_MINHOOK.bat
    pause
    exit /b 1
)
echo [✓] MinHook найден

REM Проверяем ImGui
if not exist "imgui\imgui.cpp" (
    echo [✗] ImGui не найден!
    echo.
    echo Скачайте ImGui: https://github.com/ocornut/imgui
    pause
    exit /b 1
)
echo [✓] ImGui найден

REM Проверяем Kiero
if not exist "kiero-1.2.12\kiero.cpp" (
    echo [✗] Kiero не найден!
    pause
    exit /b 1
)
echo [✓] Kiero найден
echo.

REM Создаем папку сборки
if not exist "build" mkdir build
cd build

echo [INFO] Инициализация компилятора...
call "%VS_PATH%" >nul 2>&1

if errorlevel 1 (
    echo [✗] Ошибка инициализации компилятора
    pause
    exit /b 1
)

echo [✓] Компилятор готов
echo.
echo [INFO] Компиляция DeftHack_ImGui.dll с Kiero + MinHook...
echo.

REM Компиляция с Kiero и MinHook
cl /LD /O2 /EHsc /std:c++17 ^
   /I"..\imgui" ^
   /I"..\imgui\backends" ^
   /I"..\kiero-1.2.12" ^
   /I"..\minhook\include" ^
   /I"..\minhook\src" ^
   /I".." ^
   /D"WIN32" /D"_WINDOWS" /D"_USRDLL" ^
   /D"WIN64" /D"_WIN64" ^
   /D"KIERO_USE_MINHOOK=1" ^
   /W3 ^
   "..\main.cpp" ^
   "..\imgui\imgui.cpp" ^
   "..\imgui\imgui_draw.cpp" ^
   "..\imgui\imgui_tables.cpp" ^
   "..\imgui\imgui_widgets.cpp" ^
   "..\imgui\backends\imgui_impl_dx11.cpp" ^
   "..\imgui\backends\imgui_impl_win32.cpp" ^
   "..\kiero-1.2.12\kiero.cpp" ^
   "..\minhook\src\buffer.c" ^
   "..\minhook\src\hook.c" ^
   "..\minhook\src\trampoline.c" ^
   "..\minhook\src\hde\hde64.c" ^
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

REM Проверяем размер
for %%F in (version.dll) do set SIZE=%%~zF
echo [✓] DLL создана: build\version.dll
echo [INFO] Размер: %SIZE% байт
echo.

REM Копируем в корень
copy /Y version.dll ..\version.dll >nul 2>&1
if exist "..\version.dll" (
    echo [✓] DLL скопирована в корень проекта
)

echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║                  📦 ГОТОВО К ИНЖЕКЦИИ!                      ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo Файл: version.dll
echo.
echo Использование:
echo 1. Запустите Unturned
echo 2. Инжектируйте version.dll
echo 3. Нажмите INSERT для открытия меню
echo.

cd ..
pause

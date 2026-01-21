@echo off
chcp 65001 >nul
title DeftHack ImGui - CMake компиляция

cd /d "%~dp0"

echo ╔══════════════════════════════════════════════════════════════╗
echo ║        🎨 DEFTHACK IMGUI - CMAKE КОМПИЛЯЦИЯ                ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

REM Проверяем наличие CMake
where cmake >nul 2>&1
if errorlevel 1 (
    echo ❌ CMake не найден!
    echo.
    echo 💡 Установите CMake:
    echo   1. Скачайте с https://cmake.org/download/
    echo   2. Или установите через Visual Studio Installer
    echo   3. Или используйте QuickCompile.bat (требует Visual Studio)
    echo.
    pause
    exit /b 1
)

echo ✅ CMake найден
echo.

REM Проверяем наличие файлов
if not exist "CMakeLists.txt" (
    echo ❌ CMakeLists.txt не найден!
    pause
    exit /b 1
)

if not exist "imgui\imgui.cpp" (
    echo ❌ ImGui файлы не найдены!
    echo 💡 Скачайте ImGui: https://github.com/ocornut/imgui
    pause
    exit /b 1
)

echo 🔧 Создание build директории...
if not exist "build" mkdir build
cd build

echo.
echo 🔧 Генерация проекта CMake...
cmake .. -G "Visual Studio 17 2022" -A x64
if errorlevel 1 (
    echo ⚠️  Visual Studio 2022 не найден, пробую другие генераторы...
    cmake .. -G "Visual Studio 16 2019" -A x64
    if errorlevel 1 (
        cmake .. -G "MinGW Makefiles"
        if errorlevel 1 (
            echo ❌ Не удалось сгенерировать проект!
            echo 💡 Установите Visual Studio или MinGW
            cd ..
            pause
            exit /b 1
        )
    )
)

echo.
echo 🔧 Компиляция проекта...
cmake --build . --config Release
if errorlevel 1 (
    echo ❌ Ошибка компиляции!
    cd ..
    pause
    exit /b 1
)

echo.
echo ✅ Компиляция успешна!
echo.

REM Ищем скомпилированную DLL
if exist "Release\DeftHack_ImGui.dll" (
    copy /Y "Release\DeftHack_ImGui.dll" "DeftHack_ImGui.dll" >nul 2>&1
    echo ✅ DLL найдена: Release\DeftHack_ImGui.dll
) else if exist "DeftHack_ImGui.dll" (
    echo ✅ DLL найдена: DeftHack_ImGui.dll
) else (
    echo ⚠️  DLL не найдена в ожидаемых местах
    echo 💡 Проверьте папку build вручную
)

REM Копируем в корень проекта
if exist "DeftHack_ImGui.dll" (
    copy /Y "DeftHack_ImGui.dll" "..\DeftHack_ImGui.dll" >nul 2>&1
    if exist "..\DeftHack_ImGui.dll" (
        echo ✅ DLL скопирована в корень проекта
    )
)

cd ..
echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║                    ✅ ГОТОВО!                               ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
pause

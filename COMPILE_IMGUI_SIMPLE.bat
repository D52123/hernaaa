@echo off
chcp 65001 >nul
title 🎨 DeftHack - Компиляция ImGui

cd /d "%~dp0"

echo ╔══════════════════════════════════════════════════════════════╗
echo ║        🎨 DEFTHACK IMGUI - БЫСТРАЯ КОМПИЛЯЦИЯ              ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

if not exist "DeftHack_ImGui" (
    echo ❌ Папка DeftHack_ImGui не найдена!
    pause
    exit /b 1
)

cd DeftHack_ImGui

REM Проверяем наличие уже скомпилированной DLL
if exist "build\DeftHack_ImGui.dll" (
    echo ✅ ImGui DLL уже скомпилирована!
    echo 📁 Путь: DeftHack_ImGui\build\DeftHack_ImGui.dll
    echo.
    echo Копирую в корень проекта...
    copy /Y "build\DeftHack_ImGui.dll" "..\DeftHack_ImGui.dll" >nul 2>&1
    if exist "..\DeftHack_ImGui.dll" (
        echo ✅ DLL скопирована в корень проекта
    )
    cd ..
    pause
    exit /b 0
)

echo 🔍 Поиск способа компиляции...
echo.

REM Пробуем CMake
where cmake >nul 2>&1
if not errorlevel 1 (
    if exist "CMakeLists.txt" (
        echo ✅ Найден CMake, использую его...
        echo.
        call COMPILE_CMAKE.bat
        cd ..
        exit /b %errorlevel%
    )
)

REM Пробуем Visual Studio
if exist "QuickCompile.bat" (
    echo ✅ Найден QuickCompile.bat, использую Visual Studio...
    echo.
    call QuickCompile.bat
    cd ..
    exit /b %errorlevel%
)

echo ❌ Не найден способ компиляции!
echo.
echo 💡 Установите один из вариантов:
echo   1. Visual Studio 2019/2022 с компонентом "Desktop development with C++"
echo   2. CMake (https://cmake.org/download/)
echo.
echo 💡 После установки запустите этот батник снова
echo.
cd ..
pause
exit /b 1

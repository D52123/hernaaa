@echo off
chcp 65001 >nul
title 🎨 DeftHack ImGui - Компиляция GUI

cd /d "%~dp0\DeftHack_ImGui"

echo ╔══════════════════════════════════════════════════════════════╗
echo ║         🎨 DEFTHACK IMGUI - КОМПИЛЯЦИЯ GUI                  ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

if exist "QuickCompile.bat" (
    call QuickCompile.bat
    if exist "build\DeftHack_ImGui.dll" (
        copy /Y "build\DeftHack_ImGui.dll" "..\DeftHack_ImGui.dll" >nul 2>&1
        if exist "..\DeftHack_ImGui.dll" (
            echo.
            echo ✅ DLL скопирована в корень проекта!
            echo 📁 Путь: %~dp0DeftHack_ImGui.dll
        )
    )
) else (
    echo ❌ QuickCompile.bat не найден!
    echo.
    echo 💡 Используйте Visual Studio:
    echo   1. Откройте Visual Studio
    echo   2. File -^> Open -^> CMake...
    echo   3. Выберите CMakeLists.txt
    echo   4. Build -^> Build All
)

cd ..
pause

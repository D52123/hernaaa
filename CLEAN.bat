@echo off
chcp 65001 >nul
title 🧹 DeftHack - Очистка

cd /d "%~dp0"

echo ╔══════════════════════════════════════════════════════════════╗
echo ║              🧹 DEFTHACK - ОЧИСТКА                          ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

echo 🗑️  Удаление временных файлов...
echo ═══════════════════════════════════════════════════════════════

REM Очистка bin и obj
if exist "bin" (
    echo Удаление bin...
    rmdir /s /q "bin" 2>nul
    echo ✅ bin удалена
)

if exist "obj" (
    echo Удаление obj...
    rmdir /s /q "obj" 2>nul
    echo ✅ obj удалена
)

REM Очистка build папок ImGui
if exist "DeftHack_ImGui\build" (
    echo Удаление DeftHack_ImGui\build...
    rmdir /s /q "DeftHack_ImGui\build" 2>nul
    echo ✅ build удалена
)

if exist "DeftHack_ImGui\out" (
    echo Удаление DeftHack_ImGui\out...
    rmdir /s /q "DeftHack_ImGui\out" 2>nul
    echo ✅ out удалена
)

REM Удаление временных DLL (оставляем только готовые)
if exist "DeftHack_ImGui\DeftHack_ImGui.dll" (
    echo Удаление временной DLL из папки ImGui...
    del /q "DeftHack_ImGui\DeftHack_ImGui.dll" 2>nul
)

echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║                    ✅ ОЧИСТКА ЗАВЕРШЕНА                      ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo 💡 Для компиляции запустите BUILD.bat
echo.
pause

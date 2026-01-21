@echo off
chcp 65001 >nul
title DeftHack ImGui - Полная компиляция

echo ========================================
echo  DeftHack ImGui - ПОЛНАЯ КОМПИЛЯЦИЯ
echo ========================================
echo.

REM Шаг 1: Установка ImGui
echo [1/2] Проверка и установка ImGui...
call setup_imgui.bat
if errorlevel 1 (
    echo [ERROR] Не удалось установить ImGui
    pause
    exit /b 1
)

echo.
echo [2/2] Компиляция проекта...
call QuickCompile.bat
if errorlevel 1 (
    echo [ERROR] Компиляция не удалась
    pause
    exit /b 1
)

echo.
echo ========================================
echo  ✅ ВСЁ ГОТОВО!
echo ========================================
echo.
echo DeftHack_ImGui.dll создан и готов к инжекции!
echo.
pause

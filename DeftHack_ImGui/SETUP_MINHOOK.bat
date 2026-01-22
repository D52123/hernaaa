@echo off
chcp 65001 >nul
title Установка MinHook для Kiero

echo ╔══════════════════════════════════════════════════════════════╗
echo ║              УСТАНОВКА MINHOOK ДЛЯ KIERO                    ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

REM Проверяем наличие git
where git >nul 2>&1
if errorlevel 1 (
    echo [✗] Git не найден!
    echo.
    echo Установите Git:
    echo https://git-scm.com/download/win
    echo.
    echo Или скачайте MinHook вручную:
    echo https://github.com/TsudaKageyu/minhook/archive/refs/heads/master.zip
    echo.
    echo Распакуйте в: kiero-1.2.12\minhook\
    echo.
    pause
    exit /b 1
)

echo [✓] Git найден
echo.

REM Проверяем наличие MinHook
if exist "kiero-1.2.12\minhook\src\hook.c" (
    echo [✓] MinHook уже установлен!
    echo.
    echo Файлы найдены:
    dir /b "kiero-1.2.12\minhook\src\*.c" 2>nul
    echo.
    pause
    exit /b 0
)

echo [INFO] Клонирование MinHook из GitHub...
echo.

REM Удаляем старую папку если есть
if exist "kiero-1.2.12\minhook" (
    rmdir /s /q "kiero-1.2.12\minhook" 2>nul
)

REM Клонируем MinHook
git clone --depth 1 https://github.com/TsudaKageyu/minhook.git "kiero-1.2.12\minhook"

if errorlevel 1 (
    echo.
    echo [✗] Ошибка клонирования!
    echo.
    echo Попробуйте скачать вручную:
    echo https://github.com/TsudaKageyu/minhook/archive/refs/heads/master.zip
    echo.
    pause
    exit /b 1
)

echo.
echo [✓] MinHook успешно установлен!
echo.

REM Проверяем файлы
if exist "kiero-1.2.12\minhook\src\hook.c" (
    echo [✓] Проверка файлов:
    echo.
    dir /b "kiero-1.2.12\minhook\src\*.c"
    dir /b "kiero-1.2.12\minhook\include\*.h"
    echo.
    echo ╔══════════════════════════════════════════════════════════════╗
    echo ║                    ✅ ГОТОВО!                               ║
    echo ╚══════════════════════════════════════════════════════════════╝
    echo.
    echo Теперь можно компилировать проект:
    echo   COMPILE_CMAKE.bat
    echo   или
    echo   QuickCompile.bat
    echo.
) else (
    echo [✗] Файлы не найдены после клонирования!
    echo.
)

pause

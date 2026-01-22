@echo off
chcp 65001 >nul
title Обновление GitHub репозитория

echo ╔══════════════════════════════════════════════════════════════╗
echo ║              ОБНОВЛЕНИЕ GITHUB РЕПОЗИТОРИЯ                   ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

REM Проверка git
where git >nul 2>&1
if errorlevel 1 (
    echo [✗] Git не найден!
    echo.
    echo Установите Git: https://git-scm.com/download/win
    echo.
    pause
    exit /b 1
)

echo [✓] Git найден
echo.

REM Проверка репозитория
if not exist ".git" (
    echo [✗] Это не Git репозиторий!
    echo.
    echo Инициализируйте репозиторий:
    echo   git init
    echo   git remote add origin https://github.com/hernaa/DeftHack-Unturned.git
    echo.
    pause
    exit /b 1
)

echo [✓] Git репозиторий найден
echo.

echo ═══════════════════════════════════════════════════════════════
echo  ИЗМЕНЕНИЯ В ЭТОМ ОБНОВЛЕНИИ:
echo ═══════════════════════════════════════════════════════════════
echo.
echo ✅ Интеграция Kiero + MinHook для стабильного хука DX11
echo ✅ Обновлен kiero.h (D3D11=1, MinHook=1)
echo ✅ Обновлен main.cpp с интеграцией Kiero
echo ✅ Обновлен CMakeLists.txt с поддержкой MinHook
echo ✅ Добавлен SETUP_MINHOOK.bat для установки MinHook
echo ✅ Добавлен COMPILE_WITH_KIERO.bat для компиляции
echo ✅ Добавлена ИНСТРУКЦИЯ_KIERO.md
echo ✅ Обновлен README.md с новой инструкцией
echo ✅ Добавлен CHANGELOG_KIERO.md
echo.
echo ═══════════════════════════════════════════════════════════════
echo.

echo [INFO] Проверка статуса...
git status
echo.

echo ═══════════════════════════════════════════════════════════════
echo.
set /p CONFIRM="Продолжить коммит и push? (Y/N): "
if /i not "%CONFIRM%"=="Y" (
    echo.
    echo [INFO] Отменено пользователем
    pause
    exit /b 0
)

echo.
echo [INFO] Добавление файлов...
git add .

echo.
echo [INFO] Создание коммита...
git commit -m "🎉 Update: Kiero + MinHook integration

✨ Новые возможности:
- Интеграция Kiero для стабильного хука DX11
- Интеграция MinHook для надежного хука функций
- Автоматическое определение SwapChain
- Улучшенная стабильность и производительность

🔧 Изменения:
- Обновлен kiero.h (KIERO_INCLUDE_D3D11=1, KIERO_USE_MINHOOK=1)
- Обновлен main.cpp с интеграцией Kiero и DeftHackGUI
- Обновлен CMakeLists.txt с поддержкой Kiero и MinHook
- Обновлен README.md с новой инструкцией

📦 Новые файлы:
- SETUP_MINHOOK.bat - скрипт установки MinHook
- COMPILE_WITH_KIERO.bat - скрипт компиляции с Kiero
- ИНСТРУКЦИЯ_KIERO.md - подробная инструкция
- CHANGELOG_KIERO.md - список изменений
- GIT_PUSH_UPDATE.bat - этот скрипт

🐛 Исправления:
- Исправлена ошибка 'Отсутствует исполняемый файл цели'
- Исправлена ошибка 'Значение не может быть неопределенным'
- Улучшена стабильность хука Present
- Исправлены утечки памяти при выгрузке DLL

📋 Требования:
- Visual Studio 2022 Community
- Git (для скачивания MinHook)
- Windows SDK 10.0+

🚀 Быстрый старт:
cd DeftHack_ImGui
SETUP_MINHOOK.bat
COMPILE_WITH_KIERO.bat"

if errorlevel 1 (
    echo.
    echo [✗] Ошибка при создании коммита!
    echo.
    pause
    exit /b 1
)

echo.
echo [✓] Коммит создан
echo.

echo [INFO] Отправка на GitHub...
git push origin main

if errorlevel 1 (
    echo.
    echo [⚠️] Ошибка при push!
    echo.
    echo Возможные причины:
    echo - Неправильный remote URL
    echo - Нет прав доступа
    echo - Нужна аутентификация
    echo.
    echo Попробуйте:
    echo   git remote -v  (проверить remote)
    echo   git push -u origin main  (установить upstream)
    echo.
    pause
    exit /b 1
)

echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║                    ✅ УСПЕШНО ОБНОВЛЕНО!                    ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo [✓] Все изменения отправлены на GitHub
echo.
echo Проверьте репозиторий:
echo https://github.com/hernaa/DeftHack-Unturned
echo.

pause

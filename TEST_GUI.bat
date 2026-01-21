@echo off
chcp 65001 >nul
title 🧪 DeftHack ImGui - Тест GUI

cd /d "%~dp0"

echo ╔══════════════════════════════════════════════════════════════╗
echo ║         🧪 DEFTHACK IMGUI - ТЕСТ GUI                        ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

REM Проверяем наличие DLL
if not exist "DeftHack_ImGui.dll" (
    echo ❌ DeftHack_ImGui.dll не найдена!
    echo.
    echo 💡 Сначала скомпилируйте DLL:
    echo   1. Запустите BUILD.bat
    echo   2. Или скомпилируйте через Visual Studio
    echo.
    pause
    exit /b 1
)

echo ✅ DLL найдена!
for %%A in ("DeftHack_ImGui.dll") do (
    echo    Размер: %%~zA байт
    echo    Дата: %%~tA
)
echo.

REM Проверяем наличие инжектора
if not exist "bin\Release\net48\win-x64\Injector.exe" (
    echo ⚠️ Инжектор не скомпилирован, компилирую...
    dotnet build Injector.csproj -c Release -p:Platform=x64 --verbosity quiet
    if errorlevel 1 (
        echo ❌ Ошибка компиляции инжектора
        pause
        exit /b 1
    )
)

echo ✅ Инжектор готов
echo.
echo ═══════════════════════════════════════════════════════════════
echo   📋 ИНСТРУКЦИЯ ПО ТЕСТИРОВАНИЮ GUI
echo ═══════════════════════════════════════════════════════════════
echo.
echo 1️⃣  Запустите Unturned
echo    • Дождитесь полной загрузки игры
echo    • Войдите в меню или на сервер
echo.
echo 2️⃣  Запустите инжектор от администратора:
echo    • ПКМ на Inject.bat → "Запуск от имени администратора"
echo    • Или запустите: bin\Release\net48\win-x64\Injector.exe
echo.
echo 3️⃣  В игре нажмите INSERT для открытия меню
echo    • Меню должно появиться с вкладками: LEGIT, VISUALS, MISC, SETTINGS
echo    • Попробуйте переключать опции
echo    • Попробуйте перетащить окно
echo.
echo 4️⃣  Проверьте функциональность:
echo    ✅ Меню открывается/закрывается по INSERT
echo    ✅ Вкладки переключаются
echo    ✅ Переключатели работают
echo    ✅ Слайдеры работают
echo    ✅ Кнопки работают
echo    ✅ Окно можно перетаскивать
echo.
echo ⚠️  Если меню не появляется:
echo    • Проверьте что DLL успешно инжектирована (смотрите логи инжектора)
echo    • Попробуйте нажать INSERT несколько раз
echo    • Убедитесь что игра активна (не свернута)
echo    • Проверьте что это не BattlEye сервер (BattlEye может блокировать)
echo.
echo 💡 Горячие клавиши:
echo    • INSERT - Открыть/закрыть меню
echo    • END - Выгрузить читы (в настройках)
echo.
pause

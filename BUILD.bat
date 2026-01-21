@echo off
chcp 65001 >nul
title 🚀 DeftHack - Компиляция

cd /d "%~dp0"

echo ╔══════════════════════════════════════════════════════════════╗
echo ║              🚀 DEFTHACK - КОМПИЛЯЦИЯ                       ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

echo 📦 Шаг 1: Компиляция C# проекта...
echo ═══════════════════════════════════════════════════════════════
dotnet build DeftHack_Modern.csproj -c Release --verbosity minimal
set BUILD_RESULT=%errorlevel%

REM Проверяем наличие DLL в obj\Release\ (даже если копирование в bin\Release\ не удалось)
if exist "obj\Release\UnityEngine.FileSystemModule.dll" (
    echo ✅ C# проект скомпилирован успешно!
    echo 📁 DLL находится в: obj\Release\UnityEngine.FileSystemModule.dll
    echo.
    if %BUILD_RESULT% neq 0 (
        echo ⚠️ Предупреждение: не удалось скопировать DLL в bin\Release\
        echo 💡 Это нормально если Unturned запущен - DLL всё равно готова!
        echo.
    )
) else if %BUILD_RESULT% neq 0 (
    echo ❌ Ошибка компиляции C# проекта
    echo.
    echo ⚠️ Возможно DLL заблокирована процессом Unturned
    echo 💡 Закройте Unturned и попробуйте снова
    pause
    exit /b 1
) else (
    echo ✅ C# проект скомпилирован
)
echo.

echo 📋 Шаг 1.1: Копирование DLL файлов...
echo ═══════════════════════════════════════════════════════════════
if exist "obj\Release\UnityEngine.FileSystemModule.dll" (
    REM Копируем в корень проекта
    copy /Y "obj\Release\UnityEngine.FileSystemModule.dll" "UnityEngine.FileSystemModule.dll" >nul 2>&1
    if %errorlevel% equ 0 (
        echo ✅ DLL скопирована в корень проекта
    ) else (
        echo ⚠️ Не удалось скопировать DLL в корень (возможно заблокирована Unturned)
        echo 💡 Используйте DLL напрямую из obj\Release\UnityEngine.FileSystemModule.dll
    )
    
    REM Пробуем скопировать в bin\Release (может быть заблокировано)
    copy /Y "obj\Release\UnityEngine.FileSystemModule.dll" "bin\Release\UnityEngine.FileSystemModule.dll" >nul 2>&1
    if %errorlevel% equ 0 (
        echo ✅ DLL скопирована в bin\Release
    ) else (
        REM Это нормально если Unturned запущен
        echo ℹ️  DLL не скопирована в bin\Release (Unturned использует файл - это нормально)
    )
    echo.
    echo ✅ Готово! DLL доступна в:
    echo    📁 obj\Release\UnityEngine.FileSystemModule.dll
    if exist "UnityEngine.FileSystemModule.dll" (
        echo    📁 UnityEngine.FileSystemModule.dll (в корне проекта)
    )
) else (
    echo ❌ DLL не найдена в obj\Release\
    echo 💡 Проверьте ошибки компиляции выше
)
echo.

echo 🎨 Шаг 2: Компиляция ImGui GUI...
echo ═══════════════════════════════════════════════════════════════
if exist "DeftHack_ImGui" (
    cd DeftHack_ImGui
    
    REM Проверяем наличие уже скомпилированной DLL
    if exist "build\DeftHack_ImGui.dll" (
        echo ✅ ImGui DLL уже скомпилирована
        copy /Y "build\DeftHack_ImGui.dll" "..\DeftHack_ImGui.dll" >nul 2>&1
        if exist "..\DeftHack_ImGui.dll" (
            echo ✅ ImGui DLL скопирована в корень проекта
        )
        cd ..
        goto :imgui_done
    )
    
    REM Пробуем CMake сначала (если доступен)
    where cmake >nul 2>&1
    if not errorlevel 1 (
        if exist "CMakeLists.txt" (
            echo 🔧 Попытка компиляции через CMake...
            call COMPILE_CMAKE.bat >nul 2>&1
            if exist "build\DeftHack_ImGui.dll" (
                copy /Y "build\DeftHack_ImGui.dll" "..\DeftHack_ImGui.dll" >nul 2>&1
                if exist "..\DeftHack_ImGui.dll" (
                    echo ✅ ImGui DLL скомпилирована через CMake
                    cd ..
                    goto :imgui_done
                )
            )
        )
    )
    
    REM Пробуем QuickCompile.bat (требует Visual Studio)
    if exist "QuickCompile.bat" (
        echo 🔧 Попытка компиляции через Visual Studio...
        call QuickCompile.bat
        if exist "build\DeftHack_ImGui.dll" (
            copy /Y "build\DeftHack_ImGui.dll" "..\DeftHack_ImGui.dll" >nul 2>&1
            if exist "..\DeftHack_ImGui.dll" (
                echo ✅ ImGui DLL скомпилирована и скопирована
            ) else (
                echo ✅ ImGui DLL скомпилирована (build\DeftHack_ImGui.dll)
            )
        ) else (
            echo ⚠️ ImGui DLL не скомпилирована
            echo 💡 Возможные причины:
            echo    - Visual Studio не установлен
            echo    - CMake не установлен
            echo    - Отсутствуют файлы ImGui
            echo    - Ошибка компиляции
            echo.
            echo 💡 Для компиляции ImGui:
            echo    1. Установите Visual Studio 2019/2022 с C++
            echo    2. Или установите CMake
            echo    3. Запустите: DeftHack_ImGui\QuickCompile.bat
            echo    4. Или: DeftHack_ImGui\COMPILE_CMAKE.bat
            echo.
            echo 💡 Основной чит (C# GUI) работает без ImGui!
        )
    ) else (
        echo ⚠️ QuickCompile.bat не найден в DeftHack_ImGui
    )
    :imgui_done
    cd ..
) else (
    echo ⚠️ Папка DeftHack_ImGui не найдена
    echo 💡 ImGui GUI будет недоступен, но основной чит работает
)
echo.

echo 🔧 Шаг 3: Компиляция инжектора...
echo ═══════════════════════════════════════════════════════════════
dotnet build Injector.csproj -c Release -p:Platform=x64 --verbosity minimal
if %errorlevel% neq 0 (
    echo ⚠️ Инжектор не скомпилирован (не критично)
) else (
    echo ✅ Инжектор скомпилирован
)
echo.

echo ╔══════════════════════════════════════════════════════════════╗
echo ║                    ✅ ГОТОВО!                               ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo 📦 ГОТОВЫЕ ФАЙЛЫ:
echo ═══════════════════════════════════════════════════════════════
if exist "bin\Release\net48\win-x64\Injector.exe" (
    echo ✅ Injector.exe: bin\Release\net48\win-x64\Injector.exe
) else (
    echo ❌ Injector.exe не найден
)

if exist "obj\Release\UnityEngine.FileSystemModule.dll" (
    echo ✅ C# DLL: obj\Release\UnityEngine.FileSystemModule.dll
)
if exist "UnityEngine.FileSystemModule.dll" (
    echo ✅ C# DLL: UnityEngine.FileSystemModule.dll (в корне)
)
if exist "bin\Release\UnityEngine.FileSystemModule.dll" (
    echo ✅ C# DLL: bin\Release\UnityEngine.FileSystemModule.dll
)
if exist "DeftHack_ImGui.dll" (
    echo ✅ ImGui DLL: DeftHack_ImGui.dll
)
echo.
echo 🚀 ИНСТРУКЦИЯ:
echo ═══════════════════════════════════════════════════════════════
echo 1. Для инжекции: запустите Inject.bat от администратора
echo 2. Горячие клавиши:
echo    - F1 - Основное меню (MenuComponent)
echo    - INSERT - Современное меню (ImGuiCheat)
echo    - F2 - Тест функциональности
echo    - F3 - Детальный статус
echo.
echo ⚠️ ВАЖНО:
echo    - Если DLL заблокирована Unturned, используйте DLL из obj\Release\
echo    - Запускайте инжектор от администратора для надежности
echo.
pause

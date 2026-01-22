@echo off
chcp 65001 >nul
title DeftHack ImGui - Компиляция и установка
color 0B

cd /d "%~dp0"

echo ========================================
echo  DeftHack ImGui - Компиляция и установка
echo ========================================
echo.

REM Проверяем наличие необходимых файлов
if not exist "main.cpp" (
    echo [ERROR] main.cpp не найден!
    pause
    exit /b 1
)

if not exist "renderer.hpp" (
    echo [ERROR] renderer.hpp не найден!
    pause
    exit /b 1
)

if not exist "deffhack_gui.hpp" (
    echo [ERROR] deffhack_gui.hpp не найден!
    pause
    exit /b 1
)

if not exist "imgui\imgui.cpp" (
    echo [ERROR] ImGui файлы не найдены!
    echo [TIP] Убедитесь что ImGui находится в папке imgui\
    pause
    exit /b 1
)

echo [OK] Все исходные файлы найдены
echo.

REM Пробуем QuickCompile.bat
if exist "QuickCompile.bat" (
    echo [INFO] Используем QuickCompile.bat...
    echo.
    call QuickCompile.bat
    if errorlevel 1 (
        echo [ERROR] QuickCompile.bat завершился с ошибкой!
        goto :try_cmake
    )
    
    REM Проверяем результат
    if exist "build\DeftHack_ImGui.dll" (
        echo.
        echo [OK] DLL скомпилирована: build\DeftHack_ImGui.dll
        goto :copy_files
    )
    if exist "DeftHack_ImGui.dll" (
        echo.
        echo [OK] DLL скомпилирована: DeftHack_ImGui.dll
        goto :copy_files
    )
    
    echo [WARNING] DLL не найдена после компиляции
    goto :try_cmake
)

:try_cmake
echo.
echo [INFO] Пробуем CMake компиляцию...
echo.

if not exist "CMakeLists.txt" (
    echo [ERROR] CMakeLists.txt не найден!
    pause
    exit /b 1
)

REM Создаем папку build
if exist "build" (
    echo [INFO] Очищаем старую папку build...
    rmdir /s /q build 2>nul
)
mkdir build 2>nul
cd build

echo [INFO] Конфигурируем CMake...
cmake .. -G "Visual Studio 17 2022" -A x64 -DCMAKE_BUILD_TYPE=Release
if errorlevel 1 (
    echo [WARNING] Visual Studio 2022 не найден, пробуем другую версию...
    cmake .. -G "Visual Studio 16 2019" -A x64 -DCMAKE_BUILD_TYPE=Release
    if errorlevel 1 (
        echo [ERROR] CMake конфигурация не удалась!
        echo [TIP] Установите Visual Studio 2019/2022 с C++ поддержкой
        cd ..
        pause
        exit /b 1
    )
)

echo.
echo [INFO] Компилируем...
cmake --build . --config Release
if errorlevel 1 (
    echo [ERROR] Компиляция не удалась!
    cd ..
    pause
    exit /b 1
)

cd ..

REM Ищем скомпилированную DLL
if exist "build\Release\DeftHack_ImGui.dll" (
    echo.
    echo [OK] DLL скомпилирована: build\Release\DeftHack_ImGui.dll
    set DLL_SOURCE=build\Release\DeftHack_ImGui.dll
    goto :copy_files
)
if exist "build\DeftHack_ImGui.dll" (
    echo.
    echo [OK] DLL скомпилирована: build\DeftHack_ImGui.dll
    set DLL_SOURCE=build\DeftHack_ImGui.dll
    goto :copy_files
)

echo [ERROR] DLL не найдена после компиляции!
pause
exit /b 1

:copy_files
echo.
echo ========================================
echo  Копирование DLL в нужные места...
echo ========================================
echo.

REM Копируем в корень DeftHack_ImGui
copy /Y "%DLL_SOURCE%" "DeftHack_ImGui.dll" >nul
if exist "DeftHack_ImGui.dll" (
    for %%F in ("DeftHack_ImGui.dll") do (
        echo [OK] Скопировано в DeftHack_ImGui.dll (%%~zF байт)
    )
)

REM Копируем в корень проекта (если папка существует)
if exist "..\DeftHack_ImGui.dll" (
    copy /Y "%DLL_SOURCE%" "..\DeftHack_ImGui.dll" >nul
    echo [OK] Скопировано в корень проекта
) else (
    copy /Y "%DLL_SOURCE%" "..\DeftHack_ImGui.dll" >nul
    if exist "..\DeftHack_ImGui.dll" (
        echo [OK] Скопировано в корень проекта
    )
)

REM Копируем в bin для инжектора
if exist "..\bin" (
    if not exist "..\bin\Release" mkdir "..\bin\Release"
    copy /Y "%DLL_SOURCE%" "..\bin\Release\DeftHack_ImGui.dll" >nul
    if exist "..\bin\Release\DeftHack_ImGui.dll" (
        echo [OK] Скопировано в bin\Release\DeftHack_ImGui.dll
    )
)

echo.
echo ========================================
echo  ✅ ГОТОВО!
echo ========================================
echo.
echo DLL скомпилирована и установлена!
echo Теперь можно использовать Injector.exe для инжекции.
echo.
echo Расположение DLL:
if exist "DeftHack_ImGui.dll" (
    for %%F in ("DeftHack_ImGui.dll") do (
        echo   - %%~fF (%%~zF байт)
    )
)
if exist "..\DeftHack_ImGui.dll" (
    for %%F in ("..\DeftHack_ImGui.dll") do (
        echo   - %%~fF (%%~zF байт)
    )
)
echo.
pause

@echo off
chcp 65001 >nul
echo ========================================
echo  Автоматическая установка ImGui
echo ========================================
echo.

REM Проверяем наличие git
where git >nul 2>&1
if errorlevel 1 (
    echo [WARNING] Git не найден
    echo [INFO] Скачиваю через PowerShell...
    
    powershell -Command "Invoke-WebRequest -Uri 'https://github.com/ocornut/imgui/archive/refs/heads/master.zip' -OutFile 'imgui.zip'"
    
    if exist "imgui.zip" (
        echo [INFO] Распаковка...
        powershell -Command "Expand-Archive -Path 'imgui.zip' -DestinationPath '.' -Force"
        if exist "imgui-master" (
            if exist "imgui" rmdir /s /q imgui
            move /Y "imgui-master" "imgui"
            del /F "imgui.zip"
            echo [OK] ImGui установлен!
        ) else (
            echo [ERROR] Не удалось распаковать
            pause
            exit /b 1
        )
    ) else (
        echo [ERROR] Не удалось скачать ImGui
        echo.
        echo Скачайте вручную:
        echo 1. Перейдите на https://github.com/ocornut/imgui
        echo 2. Code -^> Download ZIP
        echo 3. Распакуйте в DeftHack_ImGui\imgui\
        pause
        exit /b 1
    )
) else (
    echo [INFO] Используется Git для скачивания...
    if exist "imgui" (
        echo [INFO] Папка imgui уже существует
        choice /C YN /M "Обновить ImGui"
        if errorlevel 2 goto :end
        rmdir /s /q imgui
    )
    
    git clone https://github.com/ocornut/imgui.git imgui
    if errorlevel 1 (
        echo [ERROR] Не удалось скачать через Git
        echo Попробую через PowerShell...
        goto :git_failed
    )
    
    echo [OK] ImGui скачан!
)

:end
echo.
echo [INFO] Проверка файлов...
if exist "imgui\imgui.cpp" (
    echo [OK] imgui.cpp найден
) else (
    echo [ERROR] imgui.cpp не найден!
    pause
    exit /b 1
)

REM Проверяем в backends (новая структура ImGui)
set IMPL_PATH=
if exist "imgui\backends\imgui_impl_dx11.cpp" (
    echo [OK] imgui_impl_dx11.cpp найден (backends)
    set IMPL_PATH=imgui\backends
    goto :check_win32
)
if exist "imgui\imgui_impl_dx11.cpp" (
    echo [OK] imgui_impl_dx11.cpp найден (root)
    set IMPL_PATH=imgui
    goto :check_win32
)
echo [ERROR] imgui_impl_dx11.cpp не найден!
echo Ищите в папке imgui\backends\ или imgui\
pause
exit /b 1

:check_win32
if exist "%IMPL_PATH%\imgui_impl_win32.cpp" (
    echo [OK] imgui_impl_win32.cpp найден
) else (
    echo [ERROR] imgui_impl_win32.cpp не найден в %IMPL_PATH%!
    pause
    exit /b 1
)

echo.
echo ========================================
echo  ✅ ImGui установлен и готов!
echo ========================================
echo.
echo Теперь запустите QuickCompile.bat для компиляции
echo.
pause

:git_failed
powershell -Command "Invoke-WebRequest -Uri 'https://github.com/ocornut/imgui/archive/refs/heads/master.zip' -OutFile 'imgui.zip'"
if exist "imgui.zip" (
    powershell -Command "Expand-Archive -Path 'imgui.zip' -DestinationPath '.' -Force"
    if exist "imgui-master" (
        if exist "imgui" rmdir /s /q imgui
        move /Y "imgui-master" "imgui"
        del /F "imgui.zip"
        echo [OK] ImGui установлен через ZIP!
        goto :end
    )
)
echo [ERROR] Не удалось скачать ImGui автоматически
echo Скачайте вручную с https://github.com/ocornut/imgui
pause
exit /b 1

@echo off
echo ========================================
echo  DeftHack ImGui - C++ Compiler
echo ========================================
echo.

REM Проверка наличия Visual Studio
where cl >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Visual Studio не найден!
    echo Пожалуйста, запустите этот скрипт из "Developer Command Prompt for VS"
    echo или установите Visual Studio Build Tools
    pause
    exit /b 1
)

echo [INFO] Найден Visual Studio Compiler
echo.

REM Создаем папку build если её нет
if not exist "build" mkdir build
cd build

echo [INFO] Компиляция DeftHack_ImGui.dll...
echo.

REM Компиляция (упрощенная версия)
REM В реальности нужен полный проект с .vcxproj
echo [WARNING] Используйте Visual Studio Solution для компиляции!
echo.
echo Для компиляции:
echo 1. Откройте Visual Studio
echo 2. File -^> New -^> Project
echo 3. Visual C++ -^> Windows Desktop -^> Dynamic Link Library (DLL)
echo 4. Добавьте все файлы из папки DeftHack_ImGui
echo 5. Build -^> Build Solution
echo.

echo [INFO] Или используйте CMake:
echo   mkdir build
echo   cd build
echo   cmake ..
echo   cmake --build . --config Release
echo.

cd ..
pause

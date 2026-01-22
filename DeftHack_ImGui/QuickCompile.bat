@echo off
chcp 65001 >nul
echo ========================================
echo  DeftHack ImGui - Быстрая компиляция
echo ========================================
echo.

REM Проверяем наличие Visual Studio
set VS_PATH=
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" (
    set VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat
    echo [OK] Найден Visual Studio 2022 Community
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat" (
    set VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat
    echo [OK] Найден Visual Studio 2022 Professional
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat" (
    set VS_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat
    echo [OK] Найден Visual Studio 2019 Community
) else (
    echo [ERROR] Visual Studio не найден!
    echo.
    echo Пожалуйста установите:
    echo - Visual Studio 2019/2022 (Community бесплатно)
    echo - С компонентом "Desktop development with C++"
    echo.
    pause
    exit /b 1
)

echo.
echo [INFO] Загрузка и настройка ImGui...
echo.

REM Проверяем наличие ImGui
if not exist "imgui" (
    echo [WARNING] Папка imgui не найдена!
    echo [INFO] Создаю упрощенную структуру...
    mkdir imgui 2>nul
    
    echo [INFO] ВАЖНО: Нужно скачать ImGui вручную:
    echo   1. Перейдите на https://github.com/ocornut/imgui
    echo   2. Нажмите Code -^> Download ZIP
    echo   3. Распакуйте в папку DeftHack_ImGui\imgui\
    echo.
    echo Или используйте git:
    echo   git clone https://github.com/ocornut/imgui.git imgui
    echo.
    pause
    exit /b 1
)

REM Проверяем наличие ключевых файлов ImGui
if not exist "imgui\imgui.cpp" (
    echo [ERROR] ImGui файлы не найдены!
    echo Скачайте ImGui: https://github.com/ocornut/imgui
    pause
    exit /b 1
)

echo [OK] ImGui найден
echo.

REM Создаем папку для сборки
if not exist "build" mkdir build
cd build

echo [INFO] Инициализация компилятора...
echo.

REM Активируем окружение Visual Studio
call "%VS_PATH%" >nul 2>&1

if errorlevel 1 (
    echo [ERROR] Не удалось инициализировать компилятор
    pause
    exit /b 1
)

echo [OK] Компилятор готов
echo.
echo [INFO] Компиляция DeftHack_ImGui.dll...
echo.

REM Определяем путь к backends
set IMPL_PATH=..\imgui\backends
if not exist "%IMPL_PATH%\imgui_impl_dx11.cpp" (
    set IMPL_PATH=..\imgui
)

REM Проверяем наличие всех необходимых файлов
if not exist "..\main.cpp" (
    echo [ERROR] main.cpp не найден!
    cd ..
    exit /b 1
)

if not exist "..\imgui\imgui.cpp" (
    echo [ERROR] imgui\imgui.cpp не найден!
    echo [INFO] Скачайте ImGui: https://github.com/ocornut/imgui
    cd ..
    exit /b 1
)

REM Проверяем наличие backends
if not exist "%IMPL_PATH%\imgui_impl_dx11.cpp" (
    echo [WARNING] imgui_impl_dx11.cpp не найден в %IMPL_PATH%
    echo [INFO] Ищу в других местах...
    if exist "..\imgui\examples\example_win32_directx11\imgui_impl_dx11.cpp" (
        set IMPL_PATH=..\imgui\examples\example_win32_directx11
        echo [OK] Найдено в примерах
    ) else (
        echo [ERROR] Backends не найдены!
        echo [INFO] Скачайте полную версию ImGui с примерами
        cd ..
        exit /b 1
    )
)

echo [INFO] Компиляция с использованием: %IMPL_PATH%
echo.

REM Упрощенная компиляция одной командой
cl /LD /O2 /EHsc /std:c++17 /I"..\imgui" /I"..\imgui\backends" /I".." /D"WIN32" /D"_WINDOWS" /D"_USRDLL" /D"DEFTHACK_IMGUI_EXPORTS" /D"WIN64" /D"_WIN64" /W3 ^
   "..\main.cpp" ^
   "..\imgui\imgui.cpp" ^
   "..\imgui\imgui_draw.cpp" ^
   "..\imgui\imgui_tables.cpp" ^
   "..\imgui\imgui_widgets.cpp" ^
   "%IMPL_PATH%\imgui_impl_dx11.cpp" ^
   "%IMPL_PATH%\imgui_impl_win32.cpp" ^
   /link /OUT:"DeftHack_ImGui.dll" d3d11.lib dxgi.lib user32.lib gdi32.lib kernel32.lib /SUBSYSTEM:WINDOWS /MACHINE:X64 2>&1

if errorlevel 1 (
    echo.
    echo [ERROR] Компиляция не удалась!
    echo.
    echo Возможные причины:
    echo - Отсутствуют файлы ImGui
    echo - Неправильная версия Visual Studio
    echo - Отсутствуют зависимости
    echo - Неправильные пути к файлам
    echo.
    echo Попробуйте:
    echo 1. Убедитесь что все файлы ImGui на месте
    echo 2. Проверьте что Visual Studio установлен правильно
    echo 3. Создайте проект в Visual Studio вручную
    echo 4. Или используйте CMake: cmake -B build && cmake --build build
    echo.
    cd ..
    exit /b 1
)

echo.
echo ========================================
echo  ✅ КОМПИЛЯЦИЯ УСПЕШНА!
echo ========================================
echo.
echo [OK] DLL создана: build\DeftHack_ImGui.dll
echo.

REM Проверяем размер файла
for %%F in (DeftHack_ImGui.dll) do (
    set SIZE=%%~zF
)
echo [INFO] Размер: %SIZE% байт
echo.

REM Копируем DLL в корень для удобства
copy /Y DeftHack_ImGui.dll ..\DeftHack_ImGui.dll >nul 2>&1
if exist "..\DeftHack_ImGui.dll" (
    echo [OK] DLL скопирована в корень проекта
)

echo.
echo ========================================
echo  📦 ГОТОВО К ИНЖЕКЦИИ!
echo ========================================
echo.
echo Теперь можно инжектить DeftHack_ImGui.dll в игру
echo.
echo Использование:
echo 1. Запустите Unturned
echo 2. Инжектируйте DLL через GH Injector или другой инжектор
echo 3. Нажмите INSERT в игре для открытия меню
echo.

cd ..
pause

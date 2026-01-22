cmake_minimum_required(VERSION 3.10)
project(DeftHack_ImGui LANGUAGES CXX C)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# MinHook исходники
set(MINHOOK_SRC
    kiero-1.2.12/minhook/src/buffer.c
    kiero-1.2.12/minhook/src/hook.c
    kiero-1.2.12/minhook/src/trampoline.c
)

# Проверяем наличие MinHook
if(EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/kiero-1.2.12/minhook/src/hook.c")
    set(MINHOOK_AVAILABLE TRUE)
    message(STATUS "MinHook найден")
else()
    set(MINHOOK_AVAILABLE FALSE)
    message(WARNING "MinHook не найден! Запустите SETUP_MINHOOK.bat")
endif()

# Kiero исходники
set(KIERO_SRC
    kiero-1.2.12/kiero.cpp
)

# ImGui исходники
set(IMGUI_SRC
    imgui/imgui.cpp
    imgui/imgui_draw.cpp
    imgui/imgui_tables.cpp
    imgui/imgui_widgets.cpp
    imgui/backends/imgui_impl_dx11.cpp
    imgui/backends/imgui_impl_win32.cpp
)

# Основная библиотека
if(MINHOOK_AVAILABLE)
    add_library(DeftHack_ImGui SHARED
        main.cpp
        ${IMGUI_SRC}
        ${KIERO_SRC}
        ${MINHOOK_SRC}
    )
else()
    add_library(DeftHack_ImGui SHARED
        main.cpp
        ${IMGUI_SRC}
        ${KIERO_SRC}
    )
endif()

# маскировка
set_target_properties(DeftHack_ImGui PROPERTIES
    PREFIX ""
    OUTPUT_NAME "version"
    SUFFIX ".dll"
)

# пути к заголовкам
target_include_directories(DeftHack_ImGui PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}
    ${CMAKE_CURRENT_SOURCE_DIR}/imgui
    ${CMAKE_CURRENT_SOURCE_DIR}/imgui/backends
    ${CMAKE_CURRENT_SOURCE_DIR}/kiero-1.2.12
    ${CMAKE_CURRENT_SOURCE_DIR}/kiero-1.2.12/minhook/include
)

# линкуем
target_link_libraries(DeftHack_ImGui PRIVATE d3d11 dxgi user32 gdi32)
cmake_minimum_required(VERSION 3.10)
project(DeftHack_ImGui LANGUAGES CXX C)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ----------  IMGUI  ----------
set(IMGUI_SRC
    imgui/imgui.cpp
    imgui/imgui_draw.cpp
    imgui/imgui_tables.cpp
    imgui/imgui_widgets.cpp
    imgui/backends/imgui_impl_dx11.cpp
    imgui/backends/imgui_impl_win32.cpp
)

# ----------  KIERO  ----------
set(KIERO_SRC
    kiero-1.2.12/kiero.cpp
)

# ----------  MINHOOK  ----------
# Проверяем наличие MinHook
if(EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/minhook/src/hook.c")
    set(MINHOOK_SRC
        minhook/src/buffer.c
        minhook/src/hook.c
        minhook/src/trampoline.c
    )
    # Добавляем hde64.c для x64
    if(CMAKE_SIZEOF_VOID_P EQUAL 8)
        list(APPEND MINHOOK_SRC minhook/src/hde/hde64.c)
    else()
        list(APPEND MINHOOK_SRC minhook/src/hde/hde32.c)
    endif()
    set(MINHOOK_FOUND TRUE)
    message(STATUS "MinHook найден")
else()
    set(MINHOOK_FOUND FALSE)
    message(WARNING "MinHook не найден! Запустите SETUP_MINHOOK.bat")
endif()

# ----------  DLL  ----------
if(MINHOOK_FOUND)
    add_library(DeftHack_ImGui SHARED
        main.cpp
        ${IMGUI_SRC}
        ${KIERO_SRC}
        ${MINHOOK_SRC}
    )
else()
    # Компилируем без MinHook (не будет работать!)
    add_library(DeftHack_ImGui SHARED
        main.cpp
        ${IMGUI_SRC}
        ${KIERO_SRC}
    )
endif()

target_include_directories(DeftHack_ImGui PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}
    ${CMAKE_CURRENT_SOURCE_DIR}/imgui
    ${CMAKE_CURRENT_SOURCE_DIR}/imgui/backends
    ${CMAKE_CURRENT_SOURCE_DIR}/kiero-1.2.12
)

if(MINHOOK_FOUND)
    target_include_directories(DeftHack_ImGui PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/minhook/include
        ${CMAKE_CURRENT_SOURCE_DIR}/minhook/src
    )
endif()

target_link_libraries(DeftHack_ImGui PRIVATE d3d11 dxgi user32 gdi32)

set_target_properties(DeftHack_ImGui PROPERTIES
    PREFIX ""
    OUTPUT_NAME "version"
    SUFFIX ".dll"
)
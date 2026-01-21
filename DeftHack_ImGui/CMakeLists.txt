cmake_minimum_required(VERSION 3.15)
project(DeftHack_ImGui)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ImGui
include_directories(imgui)
include_directories(imgui/backends)
include_directories(${CMAKE_CURRENT_SOURCE_DIR})

# Source files - только основные файлы ImGui (без примеров)
file(GLOB IMGUI_CORE_SOURCES 
    "imgui/imgui.cpp"
    "imgui/imgui_demo.cpp"
    "imgui/imgui_draw.cpp"
    "imgui/imgui_tables.cpp"
    "imgui/imgui_widgets.cpp"
)

file(GLOB IMGUI_BACKEND_SOURCES
    "imgui/backends/imgui_impl_dx11.cpp"
    "imgui/backends/imgui_impl_win32.cpp"
)

file(GLOB SOURCES
    "*.cpp"
)

# Header files (только для справки, не компилируются отдельно)
file(GLOB HEADERS
    "*.hpp"
)

add_library(DeftHack_ImGui SHARED
    ${SOURCES}
    ${IMGUI_CORE_SOURCES}
    ${IMGUI_BACKEND_SOURCES}
)

# Libraries
target_link_libraries(DeftHack_ImGui
    d3d11
    dxgi
    user32
    gdi32
)

# MinHook (опционально, раскомментируйте если MinHook доступен)
# find_library(MINHOOK_LIB minhook PATHS ${CMAKE_CURRENT_SOURCE_DIR}/libs)
# if(MINHOOK_LIB)
#     target_link_libraries(DeftHack_ImGui ${MINHOOK_LIB})
#     target_compile_definitions(DeftHack_ImGui PRIVATE USE_MINHOOK)
# endif()

# Оптимизации для Release
set_target_properties(DeftHack_ImGui PROPERTIES
    CXX_STANDARD 17
    CXX_STANDARD_REQUIRED ON
)

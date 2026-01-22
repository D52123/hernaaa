#pragma once
#include <imgui.h>
#include <cmath>
#include <algorithm>

namespace DeftHackGUI {
    inline bool open = true;
    inline int tab = 0;
    inline float animation = 0.0f;

    // Переменные для читов
    inline bool aimbot = false;
    inline float fov = 90.f;
    inline float smoothness = 5.0f;
    inline bool esp = false;
    inline bool esp_box = true;
    inline bool esp_skeleton = false;
    inline bool esp_name = true;
    inline bool esp_distance = true;
    inline bool chams = false;
    inline int cham_type = 0;
    inline bool bhop = false;
    inline bool playerFlight = false;
    inline float flightSpeed = 2.0f;
    inline bool nightVision = false;
    inline bool godMode = false;
    inline bool noClip = false;
    inline float speedHack = 1.0f;
    inline ImVec4 esp_color = ImVec4(0.2f, 0.6f, 1.0f, 1.0f);
    inline ImVec4 cham_color = ImVec4(1.0f, 0.3f, 0.5f, 1.0f);

    // Вспомогательные функции для градиентов (оставлены для совместимости, но не используются в упрощенной версии)

    // Современный стиль с градиентами и анимациями
    inline void StyleDeffHack() {
        ImGuiStyle* style = &ImGui::GetStyle();
        ImVec4* colors = style->Colors;

        // Базовые настройки
        style->WindowPadding = ImVec2(15, 15);
        style->FramePadding = ImVec2(10, 6);
        style->CellPadding = ImVec2(8, 4);
        style->ItemSpacing = ImVec2(8, 6);
        style->ItemInnerSpacing = ImVec2(6, 4);
        style->TouchExtraPadding = ImVec2(0, 0);
        style->IndentSpacing = 25;
        style->ScrollbarSize = 14;
        style->GrabMinSize = 12;

        // Скругления
        style->WindowRounding = 12.0f;
        style->ChildRounding = 8.0f;
        style->FrameRounding = 6.0f;
        style->PopupRounding = 8.0f;
        style->ScrollbarRounding = 6.0f;
        style->GrabRounding = 4.0f;
        style->TabRounding = 6.0f;

        // Границы
        style->WindowBorderSize = 0.0f;
        style->ChildBorderSize = 0.0f;
        style->PopupBorderSize = 0.0f;
        style->FrameBorderSize = 0.0f;
        style->TabBorderSize = 0.0f;

        // Эффекты
        style->WindowTitleAlign = ImVec2(0.5f, 0.5f);
        style->ButtonTextAlign = ImVec2(0.5f, 0.5f);
        style->SelectableTextAlign = ImVec2(0.0f, 0.0f);

        // Современная темная палитра с акцентами
        // Фон
        colors[ImGuiCol_WindowBg] = ImVec4(0.06f, 0.06f, 0.08f, 0.98f);
        colors[ImGuiCol_ChildBg] = ImVec4(0.05f, 0.05f, 0.07f, 1.00f);
        colors[ImGuiCol_PopupBg] = ImVec4(0.08f, 0.08f, 0.10f, 0.98f);

        // Текст
        colors[ImGuiCol_Text] = ImVec4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[ImGuiCol_TextDisabled] = ImVec4(0.50f, 0.50f, 0.50f, 1.00f);

        // Рамы и элементы
        colors[ImGuiCol_FrameBg] = ImVec4(0.12f, 0.12f, 0.15f, 1.00f);
        colors[ImGuiCol_FrameBgHovered] = ImVec4(0.18f, 0.18f, 0.22f, 1.00f);
        colors[ImGuiCol_FrameBgActive] = ImVec4(0.22f, 0.22f, 0.28f, 1.00f);

        // Заголовки окон
        colors[ImGuiCol_TitleBg] = ImVec4(0.08f, 0.08f, 0.10f, 1.00f);
        colors[ImGuiCol_TitleBgActive] = ImVec4(0.10f, 0.10f, 0.12f, 1.00f);
        colors[ImGuiCol_TitleBgCollapsed] = ImVec4(0.06f, 0.06f, 0.08f, 1.00f);

        // Кнопки с градиентом
        colors[ImGuiCol_Button] = ImVec4(0.25f, 0.50f, 0.95f, 1.00f);
        colors[ImGuiCol_ButtonHovered] = ImVec4(0.35f, 0.60f, 1.00f, 1.00f);
        colors[ImGuiCol_ButtonActive] = ImVec4(0.15f, 0.40f, 0.85f, 1.00f);

        // Заголовки
        colors[ImGuiCol_Header] = ImVec4(0.25f, 0.50f, 0.95f, 0.40f);
        colors[ImGuiCol_HeaderHovered] = ImVec4(0.35f, 0.60f, 1.00f, 0.60f);
        colors[ImGuiCol_HeaderActive] = ImVec4(0.25f, 0.50f, 0.95f, 1.00f);

        // Чекбоксы и слайдеры
        colors[ImGuiCol_CheckMark] = ImVec4(0.25f, 0.60f, 1.00f, 1.00f);
        colors[ImGuiCol_SliderGrab] = ImVec4(0.25f, 0.60f, 1.00f, 1.00f);
        colors[ImGuiCol_SliderGrabActive] = ImVec4(0.35f, 0.70f, 1.00f, 1.00f);

        // Вкладки
        colors[ImGuiCol_Tab] = ImVec4(0.10f, 0.10f, 0.12f, 1.00f);
        colors[ImGuiCol_TabHovered] = ImVec4(0.25f, 0.50f, 0.95f, 0.80f);
        colors[ImGuiCol_TabActive] = ImVec4(0.25f, 0.50f, 0.95f, 1.00f);
        colors[ImGuiCol_TabUnfocused] = ImVec4(0.08f, 0.08f, 0.10f, 1.00f);
        colors[ImGuiCol_TabUnfocusedActive] = ImVec4(0.12f, 0.12f, 0.15f, 1.00f);

        // Скроллбары
        colors[ImGuiCol_ScrollbarBg] = ImVec4(0.05f, 0.05f, 0.07f, 1.00f);
        colors[ImGuiCol_ScrollbarGrab] = ImVec4(0.25f, 0.25f, 0.35f, 1.00f);
        colors[ImGuiCol_ScrollbarGrabHovered] = ImVec4(0.35f, 0.35f, 0.45f, 1.00f);
        colors[ImGuiCol_ScrollbarGrabActive] = ImVec4(0.45f, 0.45f, 0.55f, 1.00f);

        // Разделители
        colors[ImGuiCol_Separator] = ImVec4(0.20f, 0.20f, 0.25f, 1.00f);
        colors[ImGuiCol_SeparatorHovered] = ImVec4(0.30f, 0.30f, 0.35f, 1.00f);
        colors[ImGuiCol_SeparatorActive] = ImVec4(0.40f, 0.40f, 0.45f, 1.00f);

        // Ресайз
        colors[ImGuiCol_ResizeGrip] = ImVec4(0.25f, 0.50f, 0.95f, 0.20f);
        colors[ImGuiCol_ResizeGripHovered] = ImVec4(0.35f, 0.60f, 1.00f, 0.60f);
        colors[ImGuiCol_ResizeGripActive] = ImVec4(0.45f, 0.70f, 1.00f, 1.00f);

        // Прочее
        colors[ImGuiCol_Border] = ImVec4(0.20f, 0.20f, 0.25f, 1.00f);
        colors[ImGuiCol_MenuBarBg] = ImVec4(0.08f, 0.08f, 0.10f, 1.00f);
    }

    // Красивый анимированный переключатель (упрощенная версия с публичным API)
    inline bool ModernSwitch(const char* label, bool* v, const ImVec4& activeColor = ImVec4(0.25f, 0.60f, 1.00f, 1.00f)) {
        // Используем стандартный Checkbox с кастомным стилем
        ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0.12f, 0.12f, 0.15f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, ImVec4(0.18f, 0.18f, 0.22f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_FrameBgActive, ImVec4(0.22f, 0.22f, 0.28f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_CheckMark, activeColor);
        
        bool result = ImGui::Checkbox(label, v);
        
        ImGui::PopStyleColor(4);
        return result;
    }

    // Современный слайдер (используем стандартный ImGui слайдер, но с улучшенным стилем)
    inline bool ModernSliderFloat(const char* label, float* v, float v_min, float v_max, const char* format = "%.1f", float power = 1.0f) {
        // Используем стандартный слайдер ImGui с улучшенными цветами
        ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0.12f, 0.12f, 0.15f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, ImVec4(0.18f, 0.18f, 0.22f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_FrameBgActive, ImVec4(0.22f, 0.22f, 0.28f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_SliderGrab, ImVec4(0.25f, 0.60f, 1.00f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_SliderGrabActive, ImVec4(0.35f, 0.70f, 1.00f, 1.0f));
        
        bool result = ImGui::SliderFloat(label, v, v_min, v_max, format, power);
        
        ImGui::PopStyleColor(5);
        return result;
    }

    // Красивая кнопка с градиентом (упрощенная версия с публичным API)
    inline bool ModernButton(const char* label, const ImVec2& size_arg = ImVec2(0, 0), const ImVec4& color = ImVec4(0.25f, 0.50f, 0.95f, 1.0f)) {
        // Используем стандартную кнопку с кастомными цветами
        ImGui::PushStyleColor(ImGuiCol_Button, color);
        ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(color.x * 1.2f, color.y * 1.2f, color.z * 1.2f, color.w));
        ImGui::PushStyleColor(ImGuiCol_ButtonActive, ImVec4(color.x * 0.7f, color.y * 0.7f, color.z * 0.7f, color.w));
        
        bool result = ImGui::Button(label, size_arg);
        
        ImGui::PopStyleColor(3);
        return result;
    }

    inline void Render() {
        if (!open) return;

        animation += ImGui::GetIO().DeltaTime * 2.0f;
        if (animation > 6.28f) animation = 0.0f; // Reset after 2π

        // Размер и позиция окна
        ImGui::SetNextWindowSize(ImVec2(680, 520), ImGuiCond_Once);
        ImGui::SetNextWindowPos(ImVec2(100, 100), ImGuiCond_Once);
        
        // Заголовок окна с градиентом
        if (ImGui::Begin("DeftHack Premium", &open, ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_NoResize)) {
            ImDrawList* draw_list = ImGui::GetWindowDrawList();
            ImVec2 window_pos = ImGui::GetWindowPos();
            ImVec2 window_size = ImGui::GetWindowSize();
            
            // Градиентный фон заголовка
            ImU32 col1 = ImGui::ColorConvertFloat4ToU32(ImVec4(0.15f, 0.30f, 0.60f, 1.0f));
            ImU32 col2 = ImGui::ColorConvertFloat4ToU32(ImVec4(0.25f, 0.50f, 0.95f, 1.0f));
            draw_list->AddRectFilledMultiColor(
                window_pos,
                ImVec2(window_pos.x + window_size.x, window_pos.y + 40),
                col1, col2, col2, col1
            );
            
            ImGui::Spacing();
            ImGui::Spacing();
            
            // Вкладки с улучшенным стилем
            const char* tabs[] = { "LEGIT", "VISUALS", "MISC", "SETTINGS" };
            
            if (ImGui::BeginTabBar("MainTabs", ImGuiTabBarFlags_None)) {
                // TAB: LEGIT
                if (ImGui::BeginTabItem("LEGIT")) {
                    ImGui::Spacing();
                    
                    ImGui::PushFont(ImGui::GetIO().Fonts->Fonts[0]);
                    ImGui::TextColored(ImVec4(0.5f, 0.8f, 1.0f, 1.0f), "AIM ASSIST");
                    ImGui::PopFont();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ModernSwitch("Enable Aimbot", &aimbot, ImVec4(0.25f, 0.70f, 1.00f, 1.0f));
                    if (aimbot) {
                        ImGui::Indent(20);
                        ImGui::Spacing();
                        ModernSliderFloat("FOV", &fov, 0.0f, 360.0f, "%.1f°");
                        ImGui::Spacing();
                        ModernSliderFloat("Smoothness", &smoothness, 1.0f, 20.0f, "%.1f");
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::Spacing();
                    ImGui::Spacing();
                    
                    ImGui::EndTabItem();
                }

                // TAB: VISUALS
                if (ImGui::BeginTabItem("VISUALS")) {
                    ImGui::Spacing();
                    
                    ImGui::PushFont(ImGui::GetIO().Fonts->Fonts[0]);
                    ImGui::TextColored(ImVec4(0.8f, 0.5f, 1.0f, 1.0f), "ESP & CHAMS");
                    ImGui::PopFont();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ModernSwitch("ESP", &esp, ImVec4(0.3f, 0.8f, 0.5f, 1.0f));
                    if (esp) {
                        ImGui::Indent(20);
                        ImGui::Spacing();
                        ModernSwitch("Box ESP", &esp_box);
                        ModernSwitch("Skeleton ESP", &esp_skeleton);
                        ModernSwitch("Player Names", &esp_name);
                        ModernSwitch("Distance", &esp_distance);
                        ImGui::Spacing();
                        ImGui::ColorEdit4("ESP Color", (float*)&esp_color, ImGuiColorEditFlags_NoInputs | ImGuiColorEditFlags_NoLabel);
                        ImGui::SameLine();
                        ImGui::Text("ESP Color");
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::Spacing();
                    ModernSwitch("Chams", &chams, ImVec4(1.0f, 0.3f, 0.5f, 1.0f));
                    if (chams) {
                        ImGui::Indent(20);
                        ImGui::Spacing();
                        const char* cham_types[] = { "Normal", "Flat", "Wireframe", "Glow" };
                        ImGui::Combo("Type", &cham_type, cham_types, IM_ARRAYSIZE(cham_types));
                        ImGui::ColorEdit4("Chams Color", (float*)&cham_color, ImGuiColorEditFlags_NoInputs | ImGuiColorEditFlags_NoLabel);
                        ImGui::SameLine();
                        ImGui::Text("Chams Color");
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::Spacing();
                    ImGui::EndTabItem();
                }

                // TAB: MISC
                if (ImGui::BeginTabItem("MISC")) {
                    ImGui::Spacing();
                    
                    ImGui::PushFont(ImGui::GetIO().Fonts->Fonts[0]);
                    ImGui::TextColored(ImVec4(1.0f, 0.7f, 0.3f, 1.0f), "MISCELLANEOUS");
                    ImGui::PopFont();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ModernSwitch("God Mode", &godMode, ImVec4(1.0f, 0.2f, 0.2f, 1.0f));
                    ModernSwitch("No Clip", &noClip, ImVec4(0.8f, 0.2f, 0.8f, 1.0f));
                    ModernSwitch("Player Flight", &playerFlight, ImVec4(0.5f, 0.8f, 1.0f, 1.0f));
                    
                    if (playerFlight) {
                        ImGui::Indent(20);
                        ImGui::Spacing();
                        ModernSliderFloat("Flight Speed", &flightSpeed, 0.1f, 10.0f, "%.1fx");
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::Spacing();
                    ModernSwitch("Bunny Hop", &bhop, ImVec4(0.3f, 1.0f, 0.5f, 1.0f));
                    ModernSwitch("Night Vision", &nightVision, ImVec4(1.0f, 1.0f, 0.3f, 1.0f));
                    
                    ImGui::Spacing();
                    ModernSliderFloat("Speed Hack", &speedHack, 0.1f, 5.0f, "%.1fx");
                    
                    ImGui::Spacing();
                    ImGui::Spacing();
                    if (ModernButton("Heal Player", ImVec2(-1, 35), ImVec4(0.2f, 0.8f, 0.3f, 1.0f))) {
                        // Вызов функции лечения из C# чита
                    }
                    
                    ImGui::Spacing();
                    ImGui::EndTabItem();
                }

                // TAB: SETTINGS
                if (ImGui::BeginTabItem("SETTINGS")) {
                    ImGui::Spacing();
                    
                    ImGui::PushFont(ImGui::GetIO().Fonts->Fonts[0]);
                    ImGui::TextColored(ImVec4(0.6f, 0.6f, 0.8f, 1.0f), "SETTINGS");
                    ImGui::PopFont();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Text("DeftHack Premium v2.0");
                    ImGui::Text("Build: Release x64");
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Text("Hotkeys:");
                    ImGui::BulletText("INSERT - Toggle Menu");
                    ImGui::BulletText("END - Unload Hack");
                    ImGui::Spacing();
                    
                    if (ModernButton("Unload Hack", ImVec2(-1, 40), ImVec4(0.8f, 0.2f, 0.2f, 1.0f))) {
                        open = false;
                        // TODO: Отключить читы и выгрузить DLL
                    }
                    
                    ImGui::Spacing();
                    ImGui::EndTabItem();
                }

                ImGui::EndTabBar();
            }
            
            // Горячие клавиши обрабатываются в hkPresent через ImGui_ImplWin32_WndProcHandler
        }
        ImGui::End();
    }
}
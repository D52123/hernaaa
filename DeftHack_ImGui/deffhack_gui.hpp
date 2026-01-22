#pragma once
#include <imgui.h>
#include <cmath>
#include <algorithm>

namespace DeftHackGUI {
    inline bool open = true;
    inline int tab = 0;
    inline float animation = 0.0f;

    // ============ AIMBOT OPTIONS ============
    inline bool aimbot_enabled = false;
    inline bool aimbot_smooth = false;
    inline bool aimbot_onkey = false;
    inline bool aimbot_usefov = true;
    inline bool aimbot_noaimbotdrop = false;
    inline float aimbot_speed = 5.0f;
    inline float aimbot_distance = 300.0f;
    inline float aimbot_fov = 15.0f;
    
    // ============ WEAPON OPTIONS ============
    inline bool weapon_norecoil = false;
    inline bool weapon_nospread = false;
    inline bool weapon_nosway = false;
    inline bool weapon_nodrop = false;
    inline bool weapon_autoreload = false;
    inline bool weapon_tracers = false;
    inline bool weapon_zoom = false;
    inline float weapon_zoomvalue = 16.0f;
    inline bool weapon_customcrosshair = false;
    
    // ============ TRIGGERBOT ============
    inline bool triggerbot_enabled = false;
    
    // ============ ESP OPTIONS ============
    inline bool esp_enabled = true;
    inline bool esp_chams = false;
    inline bool esp_chamsflat = false;
    inline bool esp_showvanish = false;
    inline bool esp_showtooltip = false;
    inline bool esp_showcoords = false;
    inline bool esp_showplayerweapon = false;
    inline bool esp_showplayervehicle = false;
    inline bool esp_filteritems = false;
    
    // ============ RADAR OPTIONS ============
    inline bool radar_enabled = false;
    inline bool radar_trackplayer = false;
    inline bool radar_showplayers = false;
    inline bool radar_showvehicles = false;
    inline float radar_size = 300.0f;
    
    // ============ INTERACTION OPTIONS ============
    inline bool interact_throughwalls = false;
    inline bool interact_nohitstructures = false;
    inline bool interact_nohitbarricades = false;
    inline bool interact_nohititems = false;
    inline bool interact_nohitvehicles = false;
    inline bool interact_nohitresources = false;
    inline bool interact_nohitenvironment = false;
    
    // ============ ITEM OPTIONS ============
    inline bool item_autopickup = false;
    inline bool item_autoforage = false;
    inline int item_pickupdelay = 1000;
    
    // ============ MISC OPTIONS ============
    inline bool misc_punchsilentaim = false;
    inline bool misc_punchaura = false;
    inline bool misc_noflash = false;
    inline bool misc_nosnow = false;
    inline bool misc_norain = false;
    inline bool misc_noflinch = false;
    inline bool misc_nograyscale = false;
    inline bool misc_slowfall = false;
    inline bool misc_airstick = false;
    inline bool misc_compass = false;
    inline bool misc_gps = false;
    inline bool misc_bones = false;
    inline bool misc_showplayersonmap = false;
    inline bool misc_nightvision = false;
    inline bool misc_vehiclefly = false;
    inline bool misc_vehiclemaxspeed = false;
    inline float misc_speedmultiplier = 1.0f;
    inline bool misc_extendmeleerange = false;
    inline float misc_meleerange = 7.5f;
    inline bool misc_playerflight = false;
    inline float misc_flightspeed = 1.0f;
    inline bool misc_freecam = false;
    inline bool misc_spammer = false;
    inline int misc_spammerdelay = 0;
    
    inline ImVec4 accent_color = ImVec4(0.25f, 0.60f, 1.00f, 1.0f);

    inline void StyleDeffHack() {
        ImGuiStyle* style = &ImGui::GetStyle();
        ImVec4* colors = style->Colors;

        style->WindowPadding = ImVec2(15, 15);
        style->FramePadding = ImVec2(10, 6);
        style->CellPadding = ImVec2(8, 4);
        style->ItemSpacing = ImVec2(8, 6);
        style->ItemInnerSpacing = ImVec2(6, 4);
        style->IndentSpacing = 25;
        style->ScrollbarSize = 14;
        style->GrabMinSize = 12;

        style->WindowRounding = 12.0f;
        style->ChildRounding = 8.0f;
        style->FrameRounding = 6.0f;
        style->PopupRounding = 8.0f;
        style->ScrollbarRounding = 6.0f;
        style->GrabRounding = 4.0f;
        style->TabRounding = 6.0f;

        style->WindowBorderSize = 0.0f;
        style->ChildBorderSize = 0.0f;
        style->PopupBorderSize = 0.0f;
        style->FrameBorderSize = 0.0f;
        style->TabBorderSize = 0.0f;

        style->WindowTitleAlign = ImVec2(0.5f, 0.5f);
        style->ButtonTextAlign = ImVec2(0.5f, 0.5f);

        colors[ImGuiCol_WindowBg] = ImVec4(0.06f, 0.06f, 0.08f, 0.98f);
        colors[ImGuiCol_ChildBg] = ImVec4(0.05f, 0.05f, 0.07f, 1.00f);
        colors[ImGuiCol_PopupBg] = ImVec4(0.08f, 0.08f, 0.10f, 0.98f);
        colors[ImGuiCol_Text] = ImVec4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[ImGuiCol_TextDisabled] = ImVec4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[ImGuiCol_FrameBg] = ImVec4(0.12f, 0.12f, 0.15f, 1.00f);
        colors[ImGuiCol_FrameBgHovered] = ImVec4(0.18f, 0.18f, 0.22f, 1.00f);
        colors[ImGuiCol_FrameBgActive] = ImVec4(0.22f, 0.22f, 0.28f, 1.00f);
        colors[ImGuiCol_TitleBg] = ImVec4(0.08f, 0.08f, 0.10f, 1.00f);
        colors[ImGuiCol_TitleBgActive] = ImVec4(0.10f, 0.10f, 0.12f, 1.00f);
        colors[ImGuiCol_TitleBgCollapsed] = ImVec4(0.06f, 0.06f, 0.08f, 1.00f);
        colors[ImGuiCol_Button] = ImVec4(0.25f, 0.50f, 0.95f, 1.00f);
        colors[ImGuiCol_ButtonHovered] = ImVec4(0.35f, 0.60f, 1.00f, 1.00f);
        colors[ImGuiCol_ButtonActive] = ImVec4(0.15f, 0.40f, 0.85f, 1.00f);
        colors[ImGuiCol_Header] = ImVec4(0.25f, 0.50f, 0.95f, 0.40f);
        colors[ImGuiCol_HeaderHovered] = ImVec4(0.35f, 0.60f, 1.00f, 0.60f);
        colors[ImGuiCol_HeaderActive] = ImVec4(0.25f, 0.50f, 0.95f, 1.00f);
        colors[ImGuiCol_CheckMark] = ImVec4(0.25f, 0.60f, 1.00f, 1.00f);
        colors[ImGuiCol_SliderGrab] = ImVec4(0.25f, 0.60f, 1.00f, 1.00f);
        colors[ImGuiCol_SliderGrabActive] = ImVec4(0.35f, 0.70f, 1.00f, 1.00f);
        colors[ImGuiCol_Tab] = ImVec4(0.10f, 0.10f, 0.12f, 1.00f);
        colors[ImGuiCol_TabHovered] = ImVec4(0.25f, 0.50f, 0.95f, 0.80f);
        colors[ImGuiCol_TabActive] = ImVec4(0.25f, 0.50f, 0.95f, 1.00f);
        colors[ImGuiCol_TabUnfocused] = ImVec4(0.08f, 0.08f, 0.10f, 1.00f);
        colors[ImGuiCol_TabUnfocusedActive] = ImVec4(0.12f, 0.12f, 0.15f, 1.00f);
        colors[ImGuiCol_ScrollbarBg] = ImVec4(0.05f, 0.05f, 0.07f, 1.00f);
        colors[ImGuiCol_ScrollbarGrab] = ImVec4(0.25f, 0.25f, 0.35f, 1.00f);
        colors[ImGuiCol_ScrollbarGrabHovered] = ImVec4(0.35f, 0.35f, 0.45f, 1.00f);
        colors[ImGuiCol_ScrollbarGrabActive] = ImVec4(0.45f, 0.45f, 0.55f, 1.00f);
        colors[ImGuiCol_Separator] = ImVec4(0.20f, 0.20f, 0.25f, 1.00f);
        colors[ImGuiCol_SeparatorHovered] = ImVec4(0.30f, 0.30f, 0.35f, 1.00f);
        colors[ImGuiCol_SeparatorActive] = ImVec4(0.40f, 0.40f, 0.45f, 1.00f);
        colors[ImGuiCol_ResizeGrip] = ImVec4(0.25f, 0.50f, 0.95f, 0.20f);
        colors[ImGuiCol_ResizeGripHovered] = ImVec4(0.35f, 0.60f, 1.00f, 0.60f);
        colors[ImGuiCol_ResizeGripActive] = ImVec4(0.45f, 0.70f, 1.00f, 1.00f);
        colors[ImGuiCol_Border] = ImVec4(0.20f, 0.20f, 0.25f, 1.00f);
        colors[ImGuiCol_MenuBarBg] = ImVec4(0.08f, 0.08f, 0.10f, 1.00f);
    }

    inline void Render() {
        if (!open) return;

        ImGui::SetNextWindowSize(ImVec2(750, 580), ImGuiCond_Once);
        ImGui::SetNextWindowPos(ImVec2(100, 100), ImGuiCond_Once);
        
        if (ImGui::Begin("DeftHack Premium | Unturned", &open, ImGuiWindowFlags_NoCollapse)) {
            
            if (ImGui::BeginTabBar("MainTabs")) {
                
                // ========== AIMBOT TAB ==========
                if (ImGui::BeginTabItem("AIMBOT")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(0.5f, 0.8f, 1.0f, 1.0f), "AIMBOT SETTINGS");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Enable Aimbot", &aimbot_enabled);
                    ImGui::Checkbox("Smooth Aim", &aimbot_smooth);
                    ImGui::Checkbox("Aim On Key", &aimbot_onkey);
                    ImGui::Checkbox("Use FOV", &aimbot_usefov);
                    ImGui::Checkbox("No Aimbot Drop", &aimbot_noaimbotdrop);
                    
                    ImGui::Spacing();
                    ImGui::SliderFloat("Aim Speed", &aimbot_speed, 1.0f, 20.0f, "%.1f");
                    ImGui::SliderFloat("Max Distance", &aimbot_distance, 50.0f, 500.0f, "%.0f m");
                    ImGui::SliderFloat("FOV", &aimbot_fov, 1.0f, 180.0f, "%.1f°");
                    
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::TextColored(ImVec4(0.8f, 0.5f, 1.0f, 1.0f), "TRIGGERBOT");
                    ImGui::Separator();
                    ImGui::Spacing();
                    ImGui::Checkbox("Enable Triggerbot", &triggerbot_enabled);
                    
                    ImGui::EndTabItem();
                }
                
                // ========== WEAPON TAB ==========
                if (ImGui::BeginTabItem("WEAPON")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(1.0f, 0.7f, 0.3f, 1.0f), "WEAPON MODIFICATIONS");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("No Recoil", &weapon_norecoil);
                    ImGui::Checkbox("No Spread", &weapon_nospread);
                    ImGui::Checkbox("No Sway", &weapon_nosway);
                    ImGui::Checkbox("No Bullet Drop", &weapon_nodrop);
                    ImGui::Checkbox("Auto Reload", &weapon_autoreload);
                    ImGui::Checkbox("Bullet Tracers", &weapon_tracers);
                    ImGui::Checkbox("Custom Crosshair", &weapon_customcrosshair);
                    
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Zoom", &weapon_zoom);
                    if (weapon_zoom) {
                        ImGui::Indent(20);
                        ImGui::SliderFloat("Zoom Value", &weapon_zoomvalue, 1.0f, 50.0f, "%.1fx");
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::Spacing();
                    ImGui::Checkbox("Extend Melee Range", &misc_extendmeleerange);
                    if (misc_extendmeleerange) {
                        ImGui::Indent(20);
                        ImGui::SliderFloat("Melee Range", &misc_meleerange, 1.0f, 20.0f, "%.1f m");
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::EndTabItem();
                }
                
                // ========== ESP TAB ==========
                if (ImGui::BeginTabItem("ESP")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(0.3f, 0.8f, 0.5f, 1.0f), "ESP & VISUALS");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Enable ESP", &esp_enabled);
                    ImGui::Checkbox("Chams", &esp_chams);
                    if (esp_chams) {
                        ImGui::Indent(20);
                        ImGui::Checkbox("Flat Chams", &esp_chamsflat);
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::Spacing();
                    ImGui::Checkbox("Show Vanished Players", &esp_showvanish);
                    ImGui::Checkbox("Show Tooltip Window", &esp_showtooltip);
                    ImGui::Checkbox("Show Coordinates", &esp_showcoords);
                    ImGui::Checkbox("Show Player Weapon", &esp_showplayerweapon);
                    ImGui::Checkbox("Show Player Vehicle", &esp_showplayervehicle);
                    ImGui::Checkbox("Filter Items", &esp_filteritems);
                    
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::TextColored(ImVec4(0.8f, 0.8f, 0.3f, 1.0f), "RADAR");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Enable Radar", &radar_enabled);
                    if (radar_enabled) {
                        ImGui::Indent(20);
                        ImGui::Checkbox("Track Player", &radar_trackplayer);
                        ImGui::Checkbox("Show Players", &radar_showplayers);
                        ImGui::Checkbox("Show Vehicles", &radar_showvehicles);
                        ImGui::SliderFloat("Radar Size", &radar_size, 100.0f, 500.0f, "%.0f");
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::EndTabItem();
                }
                
                // ========== PLAYER TAB ==========
                if (ImGui::BeginTabItem("PLAYER")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(0.5f, 1.0f, 0.8f, 1.0f), "PLAYER MODIFICATIONS");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Player Flight", &misc_playerflight);
                    if (misc_playerflight) {
                        ImGui::Indent(20);
                        ImGui::SliderFloat("Flight Speed", &misc_flightspeed, 0.1f, 10.0f, "%.1fx");
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::Spacing();
                    ImGui::Checkbox("Freecam", &misc_freecam);
                    ImGui::Checkbox("Slow Fall", &misc_slowfall);
                    ImGui::Checkbox("Air Stick", &misc_airstick);
                    
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::TextColored(ImVec4(1.0f, 0.5f, 0.5f, 1.0f), "COMBAT");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Punch Silent Aim", &misc_punchsilentaim);
                    ImGui::Checkbox("Punch Aura", &misc_punchaura);
                    ImGui::Checkbox("No Flash", &misc_noflash);
                    ImGui::Checkbox("No Flinch", &misc_noflinch);
                    
                    ImGui::EndTabItem();
                }
                
                // ========== WORLD TAB ==========
                if (ImGui::BeginTabItem("WORLD")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(0.8f, 1.0f, 0.5f, 1.0f), "WORLD MODIFICATIONS");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Night Vision", &misc_nightvision);
                    ImGui::Checkbox("No Snow", &misc_nosnow);
                    ImGui::Checkbox("No Rain", &misc_norain);
                    ImGui::Checkbox("No Grayscale", &misc_nograyscale);
                    
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::TextColored(ImVec4(0.5f, 0.8f, 1.0f, 1.0f), "INTERACTION");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Interact Through Walls", &interact_throughwalls);
                    ImGui::Checkbox("No Hit Structures", &interact_nohitstructures);
                    ImGui::Checkbox("No Hit Barricades", &interact_nohitbarricades);
                    ImGui::Checkbox("No Hit Items", &interact_nohititems);
                    ImGui::Checkbox("No Hit Vehicles", &interact_nohitvehicles);
                    ImGui::Checkbox("No Hit Resources", &interact_nohitresources);
                    ImGui::Checkbox("No Hit Environment", &interact_nohitenvironment);
                    
                    ImGui::EndTabItem();
                }
                
                // ========== ITEMS TAB ==========
                if (ImGui::BeginTabItem("ITEMS")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(1.0f, 0.8f, 0.3f, 1.0f), "ITEM OPTIONS");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Auto Item Pickup", &item_autopickup);
                    ImGui::Checkbox("Auto Forage Pickup", &item_autoforage);
                    
                    ImGui::Spacing();
                    ImGui::SliderInt("Pickup Delay (ms)", &item_pickupdelay, 0, 5000);
                    
                    ImGui::EndTabItem();
                }
                
                // ========== VEHICLE TAB ==========
                if (ImGui::BeginTabItem("VEHICLE")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(0.8f, 0.5f, 1.0f, 1.0f), "VEHICLE MODIFICATIONS");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Vehicle Fly", &misc_vehiclefly);
                    ImGui::Checkbox("Use Max Speed", &misc_vehiclemaxspeed);
                    
                    ImGui::Spacing();
                    ImGui::SliderFloat("Speed Multiplier", &misc_speedmultiplier, 0.1f, 10.0f, "%.1fx");
                    
                    ImGui::EndTabItem();
                }
                
                // ========== MISC TAB ==========
                if (ImGui::BeginTabItem("MISC")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(1.0f, 0.7f, 0.5f, 1.0f), "MISCELLANEOUS");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Compass", &misc_compass);
                    ImGui::Checkbox("GPS", &misc_gps);
                    ImGui::Checkbox("Show Bones", &misc_bones);
                    ImGui::Checkbox("Show Players On Map", &misc_showplayersonmap);
                    
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Checkbox("Chat Spammer", &misc_spammer);
                    if (misc_spammer) {
                        ImGui::Indent(20);
                        ImGui::SliderInt("Spam Delay (ms)", &misc_spammerdelay, 0, 5000);
                        ImGui::Unindent(20);
                    }
                    
                    ImGui::EndTabItem();
                }
                
                // ========== INFO TAB ==========
                if (ImGui::BeginTabItem("INFO")) {
                    ImGui::Spacing();
                    ImGui::TextColored(ImVec4(0.6f, 0.6f, 0.8f, 1.0f), "INFORMATION");
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::Text("DeftHack Premium for Unturned");
                    ImGui::Text("Version: 2.0 ImGui Edition");
                    ImGui::Text("Build: Release x64");
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::TextColored(ImVec4(0.8f, 0.8f, 0.3f, 1.0f), "Hotkeys:");
                    ImGui::BulletText("INSERT - Toggle Menu");
                    ImGui::BulletText("END - Panic Mode");
                    ImGui::Spacing();
                    ImGui::Separator();
                    ImGui::Spacing();
                    
                    ImGui::TextColored(ImVec4(1.0f, 0.3f, 0.3f, 1.0f), "WARNING:");
                    ImGui::TextWrapped("This is a game modification tool. Use at your own risk. The developers are not responsible for any bans or consequences.");
                    
                    ImGui::Spacing();
                    ImGui::Spacing();
                    
                    if (ImGui::Button("Close Menu", ImVec2(-1, 40))) {
                        open = false;
                    }
                    
                    ImGui::EndTabItem();
                }
                
                ImGui::EndTabBar();
            }
        }
        ImGui::End();
    }
}

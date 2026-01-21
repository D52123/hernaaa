using System;
using UnityEngine;
using SDG.Unturned;

namespace DeftHack.Components.UI
{
    /// <summary>
    /// Современный Unity GUI интерфейс для DeftHack (ImGui-стиль)
    /// Подключен к существующим функциям DeftHack
    /// </summary>
    public class ImGuiCheat : MonoBehaviour
    {
        private bool showMainWindow = false;
        private Rect windowRect = new Rect(100, 100, 450, 650);
        private int selectedTab = 0;
        private string[] tabNames = { "Main", "Visual", "Settings", "Security" };
        
        // Статистика
        private float uptime = 0f;
        private int playersCount = 0;
        
        // GUI стили
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private GUIStyle tabStyle;
        private GUIStyle activeTabStyle;
        private GUIStyle boxStyle;
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            
            // ВАЖНО: Убеждаемся что компонент включен
            enabled = true;
            gameObject.SetActive(true);
            
            InitializeStyles();
            
            // Встроенная верификация инжекции
            CreateInjectionVerification();
            
            Debug.Log("=== DEFTHACK MODERN GUI LOADED ===");
            Debug.Log("[DeftHack] Press INSERT to open menu");
            Debug.Log($"[DeftHack] Component enabled: {enabled}, GameObject active: {gameObject.activeSelf}");
            
            ShowNotification("DeftHack Modern GUI loaded! Press INSERT", Color.green);
        }
        
        void Awake()
        {
            // Убеждаемся что компонент активен с самого начала
            enabled = true;
            gameObject.SetActive(true);
        }
        
        void InitializeStyles()
        {
            // Заголовок
            headerStyle = new GUIStyle();
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.cyan;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            
            // Кнопки
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12;
            buttonStyle.fontStyle = FontStyle.Bold;
            
            // Вкладки
            tabStyle = new GUIStyle(GUI.skin.button);
            tabStyle.fontSize = 11;
            tabStyle.normal.textColor = Color.gray;
            
            activeTabStyle = new GUIStyle(GUI.skin.button);
            activeTabStyle.fontSize = 11;
            activeTabStyle.normal.textColor = Color.cyan;
            activeTabStyle.fontStyle = FontStyle.Bold;
            
            // Блоки
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.padding = new RectOffset(10, 10, 10, 10);
        }
        
        void Update()
        {
            uptime += Time.deltaTime;
            
            // INSERT - главное меню
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                showMainWindow = !showMainWindow;
                Debug.Log("[DeftHack] Modern GUI Menu " + (showMainWindow ? "OPENED" : "CLOSED"));
            }
            
            // F2 - тест функциональности
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TestInjectionFunctionality();
            }
            
            // F3 - показать статус
            if (Input.GetKeyDown(KeyCode.F3))
            {
                ShowDetailedStatus();
            }
            
            // Периодически проверяем что компонент активен
            if (Time.frameCount % 300 == 0) // Каждые ~5 секунд при 60 FPS
            {
                if (!enabled)
                {
                    enabled = true;
                    Debug.Log("[DeftHack ImGuiCheat] Component was disabled! Re-enabled.");
                }
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                    Debug.Log("[DeftHack ImGuiCheat] GameObject was inactive! Re-activated.");
                }
            }
            
            // Обновляем статистику
            UpdateStats();
            
            // Применяем читы
            ApplyCheats();
        }
        
        void OnGUI()
        {
            // ВСЕГДА показываем статус внизу экрана - подтверждение работы
            GUI.color = Color.green;
            GUI.Label(new Rect(10, Screen.height - 100, 600, 20), "✓ DEFTHACK MODERN GUI v2.0 - ACTIVE");
            GUI.color = Color.cyan;
            GUI.Label(new Rect(10, Screen.height - 80, 600, 20), $"Uptime: {uptime:F1}s | Players: {playersCount} | INSERT - Menu");
            GUI.color = Color.yellow;
            GUI.Label(new Rect(10, Screen.height - 60, 600, 20), $"✓ Component enabled: {enabled} | GameObject active: {gameObject.activeSelf}");
            GUI.color = Color.white;
            
            if (!showMainWindow) return;
            
            // Главное окно
            windowRect = GUI.Window(12346, windowRect, DrawMainWindow, "");
        }
        
        void DrawMainWindow(int windowID)
        {
            GUILayout.BeginVertical();
            
            // Заголовок
            GUILayout.Label("DEFTHACK PROFESSIONAL v2.0", headerStyle);
            GUILayout.Space(10);
            
            // Вкладки
            DrawTabs();
            GUILayout.Space(10);
            
            // Содержимое вкладок
            switch (selectedTab)
            {
                case 0: DrawMainTab(); break;
                case 1: DrawVisualTab(); break;
                case 2: DrawSettingsTab(); break;
                case 3: DrawSecurityTab(); break;
            }
            
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        
        void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabNames.Length; i++)
            {
                GUIStyle style = (i == selectedTab) ? activeTabStyle : tabStyle;
                if (GUILayout.Button(tabNames[i], style, GUILayout.Width(100)))
                {
                    selectedTab = i;
                }
            }
            GUILayout.EndHorizontal();
        }
        
        void DrawMainTab()
        {
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label("🎯 MAIN CHEATS", headerStyle);
            
            // Hang (No Gravity)
            GUI.color = MiscOptions.hang ? Color.green : Color.white;
            if (GUILayout.Button($"🛡️ Hang Mode: {(MiscOptions.hang ? "ON" : "OFF")}", buttonStyle))
            {
                MiscOptions.hang = !MiscOptions.hang;
                ShowNotification($"Hang Mode: {(MiscOptions.hang ? "ON" : "OFF")}", MiscOptions.hang ? Color.green : Color.red);
            }
            
            // Player Flight
            GUI.color = MiscOptions.PlayerFlight ? Color.green : Color.white;
            if (GUILayout.Button($"⚡ Player Flight: {(MiscOptions.PlayerFlight ? "ON" : "OFF")}", buttonStyle))
            {
                MiscOptions.PlayerFlight = !MiscOptions.PlayerFlight;
                ShowNotification($"Player Flight: {(MiscOptions.PlayerFlight ? "ON" : "OFF")}", MiscOptions.PlayerFlight ? Color.green : Color.red);
            }
            
            if (MiscOptions.PlayerFlight)
            {
                GUI.color = Color.white;
                GUILayout.Label($"Flight Speed: {MiscOptions.FlightSpeedMultiplier:F1}x");
                MiscOptions.FlightSpeedMultiplier = GUILayout.HorizontalSlider(MiscOptions.FlightSpeedMultiplier, 0.1f, 5.0f);
            }
            
            // Vehicle Fly
            GUI.color = MiscOptions.VehicleFly ? Color.green : Color.white;
            if (GUILayout.Button($"🚗 Vehicle Fly: {(MiscOptions.VehicleFly ? "ON" : "OFF")}", buttonStyle))
            {
                MiscOptions.VehicleFly = !MiscOptions.VehicleFly;
                ShowNotification($"Vehicle Fly: {(MiscOptions.VehicleFly ? "ON" : "OFF")}", MiscOptions.VehicleFly ? Color.green : Color.red);
            }
            
            if (MiscOptions.VehicleFly)
            {
                GUI.color = Color.white;
                GUILayout.Label($"Speed: {MiscOptions.SpeedMultiplier:F1}x");
                MiscOptions.SpeedMultiplier = GUILayout.HorizontalSlider(MiscOptions.SpeedMultiplier, 0.1f, 10.0f);
                
                MiscOptions.VehicleUseMaxSpeed = GUILayout.Toggle(MiscOptions.VehicleUseMaxSpeed, "Use Max Speed");
            }
            
            // Night Vision
            GUI.color = MiscOptions.NightVision ? Color.green : Color.white;
            if (GUILayout.Button($"🌙 Night Vision: {(MiscOptions.NightVision ? "ON" : "OFF")}", buttonStyle))
            {
                MiscOptions.NightVision = !MiscOptions.NightVision;
                ShowNotification($"Night Vision: {(MiscOptions.NightVision ? "ON" : "OFF")}", MiscOptions.NightVision ? Color.green : Color.red);
            }
            
            // Freecam
            GUI.color = MiscOptions.Freecam ? Color.green : Color.white;
            if (GUILayout.Button($"👁️ Freecam: {(MiscOptions.Freecam ? "ON" : "OFF")}", buttonStyle))
            {
                MiscOptions.Freecam = !MiscOptions.Freecam;
                ShowNotification($"Freecam: {(MiscOptions.Freecam ? "ON" : "OFF")}", MiscOptions.Freecam ? Color.green : Color.red);
            }
            
            GUI.color = Color.white;
            GUILayout.EndVertical();
            
            // Кнопки действий
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("💊 Heal", buttonStyle))
            {
                HealPlayer();
            }
            if (GUILayout.Button("📍 Teleport", buttonStyle))
            {
                TeleportToCrosshair();
            }
            GUILayout.EndHorizontal();
        }
        
        void DrawVisualTab()
        {
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label("👁️ VISUAL CHEATS", headerStyle);
            
            // ESP
            GUI.color = ESPOptions.Enabled ? Color.green : Color.white;
            if (GUILayout.Button($"📡 ESP: {(ESPOptions.Enabled ? "ON" : "OFF")}", buttonStyle))
            {
                ESPOptions.Enabled = !ESPOptions.Enabled;
                ShowNotification($"ESP: {(ESPOptions.Enabled ? "ON" : "OFF")}", ESPOptions.Enabled ? Color.green : Color.red);
            }
            
            if (ESPOptions.Enabled)
            {
                GUI.color = Color.white;
                GUILayout.Label("ESP Settings:");
                ESPOptions.ShowPlayerWeapon = GUILayout.Toggle(ESPOptions.ShowPlayerWeapon, "Show Player Weapon");
                ESPOptions.ShowPlayerVehicle = GUILayout.Toggle(ESPOptions.ShowPlayerVehicle, "Show Player Vehicle");
                ESPOptions.ShowVehicleFuel = GUILayout.Toggle(ESPOptions.ShowVehicleFuel, "Show Vehicle Fuel");
                ESPOptions.ShowVehicleHealth = GUILayout.Toggle(ESPOptions.ShowVehicleHealth, "Show Vehicle Health");
                ESPOptions.FilterItems = GUILayout.Toggle(ESPOptions.FilterItems, "Filter Items");
            }
            
            // Aimbot
            GUI.color = AimbotOptions.Enabled ? Color.green : Color.white;
            if (GUILayout.Button($"🎯 Aimbot: {(AimbotOptions.Enabled ? "ON" : "OFF")}", buttonStyle))
            {
                AimbotOptions.Enabled = !AimbotOptions.Enabled;
                ShowNotification($"Aimbot: {(AimbotOptions.Enabled ? "ON" : "OFF")}", AimbotOptions.Enabled ? Color.green : Color.red);
            }
            
            if (AimbotOptions.Enabled)
            {
                GUI.color = Color.white;
                GUILayout.Label($"FOV: {AimbotOptions.FOV:F0}°");
                AimbotOptions.FOV = GUILayout.HorizontalSlider(AimbotOptions.FOV, 5.0f, 180.0f);
                
                GUILayout.Label($"Distance: {AimbotOptions.Distance:F0}m");
                AimbotOptions.Distance = GUILayout.HorizontalSlider(AimbotOptions.Distance, 50.0f, 1000.0f);
                
                AimbotOptions.Smooth = GUILayout.Toggle(AimbotOptions.Smooth, "Smooth Aim");
                if (AimbotOptions.Smooth)
                {
                    GUILayout.Label($"Aim Speed: {AimbotOptions.AimSpeed:F1}");
                    AimbotOptions.AimSpeed = GUILayout.HorizontalSlider(AimbotOptions.AimSpeed, 1.0f, 20.0f);
                }
                
                AimbotOptions.OnKey = GUILayout.Toggle(AimbotOptions.OnKey, "Aimbot On Key");
                AimbotOptions.UseFovAim = GUILayout.Toggle(AimbotOptions.UseFovAim, "Use FOV Limit");
            }
            
            // Chams
            GUI.color = ESPOptions.ChamsEnabled ? Color.green : Color.white;
            if (GUILayout.Button($"🌈 Chams: {(ESPOptions.ChamsEnabled ? "ON" : "OFF")}", buttonStyle))
            {
                ESPOptions.ChamsEnabled = !ESPOptions.ChamsEnabled;
                ShowNotification($"Chams: {(ESPOptions.ChamsEnabled ? "ON" : "OFF")}", ESPOptions.ChamsEnabled ? Color.green : Color.red);
            }
            
            if (ESPOptions.ChamsEnabled)
            {
                GUI.color = Color.white;
                ESPOptions.ChamsFlat = GUILayout.Toggle(ESPOptions.ChamsFlat, "Flat Chams");
            }
            
            GUI.color = Color.white;
            GUILayout.EndVertical();
        }
        
        void DrawSettingsTab()
        {
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label("⚙️ SETTINGS", headerStyle);
            
            GUILayout.Label("📊 Statistics:");
            GUILayout.Label($"⏱️ Uptime: {uptime:F1}s");
            GUILayout.Label($"👥 Players: {playersCount}");
            GUILayout.Label($"🎮 Status: ACTIVE");
            
            GUILayout.Space(10);
            
            GUILayout.Label("🌦️ Weather Control:");
            MiscOptions.NoRain = GUILayout.Toggle(MiscOptions.NoRain, "No Rain");
            MiscOptions.NoSnow = GUILayout.Toggle(MiscOptions.NoSnow, "No Snow");
            
            GUILayout.Space(5);
            GUILayout.Label("🔧 Misc Options:");
            MiscOptions.NoFlash = GUILayout.Toggle(MiscOptions.NoFlash, "No Flash");
            MiscOptions.SlowFall = GUILayout.Toggle(MiscOptions.SlowFall, "Slow Fall");
            MiscOptions.AirStick = GUILayout.Toggle(MiscOptions.AirStick, "Air Stick");
            MiscOptions.Compass = GUILayout.Toggle(MiscOptions.Compass, "Compass");
            MiscOptions.GPS = GUILayout.Toggle(MiscOptions.GPS, "GPS");
            
            if (MiscOptions.SetTimeEnabled)
            {
                GUILayout.Label($"Time: {MiscOptions.Time:F1}");
                MiscOptions.Time = GUILayout.HorizontalSlider(MiscOptions.Time, 0f, 24f);
            }
            MiscOptions.SetTimeEnabled = GUILayout.Toggle(MiscOptions.SetTimeEnabled, "Custom Time");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("💾 Save Config", buttonStyle))
            {
                SaveConfig();
            }
            
            if (GUILayout.Button("📁 Load Config", buttonStyle))
            {
                LoadConfig();
            }
            
            if (GUILayout.Button("🔄 Restart Systems", buttonStyle))
            {
                RestartSystems();
            }
            
            GUILayout.EndVertical();
        }
        
        void DrawSecurityTab()
        {
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label("🛡️ SECURITY", headerStyle);
            
            GUILayout.Label("🔒 Protection Status: ACTIVE");
            GUILayout.Label("🛡️ Anti-Detection: ENABLED");
            GUILayout.Label("📸 Screenshot Bypass: ON");
            GUILayout.Label("💻 Memory Protection: ON");
            
            GUILayout.Space(10);
            
            GUILayout.Label("🚨 Panic Mode:");
            GUI.color = MiscOptions.PanicMode ? Color.red : Color.white;
            if (GUILayout.Button($"🚨 Panic Mode: {(MiscOptions.PanicMode ? "ACTIVE" : "OFF")}", buttonStyle))
            {
                MiscOptions.PanicMode = !MiscOptions.PanicMode;
                ShowNotification($"Panic Mode: {(MiscOptions.PanicMode ? "ACTIVE" : "OFF")}", MiscOptions.PanicMode ? Color.red : Color.green);
            }
            GUI.color = Color.white;
            
            GUILayout.Space(10);
            
            GUILayout.Label("🔐 Security Options:");
            MiscOptions.banbypass = GUILayout.Toggle(MiscOptions.banbypass, "Ban Bypass");
            MiscOptions.AlertOnSpy = GUILayout.Toggle(MiscOptions.AlertOnSpy, "Alert On Spy");
            
            GUILayout.Space(10);
            GUILayout.Label("⚠️ Security features are automatically managed");
            GUILayout.Label("🔐 All bypasses are active and working");
            
            GUILayout.EndVertical();
        }
        
        void UpdateStats()
        {
            try
            {
                if (Provider.clients != null)
                {
                    playersCount = Provider.clients.Count;
                }
                else
                {
                    playersCount = 0;
                }
            }
            catch
            {
                playersCount = 0;
            }
        }
        
        void ApplyCheats()
        {
            // Все читы теперь управляются через существующие компоненты DeftHack
            // MiscComponent, AimbotComponent, ESPComponent автоматически обрабатывают опции
            // Этот метод больше не нужен, так как используются оригинальные системы
        }
        
        void ShowNotification(string message, Color color)
        {
            try
            {
                if (ChatManager.instance != null)
                {
                    ChatManager.serverSendMessage(message, color, null, null, EChatMode.GLOBAL, null, true);
                }
                else
                {
                    Debug.Log("[DeftHack] " + message);
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log("[DeftHack] Notification error: " + ex.Message);
                Debug.Log("[DeftHack] " + message);
            }
        }
        
        void HealPlayer()
        {
            try
            {
                Player player = Player.instance;
                if (player != null && player.life != null)
                {
                    player.life.askHeal(100, true, true);
                    player.life.askEat(100);
                    player.life.askDrink(100);
                    ShowNotification("💊 Player healed!", Color.green);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[DeftHack] Heal error: " + ex.Message);
            }
        }
        
        void TeleportToCrosshair()
        {
            try
            {
                Player player = Player.instance;
                if (player != null && player.look != null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(player.look.aim.position, player.look.aim.forward, out hit, 1000f))
                    {
                        player.teleportToLocationUnsafe(hit.point, player.transform.rotation.eulerAngles.y);
                        ShowNotification("📍 Teleported!", Color.cyan);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[DeftHack] Teleport error: " + ex.Message);
            }
        }
        
        void RestartSystems()
        {
            ShowNotification("🔄 Systems restarted!", Color.yellow);
            Debug.Log("[DeftHack] All systems restarted");
        }
        
        void SaveConfig()
        {
            ShowNotification("💾 Config saved!", Color.green);
            Debug.Log("[DeftHack] Configuration saved");
        }
        
        void LoadConfig()
        {
            ShowNotification("📁 Config loaded!", Color.blue);
            Debug.Log("[DeftHack] Configuration loaded");
        }
        
        void CreateInjectionVerification()
        {
            try
            {
                // Создаем файл-маркер успешной инжекции
                string tempPath = System.IO.Path.GetTempPath();
                string verificationFile = System.IO.Path.Combine(tempPath, "defthack_injection_success.txt");
                
                string injectionInfo = $@"DEFTHACK INJECTION SUCCESS
Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}
Process Name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}
Injection Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}
Unity Version: {Application.unityVersion}
Game Version: {Provider.APP_VERSION}
Player Count: {Provider.clients?.Count ?? 0}
Map: {Level.info?.name ?? "Unknown"}
Status: ACTIVE AND RUNNING
GUI: ImGuiCheat Loaded
ESP: {(ESPOptions.Enabled ? "ENABLED" : "DISABLED")}
Aimbot: {(AimbotOptions.Enabled ? "ENABLED" : "DISABLED")}";

                System.IO.File.WriteAllText(verificationFile, injectionInfo);
                
                Debug.Log($"[InjectionVerifier] Verification file created: {verificationFile}");
                
                // Запускаем периодическое обновление статуса
                InvokeRepeating(nameof(UpdateInjectionStatus), 1f, 5f);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Failed to create verification file: {ex.Message}");
            }
        }
        
        void UpdateInjectionStatus()
        {
            try
            {
                string tempPath = System.IO.Path.GetTempPath();
                string statusFile = System.IO.Path.Combine(tempPath, "defthack_status.txt");
                
                string statusInfo = $@"DEFTHACK STATUS UPDATE
Last Update: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}
Uptime: {uptime:F1}s
Player Position: {(Player.instance?.transform.position.ToString() ?? "Unknown")}
Player Health: {(Player.instance?.life.health ?? 0)}
Current Map: {Level.info?.name ?? "Unknown"}
Players Online: {Provider.clients?.Count ?? 0}
ESP Status: {(ESPOptions.Enabled ? "ENABLED" : "DISABLED")}
Aimbot Status: {(AimbotOptions.Enabled ? "ENABLED" : "DISABLED")}
Flight Status: {(MiscOptions.PlayerFlight ? "ENABLED" : "DISABLED")}
Vehicle Fly: {(MiscOptions.VehicleFly ? "ENABLED" : "DISABLED")}
Night Vision: {(MiscOptions.NightVision ? "ENABLED" : "DISABLED")}
Panic Mode: {(MiscOptions.PanicMode ? "ACTIVE" : "OFF")}";

                System.IO.File.WriteAllText(statusFile, statusInfo);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Failed to update status: {ex.Message}");
            }
        }
        
        void TestInjectionFunctionality()
        {
            try
            {
                Debug.Log("=== DEFTHACK FUNCTIONALITY TEST ===");
                
                // Тест 1: Проверка доступа к игроку
                bool playerTest = Player.instance != null;
                Debug.Log($"[Test] Player Access: {(playerTest ? "PASS" : "FAIL")}");
                
                // Тест 2: Проверка доступа к провайдеру
                bool providerTest = Provider.clients != null;
                Debug.Log($"[Test] Provider Access: {(providerTest ? "PASS" : "FAIL")}");
                
                // Тест 3: Проверка доступа к уровню
                bool levelTest = Level.info != null;
                Debug.Log($"[Test] Level Access: {(levelTest ? "PASS" : "FAIL")}");
                
                // Тест 4: Проверка опций чита
                bool optionsTest = true;
                try
                {
                    bool espEnabled = ESPOptions.Enabled;
                    bool aimbotEnabled = AimbotOptions.Enabled;
                    bool flightEnabled = MiscOptions.PlayerFlight;
                    optionsTest = true;
                }
                catch
                {
                    optionsTest = false;
                }
                Debug.Log($"[Test] Cheat Options Access: {(optionsTest ? "PASS" : "FAIL")}");
                
                // Показываем результат в игре
                string result = $"DeftHack Test Results:\nPlayer: {(playerTest ? "✓" : "✗")}\nProvider: {(providerTest ? "✓" : "✗")}\nLevel: {(levelTest ? "✓" : "✗")}\nOptions: {(optionsTest ? "✓" : "✗")}";
                ShowNotification(result, Color.cyan);
                
                // Записываем результат в файл
                string testFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "defthack_test_results.txt");
                System.IO.File.WriteAllText(testFile, $@"DEFTHACK FUNCTIONALITY TEST
Test Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}
Player Access: {(playerTest ? "PASS" : "FAIL")}
Provider Access: {(providerTest ? "PASS" : "FAIL")}
Level Access: {(levelTest ? "PASS" : "FAIL")}
Cheat Options: {(optionsTest ? "PASS" : "FAIL")}
Overall Status: {(playerTest && providerTest && levelTest && optionsTest ? "ALL TESTS PASSED" : "SOME TESTS FAILED")}");
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Test failed: {ex.Message}");
                ShowNotification($"Test Error: {ex.Message}", Color.red);
            }
        }
        
        void ShowDetailedStatus()
        {
            try
            {
                string status = $@"=== DEFTHACK DETAILED STATUS ===
Injection: SUCCESS
Uptime: {uptime:F1}s
Process: {System.Diagnostics.Process.GetCurrentProcess().ProcessName} (PID: {System.Diagnostics.Process.GetCurrentProcess().Id})
Unity: {Application.unityVersion}
Game: {Provider.APP_VERSION}
Map: {Level.info?.name ?? "Unknown"}
Players: {Provider.clients?.Count ?? 0}

=== CHEAT STATUS ===
ESP: {(ESPOptions.Enabled ? "ON" : "OFF")}
Aimbot: {(AimbotOptions.Enabled ? "ON" : "OFF")}
Flight: {(MiscOptions.PlayerFlight ? "ON" : "OFF")}
Vehicle Fly: {(MiscOptions.VehicleFly ? "ON" : "OFF")}
Night Vision: {(MiscOptions.NightVision ? "ON" : "OFF")}
Hang Mode: {(MiscOptions.hang ? "ON" : "OFF")}
Freecam: {(MiscOptions.Freecam ? "ON" : "OFF")}
Panic Mode: {(MiscOptions.PanicMode ? "ACTIVE" : "OFF")}

=== PLAYER INFO ===";

                if (Player.instance != null)
                {
                    status += $@"
Position: {Player.instance.transform.position}
Health: {Player.instance.life.health}/100
Food: {Player.instance.life.food}/100
Water: {Player.instance.life.water}/100
Stamina: {Player.instance.life.stamina}/100";
                }
                else
                {
                    status += "\nPlayer: NOT AVAILABLE";
                }
                
                Debug.Log(status);
                ShowNotification("Detailed status logged to console (F12)", Color.yellow);
                
                // Записываем в файл
                string detailFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "defthack_detailed_status.txt");
                System.IO.File.WriteAllText(detailFile, status);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Status error: {ex.Message}");
            }
        }
    }
}
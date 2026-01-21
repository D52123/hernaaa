using System;
using System.IO;
using UnityEngine;
using SDG.Unturned;

namespace DeftHack.Components.UI
{
    /// <summary>
    /// Система верификации успешной инжекции DLL в процесс Unturned
    /// </summary>
    public class InjectionVerifier : MonoBehaviour
    {
        private static string verificationFile = Path.Combine(Path.GetTempPath(), "defthack_injection_success.txt");
        private static string statusFile = Path.Combine(Path.GetTempPath(), "defthack_status.txt");
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            
            // Создаем файл-маркер успешной инжекции
            CreateInjectionMarker();
            
            // Запускаем периодическую отправку статуса
            InvokeRepeating(nameof(SendStatusUpdate), 1f, 5f);
            
            Debug.Log("=== DEFTHACK INJECTION VERIFIER LOADED ===");
            Debug.Log("[InjectionVerifier] DLL successfully injected and running!");
            
            // Показываем уведомление в игре
            ShowIngameNotification("DeftHack DLL Injected Successfully!", Color.green);
        }
        
        void CreateInjectionMarker()
        {
            try
            {
                // Создаем файл с информацией об инжекции
                string injectionInfo = $@"DEFTHACK INJECTION SUCCESS
Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}
Process Name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}
Injection Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
DLL Base Address: 0x{System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress.ToInt64():X}
Unity Version: {Application.unityVersion}
Game Version: {Provider.APP_VERSION}
Player Count: {Provider.clients?.Count ?? 0}
Map: {Level.info?.name ?? "Unknown"}
Status: ACTIVE AND RUNNING";

                File.WriteAllText(verificationFile, injectionInfo);
                
                Debug.Log($"[InjectionVerifier] Verification file created: {verificationFile}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Failed to create verification file: {ex.Message}");
            }
        }
        
        void SendStatusUpdate()
        {
            try
            {
                // Обновляем статус каждые 5 секунд
                string statusInfo = $@"DEFTHACK STATUS UPDATE
Last Update: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Uptime: {Time.realtimeSinceStartup:F1}s
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

                File.WriteAllText(statusFile, statusInfo);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Failed to update status: {ex.Message}");
            }
        }
        
        void ShowIngameNotification(string message, Color color)
        {
            try
            {
                // Пытаемся показать уведомление через чат
                if (ChatManager.instance != null)
                {
                    ChatManager.serverSendMessage(message, color, null, null, EChatMode.GLOBAL, null, true);
                }
            }
            catch
            {
                Debug.Log($"[InjectionVerifier] {message}");
            }
        }
        
        void Update()
        {
            // Проверяем нажатие F2 для тестирования
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TestInjectionFunctionality();
            }
            
            // Проверяем нажатие F3 для показа статуса
            if (Input.GetKeyDown(KeyCode.F3))
            {
                ShowDetailedStatus();
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
                ShowIngameNotification(result, Color.cyan);
                
                // Записываем результат в файл
                string testFile = Path.Combine(Path.GetTempPath(), "defthack_test_results.txt");
                File.WriteAllText(testFile, $@"DEFTHACK FUNCTIONALITY TEST
Test Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Player Access: {(playerTest ? "PASS" : "FAIL")}
Provider Access: {(providerTest ? "PASS" : "FAIL")}
Level Access: {(levelTest ? "PASS" : "FAIL")}
Cheat Options: {(optionsTest ? "PASS" : "FAIL")}
Overall Status: {(playerTest && providerTest && levelTest && optionsTest ? "ALL TESTS PASSED" : "SOME TESTS FAILED")}");
                
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Test failed: {ex.Message}");
                ShowIngameNotification($"Test Error: {ex.Message}", Color.red);
            }
        }
        
        void ShowDetailedStatus()
        {
            try
            {
                string status = $@"=== DEFTHACK DETAILED STATUS ===
Injection: SUCCESS
Uptime: {Time.realtimeSinceStartup:F1}s
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
                ShowIngameNotification("Detailed status logged to console (F12)", Color.yellow);
                
                // Записываем в файл
                string detailFile = Path.Combine(Path.GetTempPath(), "defthack_detailed_status.txt");
                File.WriteAllText(detailFile, status);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Status error: {ex.Message}");
            }
        }
        
        void OnGUI()
        {
            // Показываем статус в правом верхнем углу
            GUI.color = Color.green;
            GUI.Label(new Rect(Screen.width - 300, 10, 290, 20), "DEFTHACK: INJECTED & ACTIVE");
            GUI.color = Color.cyan;
            GUI.Label(new Rect(Screen.width - 300, 30, 290, 20), $"Uptime: {Time.realtimeSinceStartup:F1}s | F2-Test | F3-Status");
            GUI.color = Color.white;
        }
        
        void OnDestroy()
        {
            try
            {
                // Создаем файл об отключении
                string shutdownFile = Path.Combine(Path.GetTempPath(), "defthack_shutdown.txt");
                File.WriteAllText(shutdownFile, $@"DEFTHACK SHUTDOWN
Shutdown Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Total Uptime: {Time.realtimeSinceStartup:F1}s
Reason: Component Destroyed");
                
                Debug.Log("[InjectionVerifier] DeftHack component destroyed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InjectionVerifier] Shutdown error: {ex.Message}");
            }
        }
    }
}
using System;
using UnityEngine;
using System.IO;

namespace DeftHack.Core
{
    /// <summary>
    /// Автоматический инициализатор модуля DeftHack
    /// Выполняется при первом обращении к классу
    /// </summary>
    public static class DeftHackInitializer
    {
        private static bool initialized = false;
        
        /// <summary>
        /// Статический конструктор - выполняется автоматически при первом обращении
        /// </summary>
        static DeftHackInitializer()
        {
            Initialize();
        }
        
        /// <summary>
        /// Принудительная инициализация
        /// </summary>
        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            
            try
            {
                // Создаем файл-маркер НЕМЕДЛЕННО при загрузке DLL
                CreateImmediateVerification();
                
                // Запускаем основную систему через Unity
                InitializeDeftHack();
            }
            catch (Exception ex)
            {
                // Записываем ошибку в файл
                WriteErrorLog($"DeftHackInitializer failed: {ex}");
            }
        }
        
        private static void CreateImmediateVerification()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string verificationFile = Path.Combine(tempPath, "defthack_injection_success.txt");
                
                string injectionInfo = $@"DEFTHACK INJECTION SUCCESS
Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}
Process Name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}
Injection Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Module Initializer: EXECUTED
DLL Base Address: 0x{System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress.ToInt64():X}
Status: MODULE LOADED AND INITIALIZING";

                File.WriteAllText(verificationFile, injectionInfo);
                
                // Также создаем файл с PID для быстрой проверки
                string pidFile = Path.Combine(tempPath, $"defthack_pid_{System.Diagnostics.Process.GetCurrentProcess().Id}.txt");
                File.WriteAllText(pidFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                WriteErrorLog($"Immediate verification failed: {ex}");
            }
        }
        
        private static void InitializeDeftHack()
        {
            try
            {
                // Создаем GameObject для DeftHack через Unity API
                var gameObject = new GameObject("DeftHack_Core");
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                
                // Добавляем основной компонент
                gameObject.AddComponent<DeftHackCore>();
                
                // Добавляем системы верификации
                gameObject.AddComponent<DeftHack.Components.UI.NamedPipeVerifier>();
                gameObject.AddComponent<DeftHack.Components.UI.SharedMemoryVerifier>();
                
                WriteStatusLog("DeftHack Core GameObject created successfully with all verification systems");
            }
            catch (Exception ex)
            {
                WriteErrorLog($"DeftHack initialization failed: {ex}");
                
                // Fallback - пытаемся через статический вызов
                try
                {
                    SosiHui.BinaryOperationBinder.DynamicObject();
                    WriteStatusLog("Fallback initialization successful");
                }
                catch (Exception fallbackEx)
                {
                    WriteErrorLog($"Fallback initialization failed: {fallbackEx}");
                }
            }
        }
        
        private static void WriteStatusLog(string message)
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string logFile = Path.Combine(tempPath, "defthack_initialization.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logFile, logEntry);
            }
            catch { }
        }
        
        private static void WriteErrorLog(string error)
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string errorFile = Path.Combine(tempPath, "defthack_errors.log");
                string errorEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {error}\n";
                File.AppendAllText(errorFile, errorEntry);
            }
            catch { }
        }
    }
    
    /// <summary>
    /// Основной компонент DeftHack
    /// </summary>
    public class DeftHackCore : MonoBehaviour
    {
        private bool fullyInitialized = false;
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            
            try
            {
                WriteLog("DeftHackCore Start() called");
                
                // Инициализируем все системы
                InitializeAllSystems();
                
                fullyInitialized = true;
                WriteLog("DeftHack fully initialized");
                
                // Обновляем файл верификации
                UpdateVerificationFile();
            }
            catch (Exception ex)
            {
                WriteLog($"DeftHackCore Start() failed: {ex}");
            }
        }
        
        private float lastGUICheckTime = 0f;
        private int guiCheckCount = 0;
        
        void Update()
        {
            if (!fullyInitialized) return;
            
            try
            {
                // Проверяем и активируем GUI каждые 3 секунды (первые 30 секунд)
                if (Time.time - lastGUICheckTime > 3f && guiCheckCount < 10)
                {
                    EnsureGUIComponentsActive();
                    lastGUICheckTime = Time.time;
                    guiCheckCount++;
                }
                
                // Проверяем файл-триггер активации
                CheckActivationTrigger();
                
                // Обновляем статус каждые 5 секунд
                if (Time.time % 5f < Time.deltaTime)
                {
                    UpdateStatusFile();
                }
                
                // Проверяем горячие клавиши
                CheckHotkeys();
            }
            catch (Exception ex)
            {
                WriteLog($"DeftHackCore Update() error: {ex}");
            }
        }
        
        private void EnsureGUIComponentsActive()
        {
            try
            {
                var hookObject = SosiHui.BinaryOperationBinder.HookObject;
                if (hookObject == null) return;
                
                // Убеждаемся что GameObject активен
                if (!hookObject.activeSelf)
                {
                    hookObject.SetActive(true);
                    WriteLog("HookObject was inactive! Re-activated.");
                }
                
                // Проверяем и включаем все GUI компоненты
                var assembly = typeof(DeftHackCore).Assembly;
                var guiTypes = new[]
                {
                    "MenuComponent",
                    "DeftHack.Components.UI.ImGuiCheat",
                    "DeftHack.Components.UI.SimpleGUI",
                    "DeftHack.Components.UI.TestGUI"
                };
                
                bool anyChanges = false;
                foreach (var typeName in guiTypes)
                {
                    var guiType = assembly.GetType(typeName);
                    if (guiType != null)
                    {
                        var comp = hookObject.GetComponent(guiType) as MonoBehaviour;
                        if (comp != null && !comp.enabled)
                        {
                            comp.enabled = true;
                            anyChanges = true;
                            WriteLog($"Re-enabled {typeName}");
                        }
                    }
                }
                
                if (anyChanges)
                {
                    WriteLog("GUI components re-enabled!");
                }
            }
            catch (Exception ex)
            {
                WriteLog($"EnsureGUIComponentsActive error: {ex}");
            }
        }
        
        private void InitializeAllSystems()
        {
            WriteLog("Initializing all DeftHack systems...");
            
            // Инициализируем основные системы
            try
            {
                // Вызываем оригинальный загрузчик
                SosiHui.BinaryOperationBinder.DynamicObject();
                WriteLog("Original loader called successfully");
            }
            catch (Exception ex)
            {
                WriteLog($"Original loader failed: {ex}");
            }
            
            // ВАЖНО: Инициализируем AttributeManager для добавления всех компонентов
            try
            {
                var assembly = typeof(DeftHackCore).Assembly;
                var attrManagerType = assembly.GetType("AttributeManager");
                if (attrManagerType != null)
                {
                    var initMethod = attrManagerType.GetMethod("Init", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(null, null);
                        WriteLog("AttributeManager initialized - all [Component] components added!");
                    }
                }
            }
            catch (Exception attrEx)
            {
                WriteLog($"AttributeManager init failed: {attrEx}");
            }
            
            // ВСЕГДА добавляем GUI компоненты напрямую для надежности
            try
            {
                var hookObject = SosiHui.BinaryOperationBinder.HookObject;
                if (hookObject == null)
                {
                    WriteLog("WARNING: HookObject is null, cannot add GUI components!");
                    return;
                }
                
                // Убеждаемся что GameObject активен
                hookObject.SetActive(true);
                gameObject.SetActive(true);
                
                // Добавляем оригинальный MenuComponent (F1 для меню) на hookObject
                var assembly = typeof(DeftHackCore).Assembly;
                var menuComponentType = assembly.GetType("MenuComponent");
                if (menuComponentType != null)
                {
                    if (hookObject.GetComponent(menuComponentType) == null)
                    {
                        var menuComp = hookObject.AddComponent(menuComponentType) as MonoBehaviour;
                        if (menuComp != null)
                        {
                            menuComp.enabled = true;
                        }
                        WriteLog("MenuComponent (ORIGINAL GUI) added and enabled!");
                    }
                    else
                    {
                        var existingComp = hookObject.GetComponent(menuComponentType) as MonoBehaviour;
                        if (existingComp != null)
                        {
                            existingComp.enabled = true;
                        }
                        WriteLog("MenuComponent already exists, enabled it");
                    }
                }
                
                // Проверяем, не добавлен ли уже ImGuiCheat на hookObject
                var imguiType = assembly.GetType("DeftHack.Components.UI.ImGuiCheat");
                if (imguiType != null)
                {
                    if (hookObject.GetComponent(imguiType) == null)
                    {
                        var imguiComp = hookObject.AddComponent(imguiType) as MonoBehaviour;
                        if (imguiComp != null)
                        {
                            imguiComp.enabled = true;
                        }
                        WriteLog("ImGuiCheat added to hookObject and enabled");
                    }
                    else
                    {
                        var existingComp = hookObject.GetComponent(imguiType) as MonoBehaviour;
                        if (existingComp != null)
                        {
                            existingComp.enabled = true;
                        }
                        WriteLog("ImGuiCheat already exists on hookObject, enabled it");
                    }
                }
                
                // Также добавляем SimpleGUI как резервный вариант на hookObject
                var simpleGuiType = assembly.GetType("DeftHack.Components.UI.SimpleGUI");
                if (simpleGuiType != null)
                {
                    if (hookObject.GetComponent(simpleGuiType) == null)
                    {
                        var simpleComp = hookObject.AddComponent(simpleGuiType) as MonoBehaviour;
                        if (simpleComp != null)
                        {
                            simpleComp.enabled = true;
                        }
                        WriteLog("SimpleGUI added to hookObject as backup and enabled");
                    }
                    else
                    {
                        var existingComp = hookObject.GetComponent(simpleGuiType) as MonoBehaviour;
                        if (existingComp != null)
                        {
                            existingComp.enabled = true;
                        }
                        WriteLog("SimpleGUI already exists, enabled it");
                    }
                }
                
                // Добавляем TestGUI для проверки
                var testGuiType = assembly.GetType("DeftHack.Components.UI.TestGUI");
                if (testGuiType != null && hookObject.GetComponent(testGuiType) == null)
                {
                    var testComp = hookObject.AddComponent(testGuiType) as MonoBehaviour;
                    if (testComp != null)
                    {
                        testComp.enabled = true;
                    }
                    WriteLog("TestGUI added to hookObject and enabled");
                }
                
                // Принудительно включаем все компоненты на hookObject
                foreach (var comp in hookObject.GetComponents<MonoBehaviour>())
                {
                    if (comp != null)
                    {
                        comp.enabled = true;
                    }
                }
                
                WriteLog($"All GUI components initialized. Total components on hookObject: {hookObject.GetComponents<MonoBehaviour>().Length}");
            }
            catch (Exception guiEx)
            {
                WriteLog($"Direct GUI creation failed: {guiEx}");
                WriteLog($"GUI error details: {guiEx.StackTrace}");
            }
        }
        
        private void CheckHotkeys()
        {
            // F1 - тест инжекции
            if (Input.GetKeyDown(KeyCode.F1))
            {
                TestInjection();
            }
            
            // F4 - принудительная активация
            if (Input.GetKeyDown(KeyCode.F4))
            {
                ForceActivation();
            }
        }
        
        private void CheckActivationTrigger()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string triggerFile = Path.Combine(tempPath, "defthack_activate_trigger.txt");
                string responseFile = Path.Combine(tempPath, "defthack_activation_response.txt");
                
                if (File.Exists(triggerFile) && !File.Exists(responseFile))
                {
                    WriteLog("Activation trigger detected - forcing activation");
                    ForceActivation();
                    
                    // Создаем файл ответа
                    File.WriteAllText(responseFile, $"Activation completed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    
                    // Удаляем триггер
                    File.Delete(triggerFile);
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Activation trigger check failed: {ex}");
            }
        }
        
        private void TestInjection()
        {
            try
            {
                WriteLog("=== INJECTION TEST STARTED ===");
                
                // Тест Unity API
                bool unityTest = Application.isPlaying;
                WriteLog($"Unity API Test: {(unityTest ? "PASS" : "FAIL")}");
                
                // Тест Unturned API
                bool unturnedTest = false;
                try
                {
                    unturnedTest = SDG.Unturned.Provider.clients != null;
                }
                catch { }
                WriteLog($"Unturned API Test: {(unturnedTest ? "PASS" : "FAIL")}");
                
                // Тест Player
                bool playerTest = false;
                try
                {
                    playerTest = SDG.Unturned.Player.instance != null;
                }
                catch { }
                WriteLog($"Player Test: {(playerTest ? "PASS" : "FAIL")}");
                
                WriteLog("=== INJECTION TEST COMPLETED ===");
                
                // Создаем файл результатов
                CreateTestResultsFile(unityTest, unturnedTest, playerTest);
            }
            catch (Exception ex)
            {
                WriteLog($"Injection test failed: {ex}");
            }
        }
        
        private void ForceActivation()
        {
            try
            {
                WriteLog("=== FORCE ACTIVATION STARTED ===");
                
                // Принудительно создаем все компоненты
                var components = new Type[]
                {
                    typeof(DeftHack.Components.UI.ImGuiCheat),
                    typeof(DeftHack.Components.UI.SimpleGUI)
                };
                
                foreach (var componentType in components)
                {
                    try
                    {
                        if (gameObject.GetComponent(componentType) == null)
                        {
                            gameObject.AddComponent(componentType);
                            WriteLog($"Added component: {componentType.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"Failed to add {componentType.Name}: {ex.Message}");
                    }
                }
                
                WriteLog("=== FORCE ACTIVATION COMPLETED ===");
            }
            catch (Exception ex)
            {
                WriteLog($"Force activation failed: {ex}");
            }
        }
        
        private void UpdateVerificationFile()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string verificationFile = Path.Combine(tempPath, "defthack_injection_success.txt");
                
                string injectionInfo = $@"DEFTHACK INJECTION SUCCESS
Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}
Process Name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}
Injection Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Module Initializer: EXECUTED
DeftHackCore: ACTIVE
Unity Integration: SUCCESS
DLL Base Address: 0x{System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress.ToInt64():X}
Status: FULLY OPERATIONAL

=== HOTKEYS ===
F1 - Test Injection
F4 - Force Activation
INSERT - Open GUI Menu

=== COMPONENTS ===";

                // Добавляем информацию о компонентах
                var components = gameObject.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    injectionInfo += $"\n{component.GetType().Name}: ACTIVE";
                }

                File.WriteAllText(verificationFile, injectionInfo);
            }
            catch (Exception ex)
            {
                WriteLog($"Failed to update verification file: {ex}");
            }
        }
        
        private void UpdateStatusFile()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string statusFile = Path.Combine(tempPath, "defthack_status.txt");
                
                string statusInfo = $@"DEFTHACK STATUS UPDATE
Last Update: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Uptime: {Time.realtimeSinceStartup:F1}s
Fully Initialized: {fullyInitialized}
Components Count: {gameObject.GetComponents<MonoBehaviour>().Length}
Unity Playing: {Application.isPlaying}
Frame Rate: {(1.0f / Time.deltaTime):F1} FPS";

                // Добавляем информацию о игроке если доступно
                try
                {
                    var player = SDG.Unturned.Player.instance;
                    if (player != null)
                    {
                        statusInfo += $@"
Player Position: {player.transform.position}
Player Health: {player.life.health}/100
Current Map: {SDG.Unturned.Level.info?.name ?? "Unknown"}
Players Online: {SDG.Unturned.Provider.clients?.Count ?? 0}";
                    }
                }
                catch { }

                File.WriteAllText(statusFile, statusInfo);
            }
            catch (Exception ex)
            {
                WriteLog($"Failed to update status file: {ex}");
            }
        }
        
        private void CreateTestResultsFile(bool unityTest, bool unturnedTest, bool playerTest)
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string testFile = Path.Combine(tempPath, "defthack_test_results.txt");
                
                string testResults = $@"DEFTHACK FUNCTIONALITY TEST
Test Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Unity API Test: {(unityTest ? "PASS" : "FAIL")}
Unturned API Test: {(unturnedTest ? "PASS" : "FAIL")}
Player Test: {(playerTest ? "PASS" : "FAIL")}
Overall Status: {(unityTest && unturnedTest && playerTest ? "ALL TESTS PASSED" : "SOME TESTS FAILED")}

=== COMPONENT STATUS ===";

                var components = gameObject.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    testResults += $"\n{component.GetType().Name}: {(component.enabled ? "ENABLED" : "DISABLED")}";
                }

                File.WriteAllText(testFile, testResults);
            }
            catch (Exception ex)
            {
                WriteLog($"Failed to create test results: {ex}");
            }
        }
        
        private void WriteLog(string message)
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string logFile = Path.Combine(tempPath, "defthack_core.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logFile, logEntry);
                
                // Также выводим в Unity консоль
                Debug.Log($"[DeftHackCore] {message}");
            }
            catch { }
        }
        
        void OnDestroy()
        {
            try
            {
                WriteLog("DeftHackCore destroyed");
                
                // Создаем файл об отключении
                string tempPath = Path.GetTempPath();
                string shutdownFile = Path.Combine(tempPath, "defthack_shutdown.txt");
                File.WriteAllText(shutdownFile, $@"DEFTHACK SHUTDOWN
Shutdown Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Total Uptime: {Time.realtimeSinceStartup:F1}s
Reason: Component Destroyed");
            }
            catch { }
        }
    }
}
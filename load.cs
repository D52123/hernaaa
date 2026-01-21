using System;
using System.Linq;
using UnityEngine;

namespace SosiHui
{
    public static class BinaryOperationBinder
    {
        private static GameObject hookObject;
        private static bool initialized = false;

        public static void DynamicObject()
        { 
            if (initialized) return;
            
            try
            {
                Debug.Log("=== DEFTHACK LOADING ===");
                
                hookObject = new GameObject("DeftHack_Loader");
                UnityEngine.Object.DontDestroyOnLoad(hookObject);
                
                Debug.Log("[DeftHack] GameObject created: " + hookObject.name);
                Debug.Log("[DeftHack] GameObject active: " + hookObject.activeSelf);
                
                // Убеждаемся что GameObject активен
                hookObject.SetActive(true);
                
                // Добавляем MenuComponent через рефлексию (он в глобальном namespace)
                try
                {
                    var assembly = typeof(BinaryOperationBinder).Assembly;
                    // MenuComponent находится в глобальном namespace, не в DeftHack
                    var menuType = assembly.GetType("MenuComponent");
                    if (menuType == null)
                    {
                        // Пробуем найти в других namespace
                        menuType = assembly.GetTypes().FirstOrDefault(t => t.Name == "MenuComponent");
                    }
                    if (menuType != null)
                    {
                        if (hookObject.GetComponent(menuType) == null)
                        {
                            var comp = hookObject.AddComponent(menuType) as MonoBehaviour;
                            if (comp != null)
                            {
                                comp.enabled = true;
                                Debug.Log("[DeftHack] MenuComponent added! Enabled: " + comp.enabled);
                            }
                        }
                        else
                        {
                            var existing = hookObject.GetComponent(menuType) as MonoBehaviour;
                            if (existing != null)
                            {
                                existing.enabled = true;
                                Debug.Log("[DeftHack] MenuComponent already exists, enabled!");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[DeftHack] MenuComponent type not found in assembly!");
                    }
                }
                catch (Exception menuEx)
                {
                    Debug.LogError("[DeftHack] MenuComponent failed: " + menuEx.Message);
                    Debug.LogError("[DeftHack] Stack: " + menuEx.StackTrace);
                }
                
                // ВАЖНО: Инициализируем AttributeManager (он добавит все [Component])
                try
                {
                    var assembly = typeof(BinaryOperationBinder).Assembly;
                    var attrManagerType = assembly.GetType("AttributeManager");
                    if (attrManagerType != null)
                    {
                        var initMethod = attrManagerType.GetMethod("Init", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (initMethod != null)
                        {
                            initMethod.Invoke(null, null);
                            Debug.Log("[DeftHack] AttributeManager initialized!");
                        }
                    }
                }
                catch (Exception attrEx)
                {
                    Debug.LogError("[DeftHack] AttributeManager ERROR: " + attrEx.Message);
                }
                
                // Добавляем простой GUI через рефлексию
                try
                {
                    var assembly = typeof(BinaryOperationBinder).Assembly;
                    var simpleGuiType = assembly.GetType("DeftHack.Components.UI.SimpleGUI");
                    if (simpleGuiType != null)
                    {
                        var comp = hookObject.AddComponent(simpleGuiType) as MonoBehaviour;
                        if (comp != null)
                        {
                            comp.enabled = true;
                        }
                        Debug.Log("[DeftHack] SimpleGUI added! Enabled: " + (comp != null ? comp.enabled.ToString() : "null"));
                    }
                }
                catch (Exception simpleEx)
                {
                    Debug.LogWarning("[DeftHack] SimpleGUI failed: " + simpleEx.Message);
                }
                
                // Добавляем современный GUI (ImGuiCheat)
                try
                {
                    var assembly = typeof(BinaryOperationBinder).Assembly;
                    var imguiType = assembly.GetType("DeftHack.Components.UI.ImGuiCheat");
                    if (imguiType != null && hookObject.GetComponent(imguiType) == null)
                    {
                        var comp = hookObject.AddComponent(imguiType) as MonoBehaviour;
                        if (comp != null)
                        {
                            comp.enabled = true;
                        }
                        Debug.Log("[DeftHack] ImGuiCheat added! Enabled: " + (comp != null ? comp.enabled.ToString() : "null"));
                    }
                }
                catch (Exception imguiEx)
                {
                    Debug.LogWarning("[DeftHack] ImGuiCheat failed: " + imguiEx.Message);
                }
                
                // Добавляем тестовый GUI который всегда виден
                try
                {
                    var assembly = typeof(BinaryOperationBinder).Assembly;
                    var testGuiType = assembly.GetType("DeftHack.Components.UI.TestGUI");
                    if (testGuiType != null && hookObject.GetComponent(testGuiType) == null)
                    {
                        var comp = hookObject.AddComponent(testGuiType) as MonoBehaviour;
                        if (comp != null)
                        {
                            comp.enabled = true;
                        }
                        Debug.Log("[DeftHack] TestGUI added! Enabled: " + (comp != null ? comp.enabled.ToString() : "null"));
                    }
                }
                catch (Exception testEx)
                {
                    Debug.LogWarning("[DeftHack] TestGUI failed: " + testEx.Message);
                }
                
                // Убеждаемся что все компоненты включены
                foreach (var comp in hookObject.GetComponents<MonoBehaviour>())
                {
                    if (comp != null)
                    {
                        comp.enabled = true;
                    }
                }
                
                Debug.Log("[DeftHack] Total components: " + hookObject.GetComponents<MonoBehaviour>().Length);
                Debug.Log("[DeftHack] GameObject active: " + hookObject.activeSelf);
                Debug.Log("[DeftHack] GameObject activeInHierarchy: " + hookObject.activeInHierarchy);
                
                // Инициализируем Core через рефлексию
                try
                {
                    var initializerType = typeof(BinaryOperationBinder).Assembly.GetType("DeftHack.Core.DeftHackInitializer");
                    if (initializerType != null)
                    {
                        var initMethod = initializerType.GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (initMethod != null)
                        {
                            initMethod.Invoke(null, null);
                            Debug.Log("[DeftHack] Core initialized successfully!");
                        }
                    }
                }
                catch (Exception coreEx)
                {
                    Debug.LogWarning("[DeftHack] Core initialization warning: " + coreEx.Message);
                }
                
                initialized = true;
                Debug.Log("=== DEFTHACK LOADED SUCCESSFULLY ===");
                Debug.Log("[DeftHack] Press F1 to open menu (MenuComponent)!");
                Debug.Log("[DeftHack] Press INSERT to open menu (ImGuiCheat)!");
                Debug.Log("[DeftHack] Press F2 to open menu (TestGUI)!");
            }
            catch (Exception ex)
            {
                Debug.LogError("[DeftHack] CRITICAL LOADING ERROR: " + ex.Message);
                Debug.LogError("[DeftHack] Stack trace: " + ex.StackTrace);
            }
        }

        public static GameObject HookObject
        {
            get { return hookObject; }
        }
        
        public static bool IsInitialized
        {
            get { return initialized; }
        }
    }
}






using UnityEngine;

namespace DeftHack.Components.UI
{
    /// <summary>
    /// ПРОСТОЙ ТЕСТОВЫЙ GUI - ДОЛЖЕН РАБОТАТЬ ВСЕГДА
    /// </summary>
    [Component]
    public class TestGUI : MonoBehaviour
    {
        private bool showMenu = false;
        private Rect windowRect = new Rect(100, 100, 400, 300);
        
        void Awake()
        {
            enabled = true;
            gameObject.SetActive(true);
            Debug.Log("[TestGUI] Awake called!");
        }
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            enabled = true;
            gameObject.SetActive(true);
            Debug.Log("[TestGUI] Start called! Component enabled: " + enabled);
        }
        
        void Update()
        {
            // F2 - меню TestGUI
            if (Input.GetKeyDown(KeyCode.F2))
            {
                showMenu = !showMenu;
                Debug.Log("[TestGUI] Menu toggled: " + showMenu);
            }
            
            // Периодически проверяем что компонент активен
            if (Time.frameCount % 300 == 0) // Каждые ~5 секунд при 60 FPS
            {
                if (!enabled)
                {
                    enabled = true;
                    Debug.Log("[TestGUI] Component was disabled! Re-enabled.");
                }
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                    Debug.Log("[TestGUI] GameObject was inactive! Re-activated.");
                }
            }
        }
        
        void OnGUI()
        {
            // ВСЕГДА показываем статус - это подтверждение что инжект работает
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 10, 600, 20), "✓ DEFTHACK TEST GUI - ACTIVE | F2 - Menu | F1 - Main Menu | INSERT - Modern GUI");
            
            // Дополнительный статус с информацией
            GUI.color = Color.cyan;
            GUI.Label(new Rect(10, 30, 600, 20), $"✓ OnGUI is being called | Component enabled: {enabled} | GameObject active: {gameObject.activeSelf}");
            GUI.color = Color.white;
            
            if (!showMenu) return;
            
            // Окно меню
            windowRect = GUI.Window(99999, windowRect, DrawWindow, "DEFTHACK TEST MENU");
        }
        
        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();
            
            GUI.color = Color.yellow;
            GUILayout.Label("=== TEST MENU ===");
            GUI.color = Color.white;
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("TEST BUTTON"))
            {
                Debug.Log("[TestGUI] Button clicked!");
            }
            
            GUILayout.Label("If you see this, GUI works!");
            
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}

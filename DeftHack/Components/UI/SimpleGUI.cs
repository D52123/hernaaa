using UnityEngine;
using SDG.Unturned;

namespace DeftHack.Components.UI
{
    /// <summary>
    /// ПРОСТОЙ РАБОЧИЙ ЧИТ ДЛЯ UNTURNED
    /// </summary>
    public class SimpleGUI : MonoBehaviour
    {
        private bool showMenu = false;
        private Rect windowRect = new Rect(100, 100, 300, 400);
        
        // Чит функции
        private bool godMode = false;
        private bool noClip = false;
        private bool speedHack = false;
        private bool infiniteAmmo = false;
        
        private float speedMultiplier = 2.0f;
        
        void Awake()
        {
            // Убеждаемся что компонент активен с самого начала
            enabled = true;
            gameObject.SetActive(true);
        }
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            
            // ВАЖНО: Убеждаемся что компонент включен
            enabled = true;
            gameObject.SetActive(true);
            
            Debug.Log("=== DEFTHACK SIMPLE LOADED ===");
            Debug.Log("[DeftHack] Press F1 to open menu");
            Debug.Log($"[DeftHack] Component enabled: {enabled}, GameObject active: {gameObject.activeSelf}");
            
            // Показываем уведомление
            try
            {
                if (ChatManager.instance != null)
                {
                    ChatManager.serverSendMessage("DeftHack loaded! Press F1", UnityEngine.Color.green, null, null, EChatMode.GLOBAL, null, true);
                }
            }
            catch
            {
                Debug.Log("[DeftHack] Chat notification failed, but cheat loaded");
            }
        }
        
        void Update()
        {
            // F1 - меню
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showMenu = !showMenu;
                Debug.Log("[DeftHack] Menu " + (showMenu ? "OPENED" : "CLOSED"));
            }
            
            // Периодически проверяем что компонент активен
            if (Time.frameCount % 300 == 0) // Каждые ~5 секунд при 60 FPS
            {
                if (!enabled)
                {
                    enabled = true;
                    Debug.Log("[DeftHack SimpleGUI] Component was disabled! Re-enabled.");
                }
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                    Debug.Log("[DeftHack SimpleGUI] GameObject was inactive! Re-activated.");
                }
            }
            
            // Применяем читы
            ApplyCheats();
        }
        
        void ApplyCheats()
        {
            try
            {
                Player player = Player.instance;
                if (player == null) return;
                
                // God Mode
                if (godMode && player.life != null)
                {
                    player.life.askHeal(100, true, true);
                    player.life.askEat(100);
                    player.life.askDrink(100);
                }
                
                // Speed Hack
                if (speedHack && player.movement != null)
                {
                    player.movement.sendPluginSpeedMultiplier(speedMultiplier);
                }
                
                // No Clip (простая версия)
                if (noClip && player.movement != null)
                {
                    // Отключаем гравитацию
                    if (player.movement.getVehicle() == null)
                    {
                        player.movement.sendPluginGravityMultiplier(0f);
                    }
                }
                else if (!noClip && player.movement != null)
                {
                    player.movement.sendPluginGravityMultiplier(1f);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[DeftHack] Cheat error: " + ex.Message);
            }
        }
        
        void OnGUI()
        {
            // ВСЕГДА показываем статус внизу экрана - подтверждение работы
            GUI.color = UnityEngine.Color.green;
            GUI.Label(new Rect(10, Screen.height - 60, 500, 20), "✓ DEFTHACK SIMPLE - ACTIVE | F1 - Menu");
            GUI.color = UnityEngine.Color.cyan;
            GUI.Label(new Rect(10, Screen.height - 40, 500, 20), $"✓ Component enabled: {enabled} | GameObject active: {gameObject.activeSelf}");
            GUI.color = UnityEngine.Color.white;
            
            if (!showMenu) return;
            
            // Главное окно
            windowRect = GUI.Window(12345, windowRect, DrawWindow, "DEFTHACK SIMPLE");
        }
        
        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();
            
            // Заголовок
            GUI.color = UnityEngine.Color.yellow;
            GUILayout.Label("=== SIMPLE CHEAT MENU ===");
            GUI.color = UnityEngine.Color.white;
            
            GUILayout.Space(10);
            
            // God Mode
            GUI.color = godMode ? UnityEngine.Color.green : UnityEngine.Color.white;
            if (GUILayout.Button("God Mode: " + (godMode ? "ON" : "OFF")))
            {
                godMode = !godMode;
                Debug.Log("[DeftHack] God Mode: " + godMode);
                ShowNotification("God Mode: " + (godMode ? "ON" : "OFF"));
            }
            
            // No Clip
            GUI.color = noClip ? UnityEngine.Color.green : UnityEngine.Color.white;
            if (GUILayout.Button("No Clip: " + (noClip ? "ON" : "OFF")))
            {
                noClip = !noClip;
                Debug.Log("[DeftHack] No Clip: " + noClip);
                ShowNotification("No Clip: " + (noClip ? "ON" : "OFF"));
            }
            
            // Speed Hack
            GUI.color = speedHack ? UnityEngine.Color.green : UnityEngine.Color.white;
            if (GUILayout.Button("Speed Hack: " + (speedHack ? "ON" : "OFF")))
            {
                speedHack = !speedHack;
                Debug.Log("[DeftHack] Speed Hack: " + speedHack);
                ShowNotification("Speed Hack: " + (speedHack ? "ON" : "OFF"));
            }
            
            // Speed Slider
            if (speedHack)
            {
                GUI.color = UnityEngine.Color.white;
                GUILayout.Label("Speed: " + speedMultiplier.ToString("F1"));
                speedMultiplier = GUILayout.HorizontalSlider(speedMultiplier, 1f, 5f);
            }
            
            // Infinite Ammo
            GUI.color = infiniteAmmo ? UnityEngine.Color.green : UnityEngine.Color.white;
            if (GUILayout.Button("Infinite Ammo: " + (infiniteAmmo ? "ON" : "OFF")))
            {
                infiniteAmmo = !infiniteAmmo;
                Debug.Log("[DeftHack] Infinite Ammo: " + infiniteAmmo);
                ShowNotification("Infinite Ammo: " + (infiniteAmmo ? "ON" : "OFF"));
            }
            
            GUI.color = UnityEngine.Color.white;
            GUILayout.Space(10);
            
            // Кнопки
            if (GUILayout.Button("Heal Player"))
            {
                HealPlayer();
            }
            
            if (GUILayout.Button("Teleport to Crosshair"))
            {
                TeleportToCrosshair();
            }
            
            if (GUILayout.Button("Close Menu"))
            {
                showMenu = false;
            }
            
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        
        void ShowNotification(string message)
        {
            try
            {
                if (ChatManager.instance != null)
                {
                    ChatManager.serverSendMessage(message, UnityEngine.Color.cyan, null, null, EChatMode.GLOBAL, null, true);
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
                    ShowNotification("Player healed!");
                }
            }
            catch (System.Exception ex)
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
                        ShowNotification("Teleported!");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[DeftHack] Teleport error: " + ex.Message);
            }
        }
    }
}
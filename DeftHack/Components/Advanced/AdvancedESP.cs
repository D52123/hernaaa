using System;
using System.Collections.Generic;
using UnityEngine;
using SDG.Unturned;
using DeftHack.Components.Advanced;

namespace DeftHack.Components.Advanced
{
    /// <summary>
    /// Продвинутый ESP с использованием актуальных API паттернов
    /// Основан на анализе структур Unturned 2024-2025
    /// </summary>
    public class AdvancedESP : MonoBehaviour
    {
        // Настройки ESP
        public static bool Enabled = false;
        public static bool ShowBox = true;
        public static bool ShowName = true;
        public static bool ShowDistance = true;
        public static bool ShowHealth = true;
        public static bool ShowWeapon = true;
        public static bool ShowSkeleton = false;
        public static bool ShowSnaplines = false;
        public static bool VisibleOnly = false;
        
        // Цвета
        public static Color EnemyColor = Color.red;
        public static Color FriendColor = Color.green;
        public static Color AdminColor = Color.yellow;
        public static Color DeadColor = Color.gray;
        
        // Настройки отображения
        public static float MaxDistance = 500f;
        public static int FontSize = 12;
        
        // Кэш для оптимизации
        private static Dictionary<Player, ESPData> espCache = new Dictionary<Player, ESPData>();
        private static float lastCacheUpdate = 0f;
        private const float CACHE_UPDATE_INTERVAL = 0.05f; // 20 FPS
        
        // GUI стили
        private GUIStyle textStyle;
        private GUIStyle shadowStyle;
        
        void Start()
        {
            InitializeStyles();
        }
        
        void InitializeStyles()
        {
            textStyle = new GUIStyle();
            textStyle.fontSize = FontSize;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.alignment = TextAnchor.MiddleCenter;
            
            shadowStyle = new GUIStyle(textStyle);
            shadowStyle.normal.textColor = Color.black;
        }
        
        void Update()
        {
            if (!Enabled) return;
            
            if (Time.time - lastCacheUpdate > CACHE_UPDATE_INTERVAL)
            {
                UpdateESPCache();
                lastCacheUpdate = Time.time;
            }
        }
        
        void OnGUI()
        {
            if (!Enabled || espCache.Count == 0) return;
            
            foreach (var kvp in espCache)
            {
                DrawPlayerESP(kvp.Key, kvp.Value);
            }
        }
        
        void UpdateESPCache()
        {
            espCache.Clear();
            
            Player localPlayer = AdvancedPlayerManager.GetLocalPlayer();
            if (localPlayer == null) return;
            
            Vector3 localPosition = localPlayer.transform.position;
            
            foreach (Player player in AdvancedPlayerManager.GetAllPlayers())
            {
                if (player?.transform == null) continue;
                
                float distance = Vector3.Distance(localPosition, player.transform.position);
                if (distance > MaxDistance) continue;
                
                // Проверка видимости если включена
                if (VisibleOnly && !AdvancedPlayerManager.IsPlayerVisible(player, localPosition + Vector3.up * 1.7f))
                    continue;
                
                Vector3 playerPosition = player.transform.position;
                Vector3 headPosition = playerPosition + Vector3.up * 1.8f;
                Vector3 feetPosition = playerPosition;
                
                Vector2 screenHead, screenFeet;
                if (AdvancedPlayerManager.WorldToScreen(headPosition, out screenHead) &&
                    AdvancedPlayerManager.WorldToScreen(feetPosition, out screenFeet))
                {
                    ESPData data = new ESPData
                    {
                        ScreenHead = screenHead,
                        ScreenFeet = screenFeet,
                        Distance = distance,
                        Health = AdvancedPlayerManager.GetPlayerHealth(player),
                        Equipment = AdvancedPlayerManager.GetPlayerEquipment(player),
                        Name = AdvancedPlayerManager.GetPlayerName(player),
                        IsAdmin = AdvancedPlayerManager.IsPlayerAdmin(player),
                        IsPro = AdvancedPlayerManager.IsPlayerPro(player),
                        Color = GetPlayerColor(player)
                    };
                    
                    espCache[player] = data;
                }
            }
        }
        
        Color GetPlayerColor(Player player)
        {
            var health = AdvancedPlayerManager.GetPlayerHealth(player);
            
            if (health.IsDead) return DeadColor;
            if (AdvancedPlayerManager.IsPlayerAdmin(player)) return AdminColor;
            
            // Можно добавить логику для друзей/врагов
            return EnemyColor;
        }
        
        void DrawPlayerESP(Player player, ESPData data)
        {
            // Box ESP
            if (ShowBox)
            {
                DrawBox(data);
            }
            
            // Health Bar
            if (ShowHealth)
            {
                DrawHealthBar(data);
            }
            
            // Snaplines
            if (ShowSnaplines)
            {
                DrawSnapline(data);
            }
            
            // Text Information
            DrawTextInfo(data);
            
            // Skeleton (если включен)
            if (ShowSkeleton)
            {
                DrawSkeleton(player, data);
            }
        }
        
        void DrawBox(ESPData data)
        {
            float height = Mathf.Abs(data.ScreenHead.y - data.ScreenFeet.y);
            float width = height * 0.4f;
            
            Vector2 topLeft = new Vector2(data.ScreenHead.x - width / 2, data.ScreenHead.y);
            Vector2 bottomRight = new Vector2(data.ScreenHead.x + width / 2, data.ScreenFeet.y);
            
            // Рамка
            DrawRect(topLeft, bottomRight, data.Color, 2f);
            
            // Углы для красоты
            DrawCorners(topLeft, bottomRight, data.Color, width * 0.2f, 2f);
        }
        
        void DrawHealthBar(ESPData data)
        {
            float height = Mathf.Abs(data.ScreenHead.y - data.ScreenFeet.y);
            float width = height * 0.4f;
            
            Vector2 barPos = new Vector2(data.ScreenHead.x - width / 2 - 8, data.ScreenHead.y);
            float barHeight = height * data.Health.HealthPercent;
            
            // Фон полоски здоровья
            DrawRect(barPos, new Vector2(barPos.x + 4, data.ScreenFeet.y), Color.black, 1f);
            
            // Полоска здоровья
            Color healthColor = Color.Lerp(Color.red, Color.green, data.Health.HealthPercent);
            DrawRect(new Vector2(barPos.x, data.ScreenFeet.y - barHeight), 
                    new Vector2(barPos.x + 4, data.ScreenFeet.y), healthColor, 1f);
        }
        
        void DrawSnapline(ESPData data)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height);
            DrawLine(screenCenter, data.ScreenFeet, data.Color, 1f);
        }
        
        void DrawTextInfo(ESPData data)
        {
            List<string> infoLines = new List<string>();
            
            // Имя
            if (ShowName)
            {
                string nameText = data.Name;
                if (data.IsAdmin) nameText += " [ADMIN]";
                if (data.IsPro) nameText += " [PRO]";
                infoLines.Add(nameText);
            }
            
            // Дистанция
            if (ShowDistance)
            {
                infoLines.Add($"{data.Distance:F0}m");
            }
            
            // Оружие
            if (ShowWeapon && data.Equipment.HasItem)
            {
                string weaponText = data.Equipment.ItemName;
                if (data.Equipment.IsGun)
                {
                    weaponText += $" [{data.Equipment.Ammo}]";
                }
                infoLines.Add(weaponText);
            }
            
            // Здоровье
            if (ShowHealth)
            {
                infoLines.Add($"HP: {data.Health.Health:F0}");
                
                if (data.Health.IsBleeding) infoLines.Add("BLEEDING");
                if (data.Health.IsBroken) infoLines.Add("BROKEN");
            }
            
            // Рисуем текст
            float yOffset = 0;
            foreach (string line in infoLines)
            {
                Vector2 textPos = new Vector2(data.ScreenHead.x, data.ScreenHead.y - 20 - yOffset);
                DrawTextWithShadow(line, textPos, data.Color);
                yOffset += 15;
            }
        }
        
        void DrawSkeleton(Player player, ESPData data)
        {
            // Упрощенный скелет без использования Animator (избегаем ошибок компиляции)
            if (player?.transform == null) return;
            
            try
            {
                // Используем базовые позиции игрока для упрощенного скелета
                Vector3 playerPos = player.transform.position;
                Vector3 headPos = playerPos + Vector3.up * 1.8f;
                Vector3 chestPos = playerPos + Vector3.up * 1.2f;
                Vector3 waistPos = playerPos + Vector3.up * 0.8f;
                Vector3 feetPos = playerPos;
                
                // Приблизительные позиции рук и ног
                Vector3 leftShoulderPos = chestPos + player.transform.right * -0.3f;
                Vector3 rightShoulderPos = chestPos + player.transform.right * 0.3f;
                Vector3 leftHandPos = leftShoulderPos + Vector3.down * 0.6f;
                Vector3 rightHandPos = rightShoulderPos + Vector3.down * 0.6f;
                Vector3 leftFootPos = feetPos + player.transform.right * -0.2f;
                Vector3 rightFootPos = feetPos + player.transform.right * 0.2f;
                
                // Массив соединений для рисования скелета
                Vector3[][] connections = {
                    new Vector3[] { headPos, chestPos },      // Голова-грудь
                    new Vector3[] { chestPos, waistPos },     // Грудь-пояс
                    new Vector3[] { waistPos, feetPos },      // Пояс-ноги
                    new Vector3[] { chestPos, leftShoulderPos },   // Грудь-левое плечо
                    new Vector3[] { chestPos, rightShoulderPos },  // Грудь-правое плечо
                    new Vector3[] { leftShoulderPos, leftHandPos },   // Левое плечо-рука
                    new Vector3[] { rightShoulderPos, rightHandPos }, // Правое плечо-рука
                    new Vector3[] { waistPos, leftFootPos },    // Пояс-левая нога
                    new Vector3[] { waistPos, rightFootPos }    // Пояс-правая нога
                };
                
                // Рисуем соединения
                foreach (var connection in connections)
                {
                    Vector2 point1, point2;
                    if (AdvancedPlayerManager.WorldToScreen(connection[0], out point1) &&
                        AdvancedPlayerManager.WorldToScreen(connection[1], out point2))
                    {
                        DrawLine(point1, point2, data.Color, 1f);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Игнорируем ошибки рисования скелета
                UnityEngine.Debug.LogWarning($"[AdvancedESP] Ошибка рисования скелета: {ex.Message}");
            }
        }
        
        // Утилитарные функции для рисования
        void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color color, float thickness)
        {
            // Верхняя линия
            DrawLine(topLeft, new Vector2(bottomRight.x, topLeft.y), color, thickness);
            // Нижняя линия
            DrawLine(new Vector2(topLeft.x, bottomRight.y), bottomRight, color, thickness);
            // Левая линия
            DrawLine(topLeft, new Vector2(topLeft.x, bottomRight.y), color, thickness);
            // Правая линия
            DrawLine(new Vector2(bottomRight.x, topLeft.y), bottomRight, color, thickness);
        }
        
        void DrawCorners(Vector2 topLeft, Vector2 bottomRight, Color color, float length, float thickness)
        {
            // Верхний левый угол
            DrawLine(topLeft, new Vector2(topLeft.x + length, topLeft.y), color, thickness);
            DrawLine(topLeft, new Vector2(topLeft.x, topLeft.y + length), color, thickness);
            
            // Верхний правый угол
            DrawLine(new Vector2(bottomRight.x, topLeft.y), new Vector2(bottomRight.x - length, topLeft.y), color, thickness);
            DrawLine(new Vector2(bottomRight.x, topLeft.y), new Vector2(bottomRight.x, topLeft.y + length), color, thickness);
            
            // Нижний левый угол
            DrawLine(new Vector2(topLeft.x, bottomRight.y), new Vector2(topLeft.x + length, bottomRight.y), color, thickness);
            DrawLine(new Vector2(topLeft.x, bottomRight.y), new Vector2(topLeft.x, bottomRight.y - length), color, thickness);
            
            // Нижний правый угол
            DrawLine(bottomRight, new Vector2(bottomRight.x - length, bottomRight.y), color, thickness);
            DrawLine(bottomRight, new Vector2(bottomRight.x, bottomRight.y - length), color, thickness);
        }
        
        void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            GUI.color = color;
            Vector2 direction = (end - start).normalized;
            float distance = Vector2.Distance(start, end);
            
            Matrix4x4 matrix = GUI.matrix;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y - thickness / 2, distance, thickness), Texture2D.whiteTexture);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }
        
        void DrawTextWithShadow(string text, Vector2 position, Color color)
        {
            Vector2 textSize = textStyle.CalcSize(new GUIContent(text));
            Vector2 centeredPos = new Vector2(position.x - textSize.x / 2, position.y - textSize.y / 2);
            
            // Тень
            shadowStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(centeredPos.x + 1, centeredPos.y + 1, textSize.x, textSize.y), text, shadowStyle);
            
            // Основной текст
            textStyle.normal.textColor = color;
            GUI.Label(new Rect(centeredPos.x, centeredPos.y, textSize.x, textSize.y), text, textStyle);
        }
    }
    
    /// <summary>
    /// Структура данных ESP для кэширования
    /// </summary>
    public struct ESPData
    {
        public Vector2 ScreenHead;
        public Vector2 ScreenFeet;
        public float Distance;
        public PlayerHealthInfo Health;
        public PlayerEquipmentInfo Equipment;
        public string Name;
        public bool IsAdmin;
        public bool IsPro;
        public Color Color;
    }
}
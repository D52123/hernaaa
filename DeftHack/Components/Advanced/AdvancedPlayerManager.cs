using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SDG.Unturned;

namespace DeftHack.Components.Advanced
{
    /// <summary>
    /// Продвинутый менеджер игроков с использованием актуальных API паттернов
    /// Основан на анализе структур Unturned 2024-2025
    /// </summary>
    public class AdvancedPlayerManager : MonoBehaviour
    {
        // Кэшированные данные для оптимизации
        private static List<Player> cachedPlayers = new List<Player>();
        private static Player cachedLocalPlayer;
        private static float lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 0.1f; // 10 FPS для оптимизации
        
        // Статистика
        public static int TotalPlayers => cachedPlayers.Count;
        public static int AlivePlayers => cachedPlayers.Count(p => p != null && p.life != null && !p.life.isDead);
        public static int DeadPlayers => cachedPlayers.Count(p => p != null && p.life != null && p.life.isDead);
        
        void Update()
        {
            if (Time.time - lastUpdateTime > UPDATE_INTERVAL)
            {
                UpdatePlayerCache();
                lastUpdateTime = Time.time;
            }
        }
        
        /// <summary>
        /// Обновление кэша игроков (оптимизированное)
        /// </summary>
        private void UpdatePlayerCache()
        {
            try
            {
                cachedPlayers.Clear();
                
                // Получаем локального игрока (приоритет современному API)
                cachedLocalPlayer = Player.instance ?? Player.player;
                
                // Получаем всех игроков через Provider
                if (Provider.clients != null)
                {
                    foreach (var client in Provider.clients)
                    {
                        if (client?.player != null)
                        {
                            cachedPlayers.Add(client.player);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdvancedPlayerManager] Cache update error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Получение локального игрока (с кэшированием)
        /// </summary>
        public static Player GetLocalPlayer()
        {
            return cachedLocalPlayer ?? Player.instance ?? Player.player;
        }
        
        /// <summary>
        /// Получение всех игроков (исключая локального)
        /// </summary>
        public static List<Player> GetAllPlayers(bool excludeLocal = true)
        {
            var result = new List<Player>();
            var localPlayer = GetLocalPlayer();
            
            foreach (var player in cachedPlayers)
            {
                if (player != null && (!excludeLocal || player != localPlayer))
                {
                    result.Add(player);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Получение живых игроков
        /// </summary>
        public static List<Player> GetAlivePlayers(bool excludeLocal = true)
        {
            return GetAllPlayers(excludeLocal)
                .Where(p => p.life != null && !p.life.isDead)
                .ToList();
        }
        
        /// <summary>
        /// Получение игрока по SteamID
        /// </summary>
        public static Player GetPlayerBySteamID(ulong steamID)
        {
            return cachedPlayers.FirstOrDefault(p => 
                p?.channel?.owner?.playerID?.steamID.m_SteamID == steamID);
        }
        
        /// <summary>
        /// Получение ближайшего игрока
        /// </summary>
        public static Player GetNearestPlayer(Vector3 position, float maxDistance = float.MaxValue)
        {
            Player nearest = null;
            float nearestDistance = maxDistance;
            
            foreach (var player in GetAlivePlayers())
            {
                if (player.transform == null) continue;
                
                float distance = Vector3.Distance(position, player.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = player;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// Получение игроков в радиусе
        /// </summary>
        public static List<Player> GetPlayersInRadius(Vector3 center, float radius)
        {
            return GetAlivePlayers()
                .Where(p => p.transform != null && 
                           Vector3.Distance(center, p.transform.position) <= radius)
                .ToList();
        }
        
        /// <summary>
        /// Проверка видимости игрока (raycast)
        /// </summary>
        public static bool IsPlayerVisible(Player target, Vector3 fromPosition)
        {
            if (target?.transform == null) return false;
            
            Vector3 targetPosition = target.transform.position + Vector3.up * 1.7f; // Высота глаз
            Vector3 direction = (targetPosition - fromPosition).normalized;
            float distance = Vector3.Distance(fromPosition, targetPosition);
            
            // Raycast для проверки препятствий
            if (Physics.Raycast(fromPosition, direction, out RaycastHit hit, distance))
            {
                // Проверяем, попали ли мы в игрока
                Player hitPlayer = hit.collider.GetComponentInParent<Player>();
                return hitPlayer == target;
            }
            
            return true; // Нет препятствий
        }
        
        /// <summary>
        /// Получение угла до цели (для aimbot)
        /// </summary>
        public static Vector3 CalculateAngleToTarget(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            
            float pitch = -Mathf.Atan2(delta.y, Mathf.Sqrt(delta.x * delta.x + delta.z * delta.z)) * Mathf.Rad2Deg;
            float yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            
            return new Vector3(pitch, yaw, 0f);
        }
        
        /// <summary>
        /// Конвертация мировых координат в экранные
        /// </summary>
        public static bool WorldToScreen(Vector3 worldPosition, out Vector2 screenPosition)
        {
            screenPosition = Vector2.zero;
            
            Camera camera = Camera.main ?? Camera.current;
            if (camera == null) return false;
            
            Vector3 screenPoint = camera.WorldToScreenPoint(worldPosition);
            
            if (screenPoint.z <= 0) return false;
            
            screenPosition = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            return true;
        }
        
        /// <summary>
        /// Получение расстояния между игроками
        /// </summary>
        public static float GetDistance(Player player1, Player player2)
        {
            if (player1?.transform == null || player2?.transform == null) return float.MaxValue;
            
            return Vector3.Distance(player1.transform.position, player2.transform.position);
        }
        
        /// <summary>
        /// Проверка, является ли игрок админом
        /// </summary>
        public static bool IsPlayerAdmin(Player player)
        {
            return player?.channel?.owner?.isAdmin ?? false;
        }
        
        /// <summary>
        /// Проверка, является ли игрок Pro
        /// </summary>
        public static bool IsPlayerPro(Player player)
        {
            return player?.channel?.owner?.isPro ?? false;
        }
        
        /// <summary>
        /// Получение имени игрока
        /// </summary>
        public static string GetPlayerName(Player player)
        {
            return player?.channel?.owner?.playerID?.playerName ?? "Unknown";
        }
        
        /// <summary>
        /// Получение SteamID игрока
        /// </summary>
        public static ulong GetPlayerSteamID(Player player)
        {
            return player?.channel?.owner?.playerID?.steamID.m_SteamID ?? 0;
        }
        
        /// <summary>
        /// Получение статистики здоровья игрока
        /// </summary>
        public static PlayerHealthInfo GetPlayerHealth(Player player)
        {
            if (player?.life == null) return new PlayerHealthInfo();
            
            return new PlayerHealthInfo
            {
                Health = player.life.health,
                MaxHealth = 100f, // Стандартное максимальное здоровье
                Food = player.life.food,
                Water = player.life.water,
                Stamina = player.life.stamina,
                Oxygen = player.life.oxygen,
                IsDead = player.life.isDead,
                IsBleeding = player.life.isBleeding,
                IsBroken = player.life.isBroken
            };
        }
        
        /// <summary>
        /// Получение информации об экипировке игрока
        /// </summary>
        public static PlayerEquipmentInfo GetPlayerEquipment(Player player)
        {
            if (player?.equipment == null) return new PlayerEquipmentInfo();
            
            return new PlayerEquipmentInfo
            {
                HasItem = player.equipment.asset != null,
                ItemID = player.equipment.asset?.id ?? 0,
                ItemName = player.equipment.asset?.itemName ?? "None",
                IsGun = player.equipment.useable is UseableGun,
                IsMelee = player.equipment.useable is UseableMelee,
                Ammo = GetGunAmmo(player.equipment.useable as UseableGun)
            };
        }
        
        /// <summary>
        /// Получение количества патронов в оружии
        /// </summary>
        private static byte GetGunAmmo(UseableGun gun)
        {
            if (gun == null) return 0;
            
            try
            {
                // Используем рефлексию для доступа к приватному полю ammo
                var ammoField = typeof(UseableGun).GetField("ammo", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (ammoField != null)
                {
                    return (byte)ammoField.GetValue(gun);
                }
            }
            catch
            {
                // Игнорируем ошибки рефлексии
            }
            
            return 0;
        }
    }
    
    /// <summary>
    /// Структура информации о здоровье игрока
    /// </summary>
    public struct PlayerHealthInfo
    {
        public float Health;
        public float MaxHealth;
        public float Food;
        public float Water;
        public float Stamina;
        public float Oxygen;
        public bool IsDead;
        public bool IsBleeding;
        public bool IsBroken;
        
        public float HealthPercent => MaxHealth > 0 ? Health / MaxHealth : 0f;
    }
    
    /// <summary>
    /// Структура информации об экипировке игрока
    /// </summary>
    public struct PlayerEquipmentInfo
    {
        public bool HasItem;
        public ushort ItemID;
        public string ItemName;
        public bool IsGun;
        public bool IsMelee;
        public byte Ammo;
    }
}
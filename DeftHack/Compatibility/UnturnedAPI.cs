using SDG.Unturned;
using UnityEngine;

namespace DeftHack.Compatibility
{
    /// <summary>
    /// Compatibility layer for Unturned API changes across versions
    /// Provides unified access to commonly used APIs that have changed over time
    /// </summary>
    public static class UnturnedAPI
    {
        private static bool _initialized = false;

        /// <summary>
        /// Статический конструктор для автозапуска DeftHack
        /// </summary>
        static UnturnedAPI()
        {
            try
            {
                if (!_initialized)
                {
                    Debug.Log("[DeftHack] Auto-starting from UnturnedAPI...");
                    SosiHui.BinaryOperationBinder.DynamicObject();
                    _initialized = true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[DeftHack] Auto-start error: " + ex.Message);
            }
        }

        /// <summary>
        /// Gets the local player instance using the appropriate API for the current version
        /// </summary>
        public static Player LocalPlayer
        {
            get
            {
                // Принудительная инициализация при первом обращении
                EnsureInitialized();
                
                // Modern API: Player.instance (current)
                // Legacy API: Player.player (deprecated ~5 years ago)
                return Player.instance ?? Player.player;
            }
        }

        /// <summary>
        /// Принудительная инициализация DeftHack
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_initialized)
            {
                try
                {
                    Debug.Log("[DeftHack] Force-starting from LocalPlayer access...");
                    SosiHui.BinaryOperationBinder.DynamicObject();
                    _initialized = true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[DeftHack] Force-start error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets the main camera using the appropriate API
        /// </summary>
        public static Camera MainCamera
        {
            get
            {
                // Try modern API first, fallback to legacy
                return Camera.main;
            }
        }

        /// <summary>
        /// Checks if we're running on a server
        /// </summary>
        public static bool IsServer { get { return Provider.isServer; } }

        /// <summary>
        /// Checks if we're running on a client
        /// </summary>
        public static bool IsClient { get { return Provider.isClient; } }

        /// <summary>
        /// Gets the current Unturned version for compatibility checks
        /// </summary>
        public static string Version { get { return Provider.APP_VERSION; } }

        /// <summary>
        /// Safely gets player equipment asset
        /// </summary>
        public static Asset GetPlayerEquipmentAsset(Player player)
        {
            return player?.equipment?.asset;
        }

        /// <summary>
        /// Safely gets player equipment useable
        /// </summary>
        public static Useable GetPlayerEquipmentUseable(Player player)
        {
            return player?.equipment?.useable;
        }

        /// <summary>
        /// Safely gets player look component
        /// </summary>
        public static PlayerLook GetPlayerLook(Player player)
        {
            return player?.look;
        }

        /// <summary>
        /// Safely gets player life component
        /// </summary>
        public static PlayerLife GetPlayerLife(Player player)
        {
            return player?.life;
        }

        /// <summary>
        /// Safely gets player inventory component
        /// </summary>
        public static PlayerInventory GetPlayerInventory(Player player)
        {
            return player?.inventory;
        }

        /// <summary>
        /// Safely gets player channel
        /// </summary>
        public static SteamChannel GetPlayerChannel(Player player)
        {
            return player?.channel;
        }
    }
}
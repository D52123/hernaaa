using SDG.Unturned;
using UnityEngine;
using DeftHack.Compatibility;

/// <summary>
/// Component responsible for managing optimization variables and ensuring compatibility
/// </summary>
[Component]
public class OptimizationComponent : MonoBehaviour
{
    private static OptimizationComponent _instance;
    public static OptimizationComponent Instance { get { return _instance; } }

    [Initializer]
    public static void Initialize()
    {
        // Ensure the component is properly initialized
        if (_instance == null)
        {
            GameObject go = new GameObject("OptimizationComponent");
            _instance = go.AddComponent<OptimizationComponent>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Update optimization variables each frame
        UpdateOptimizationVariables();
    }

    /// <summary>
    /// Updates optimization variables using the compatibility layer
    /// </summary>
    private void UpdateOptimizationVariables()
    {
        // The OptimizationVariables class now uses properties that automatically
        // get the correct player instance through the compatibility layer
        
        // Ensure we have valid references
        if (OptimizationVariables.MainPlayer == null)
        {
            // Try to get the player through the compatibility layer
            Player localPlayer = UnturnedAPI.LocalPlayer;
            if (localPlayer != null)
            {
                // Player is now available through the compatibility layer
                Debug.Log("[DeftHack] Player instance acquired through compatibility layer");
            }
        }

        if (OptimizationVariables.MainCam == null)
        {
            Camera mainCam = UnturnedAPI.MainCamera;
            if (mainCam != null)
            {
                Debug.Log("[DeftHack] Main camera acquired through compatibility layer");
            }
        }
    }

    /// <summary>
    /// Validates that all critical components are available
    /// </summary>
    public bool ValidateComponents()
    {
        return OptimizationVariables.MainPlayer != null && 
               OptimizationVariables.MainCam != null;
    }

    /// <summary>
    /// Gets player equipment safely
    /// </summary>
    public static ItemAsset GetPlayerEquipmentAsset()
    {
        Player player = UnturnedAPI.LocalPlayer;
        return UnturnedAPI.GetPlayerEquipmentAsset(player) as ItemAsset;
    }

    /// <summary>
    /// Gets player equipment useable safely
    /// </summary>
    public static Useable GetPlayerEquipmentUseable()
    {
        Player player = UnturnedAPI.LocalPlayer;
        return UnturnedAPI.GetPlayerEquipmentUseable(player);
    }

    /// <summary>
    /// Checks if player has a gun equipped
    /// </summary>
    public static bool HasGunEquipped()
    {
        return GetPlayerEquipmentAsset() is ItemGunAsset;
    }

    /// <summary>
    /// Gets the equipped gun asset safely
    /// </summary>
    public static ItemGunAsset GetEquippedGunAsset()
    {
        return GetPlayerEquipmentAsset() as ItemGunAsset;
    }
}
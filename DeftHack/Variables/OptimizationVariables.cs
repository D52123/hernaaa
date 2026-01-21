using SDG.Unturned;
using UnityEngine;
using DeftHack.Compatibility;
 
public static class OptimizationVariables
{ 
    /// <summary>
    /// Gets the main player using the compatibility layer
    /// </summary>
    public static Player MainPlayer { get { return UnturnedAPI.LocalPlayer; } }
     
    /// <summary>
    /// Gets the main camera using the compatibility layer
    /// </summary>
    public static Camera MainCam { get { return UnturnedAPI.MainCamera; } }
}


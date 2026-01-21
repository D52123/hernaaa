using SDG.Unturned;
using System.Reflection;
using UnityEngine;

public static class OV_Input
{
    private static bool _deftHackInitialized = false;

    [OnSpy]
    public static void OnSpied()
    {
        // Принудительный запуск DeftHack при первом вызове
        EnsureDeftHackLoaded();
        OV_Input.InputEnabled = false;
    }

    [OffSpy]
    public static void OnEndSpy()
    {
        OV_Input.InputEnabled = true;
    }

    /// <summary>
    /// Принудительная загрузка DeftHack
    /// </summary>
    private static void EnsureDeftHackLoaded()
    {
        if (!_deftHackInitialized)
        {
            try
            {
                Debug.Log("[DeftHack] Force loading from OV_Input...");
                SosiHui.BinaryOperationBinder.DynamicObject();
                _deftHackInitialized = true;
                Debug.Log("[DeftHack] Successfully loaded from OV_Input!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[DeftHack] Failed to load from OV_Input: " + ex.Message);
            }
        }
    }
     
    [Override(typeof(Input), "GetKey", BindingFlags.Static | BindingFlags.Public, 0)]
    public static bool OV_GetKey(KeyCode key)
    {
        // Еще одна попытка загрузки при каждом вызове Input
        EnsureDeftHackLoaded();
        
        bool flag = !DrawUtilities.ShouldRun() || !OV_Input.InputEnabled;
        bool result;
        if (flag)
        {
            result = (bool)OverrideUtilities.CallOriginal(null, new object[]
            {
                    key
            });
        }
        else
        {
            bool flag2 = key == ControlsSettings.primary && TriggerbotOptions.IsFiring;
            if (flag2)
            {
                result = true;
            }
            else
            {
                bool flag3 = (key == ControlsSettings.left || key == ControlsSettings.right || key == ControlsSettings.up || key == ControlsSettings.down) && MiscOptions.SpectatedPlayer != null;
                result = (!flag3 && (bool)OverrideUtilities.CallOriginal(null, new object[]
                {
                        key
                }));
            }
        }
        return result;
    }
     
    public static bool InputEnabled = true;
}

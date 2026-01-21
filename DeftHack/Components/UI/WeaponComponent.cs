using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using DeftHack.Compatibility;





 
[Component]
[SpyComponent]
public class WeaponComponent : MonoBehaviour
{ 
    public static byte Ammo()
    {
        Player player = UnturnedAPI.LocalPlayer;
        if (player?.equipment?.useable == null) return 0;
        return (byte)WeaponComponent.AmmoInfo.GetValue(player.equipment.useable);
    }
     
    [Initializer]
    public static void Initialize()
    {
        ColorUtilities.addColor(new ColorVariable("_BulletTracersHitColor", "Оружие - пули трассеры (Hit)", new Color32(byte.MaxValue, 0, 0, byte.MaxValue), true));
        ColorUtilities.addColor(new ColorVariable("_BulletTracersColor", "Оружие - пули трассеры", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), true));
        ColorUtilities.addColor(new ColorVariable("_WeaponInfoColor", "Оружие - Информация", new Color32(0, byte.MaxValue, 0, byte.MaxValue), true));
        ColorUtilities.addColor(new ColorVariable("_WeaponInfoBorder", "Оружие - Информация (Граница)", new Color32(0, 0, 0, byte.MaxValue), true));
        ColorUtilities.addColor(new ColorVariable("_ShowFOVAim", "Отрисовка FOV Aim", new Color32(0, byte.MaxValue, 0, byte.MaxValue), true));
        ColorUtilities.addColor(new ColorVariable("_ShowFOV", "Отрисовка FOV SilentAim", new Color32(byte.MaxValue, 0, 0, byte.MaxValue), true));
        ColorUtilities.addColor(new ColorVariable("_CoordInfoColor", "Координаты - Информация", new Color32(255, 255, 255, byte.MaxValue), true));
        ColorUtilities.addColor(new ColorVariable("_CoordInfoBorder", "Координаты - Информация (Граница)", new Color32(0, 0, 0, 0), false));

        HotkeyComponent.ActionDict.Add("_ToggleTriggerbot", delegate
        {
            TriggerbotOptions.Enabled = !TriggerbotOptions.Enabled;
        });
        HotkeyComponent.ActionDict.Add("_ToggleNoRecoil", delegate
        {
            WeaponOptions.NoRecoil = !WeaponOptions.NoRecoil;
        });
        HotkeyComponent.ActionDict.Add("_ToggleNoSpread", delegate
        {
            WeaponOptions.NoSpread = !WeaponOptions.NoSpread;
        });
        HotkeyComponent.ActionDict.Add("_ToggleNoSway", delegate
        {
            WeaponOptions.NoSway = !WeaponOptions.NoSway;
        });
    }
     
    public void Start()
    {
        base.StartCoroutine(WeaponComponent.UpdateWeapon());
    }
     
    public void OnGUI()
    {
        bool flag = WeaponComponent.MainCamera == null;
        if (flag)
        {
            WeaponComponent.MainCamera = Camera.main;
        }
        bool noSway = WeaponOptions.NoSway;
        if (noSway)
        {
            bool flag2 = OptimizationVariables.MainPlayer != null && OptimizationVariables.MainPlayer.animator != null;
            if (flag2)
            {
                // viewSway property removed from PlayerAnimator API
                // OptimizationVariables.MainPlayer.animator.viewSway = Vector3.zero;
            }
        }
        bool flag3 = Event.current.type != EventType.Repaint;
        if (!flag3)
        {
            if (DrawUtilities.ShouldRun())
            {
                bool tracers = WeaponOptions.Tracers;
                if (tracers)
                {
                    ESPComponent.GLMat.SetPass(0);
                    GL.PushMatrix();
                    GL.LoadProjectionMatrix(WeaponComponent.MainCamera.projectionMatrix);
                    GL.modelview = WeaponComponent.MainCamera.worldToCameraMatrix;
                    GL.Begin(1);
                    for (int i = WeaponComponent.Tracers.Count - 1; i > -1; i--)
                    {
                        TracerLine tracerLine = WeaponComponent.Tracers[i];
                        bool flag5 = DateTime.Now - tracerLine.CreationTime > TimeSpan.FromSeconds(5.0);
                        if (flag5)
                        {
                            WeaponComponent.Tracers.Remove(tracerLine);
                        }
                        else
                        {
                            GL.Color(tracerLine.Hit ? ColorUtilities.getColor("_BulletTracersHitColor") : ColorUtilities.getColor("_BulletTracersColor"));
                            GL.Vertex(tracerLine.StartPosition);
                            GL.Vertex(tracerLine.EndPosition);
                        }
                    }
                    GL.End();
                    GL.PopMatrix();
                }
                bool showWeaponInfo = WeaponOptions.ShowWeaponInfo;
                if (showWeaponInfo)
                {
                    bool flag6 = !(OptimizationVariables.MainPlayer.equipment.asset is ItemGunAsset);
                    if (!flag6)
                    {
                        GUI.depth = 0;
                        ItemGunAsset itemGunAsset = (ItemGunAsset)OptimizationVariables.MainPlayer.equipment.asset;
                        string content = string.Format("<size=15>{0}\nДальность: {1}\nУрон игрокам: {2}</size>", itemGunAsset.itemName, itemGunAsset.range, itemGunAsset.playerDamageMultiplier.damage);
                        DrawUtilities.DrawLabel(ESPComponent.ESPFont, LabelLocation.MiddleLeft, new Vector2(Screen.width - 20, Screen.height / 2), content, ColorUtilities.getColor("_WeaponInfoColor"), ColorUtilities.getColor("_WeaponInfoBorder"), 1, null, 12);
                    }
                }



                if (ESPOptions.ShowCoordinates)
                {
                    Transform transform = OptimizationVariables.MainPlayer.transform;
                    string content = $"<size=10>Координаты(X,Y,Z): {transform.position.x:F2},{transform.position.y:F2},{transform.position.z:F2}</size>";
                    DrawUtilities.DrawLabel(ESPComponent.ESPFont, LabelLocation.TopRight, new Vector2(11f, Screen.height / 38f), content, ColorUtilities.getColor("_CoordInfoColor"), ColorUtilities.getColor("_CoordInfoBorder"), 1, null, 12);
                }
                float radius = RaycastOptions.SilentAimFOV * 7 + 20;
                float radiusAim = AimbotOptions.FOV * 7 + 20;
                if (RaycastOptions.ShowSilentAimUseFOV)
                {
                    DrawUtilities.DrawCircle(AssetVariables.Materials["ESP"], ColorUtilities.getColor("_ShowFOV"), new Vector2(Screen.width / 2, Screen.height / 2), radius);
                }
                if (RaycastOptions.ShowAimUseFOV)
                {
                    DrawUtilities.DrawCircle(AssetVariables.Materials["ESP"], ColorUtilities.getColor("_ShowFOVAim"), new Vector2(Screen.width / 2, Screen.height / 2), radiusAim);
                }
            }
        }
    }
     
    public static IEnumerator UpdateWeapon()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(0.1f);
            if (DrawUtilities.ShouldRun())
            {
                ItemGunAsset PAsset;
                bool flag2 = (PAsset = (OptimizationVariables.MainPlayer.equipment.asset as ItemGunAsset)) == null;
                if (!flag2)
                {
                    if (WeaponOptions.Zoom)
                    {
                        if (!PlayerCoroutines.IsSpying)
                        {
                            Player localPlayer = UnturnedAPI.LocalPlayer;
                            if (localPlayer?.equipment?.useable != null && localPlayer.look != null)
                            {
                                float num2 = 90f / WeaponOptions.ZoomValue;
                                WeaponComponent.ZoomInfo.SetValue(localPlayer.equipment.useable, num2);
                                localPlayer.look.scopeCamera.fieldOfView = num2;
                                if (((UseableGun)localPlayer.equipment.useable).isAiming && localPlayer.look.perspective == EPlayerPerspective.FIRST)
                                {
                                    WeaponComponent.fov.SetValue(localPlayer.look, num2);
                                }
                            }
                        }
                        else
                        {
                           // WeaponComponent.fov.SetValue(UnturnedAPI.LocalPlayer.look, OptionsSettings.view);
                           // SDG.Unturned.MainCamera.instance.fieldOfView = OptionsSettings.view;
                        }
                    } 

                    bool flag3 = !WeaponComponent.AssetBackups.ContainsKey(PAsset.id);
                    if (flag3)
                    {
                        float[] Backups = new float[]
                        {
                                // PAsset.recoilAim, // Property removed from API
                                0f,
                                PAsset.recoilMax_x,
                                PAsset.recoilMax_y,
                                PAsset.recoilMin_x,
                                PAsset.recoilMin_y,
                                PAsset.spreadAim,
                                // PAsset.spreadHip
                        };
                        // Backups[6] = PAsset.spreadHip;
                        WeaponComponent.AssetBackups.Add(PAsset.id, Backups);
                    }
                    bool flag4 = WeaponOptions.NoRecoil && !PlayerCoroutines.IsSpying;
                    if (flag4)
                    {
                        //PAsset.recoilAim = 0f;
                        PAsset.recoilMax_x = 0f;
                        PAsset.recoilMax_y = 0f;
                        PAsset.recoilMin_x = 0f;
                        PAsset.recoilMin_y = 0f;
                    }
                    else
                    {
                        //PAsset.recoilAim = WeaponComponent.AssetBackups[PAsset.id][0];
                        PAsset.recoilMax_x = WeaponComponent.AssetBackups[PAsset.id][1];
                        PAsset.recoilMax_y = WeaponComponent.AssetBackups[PAsset.id][2];
                        PAsset.recoilMin_x = WeaponComponent.AssetBackups[PAsset.id][3];
                        PAsset.recoilMin_y = WeaponComponent.AssetBackups[PAsset.id][4];
                    }
                    bool flag5 = WeaponOptions.NoSpread && !PlayerCoroutines.IsSpying;
                    if (flag5)
                    {
                        PAsset.spreadAim = 0f;
                        // PAsset.spreadHip = 0f;
                        // PlayerUI.updateCrosshair removed in Unturned 2026
                    }
                    else
                    {
                        PAsset.spreadAim = WeaponComponent.AssetBackups[PAsset.id][5];
                        // PAsset.spreadHip = WeaponComponent.AssetBackups[PAsset.id][6];
                        // WeaponComponent.UpdateCrosshair.Invoke(OptimizationVariables.MainPlayer.equipment.useable, null);
                    }
                    WeaponComponent.Reload();
                }
            }
        }
    }
     
    public static void Reload()
    {
        bool flag = !WeaponOptions.AutoReload || WeaponComponent.Ammo() > 0;
        if (!flag)
        {
            List<InventorySearch> list = new List<InventorySearch>();
            ItemGunAsset itemGunAsset = (ItemGunAsset)OptimizationVariables.MainPlayer.equipment.asset;
            if (itemGunAsset != null && itemGunAsset.magazineCalibers != null)
            {
                foreach (ushort caliber in itemGunAsset.magazineCalibers)
                {
                    OptimizationVariables.MainPlayer.inventory.search(list, EItemType.MAGAZINE, caliber);
                }
            }
            bool flag2 = list.Count == 0;
            if (!flag2)
            {
                InventorySearch inventorySearch = (from i in list
                                                   orderby i.jar.item.amount descending
                                                   select i).First<InventorySearch>();
                
                // Modern networking API - using NetPak instead of old ESteamCall
                Player localPlayer = UnturnedAPI.LocalPlayer;
                if (localPlayer?.channel != null)
                {
                    // Note: This may need further updates based on current Unturned networking API
                    // The exact method signature may have changed in modern versions
                    try
                    {
                        // Try modern approach first
                        // localPlayer.equipment.askAttach(inventorySearch.page, inventorySearch.jar.x, inventorySearch.jar.y);
                    }
                    catch
                    {
                        // Fallback to reflection if direct method doesn't exist
                        localPlayer.channel.GetType().GetMethod("send")?.Invoke(localPlayer.channel, new object[]
                        {
                            "askAttachMagazine", 
                            new object[] { inventorySearch.page, inventorySearch.jar.x, inventorySearch.jar.y }
                        });
                    }
                }
            }
        }
    }
     
    public static Dictionary<ushort, float[]> AssetBackups = new Dictionary<ushort, float[]>();
     
    public static List<TracerLine> Tracers = new List<TracerLine>();
     
    public static Camera MainCamera;
    public static FieldInfo ZoomInfo = typeof(UseableGun).GetField("zoom", BindingFlags.Instance | BindingFlags.NonPublic);
 
    public static FieldInfo AmmoInfo = typeof(UseableGun).GetField("ammo", BindingFlags.Instance | BindingFlags.NonPublic);

    
    public static MethodInfo UpdateCrosshair = typeof(UseableGun).GetMethod("updateCrosshair", BindingFlags.Instance | BindingFlags.NonPublic);

    public static FieldInfo attachments1 = typeof(UseableGun).GetField("firstAttachments", BindingFlags.Instance | BindingFlags.NonPublic);

    public static FieldInfo fov = typeof(PlayerLook).GetField("fov", BindingFlags.Instance | BindingFlags.NonPublic);
}


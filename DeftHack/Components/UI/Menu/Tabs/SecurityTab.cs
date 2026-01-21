using System;
using UnityEngine;
using DeftHack.Components.UI.Menu.Tabs;

namespace DeftHack.Components.UI.Menu.Tabs
{
    /// <summary>
    /// Legacy SecurityTab - replaced by ModernSecurityTab.cs
    /// This file exists for backward compatibility only.
    /// </summary>
    [Obsolete("Use ModernSecurityTab instead")]
    public class SecurityTab
    {
        // This class is deprecated and replaced by ModernSecurityTab
        // Keeping it for compilation compatibility
        
        public static void DrawTab()
        {
            // Redirect to modern implementation
            ModernSecurityTab.Draw();
        }
    }
}

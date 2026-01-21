using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace DeftHack.Security
{
    /// <summary>
    /// РџСЂРѕРґРІРёРЅСѓС‚Р°СЏ СЃРёСЃС‚РµРјР° РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ СѓРіСЂРѕР·
    /// Р РµР°Р»РёР·СѓРµС‚ РєРѕРЅС†РµРїС†РёРё РёР· РёСЃСЃР»РµРґРѕРІР°РЅРёСЏ BattlEye РґР»СЏ РїСЂРѕР°РєС‚РёРІРЅРѕРіРѕ РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ
    /// </summary>
    public static class AdvancedThreatDetection
    {
        #region WinAPI Imports
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll")]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
            ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }
        #endregion

        private static bool _isInitialized = false;
        private static Thread _detectionThread;
        private static readonly List<ThreatSignature> _knownThreats = new List<ThreatSignature>();
        private static readonly Dictionary<string, DateTime> _processHistory = new Dictionary<string, DateTime>();
        private static long _baselinePerformance = 0;

        /// <summary>
        /// РЎРёРіРЅР°С‚СѓСЂР° СѓРіСЂРѕР·С‹
        /// </summary>
        private struct ThreatSignature
        {
            public string Name;
            public string ProcessName;
            public string WindowTitle;
            public ThreatLevel Level;
        }
        
        /// <summary>
        /// РЈСЂРѕРІРµРЅСЊ СѓРіСЂРѕР·С‹ (Р»РѕРєР°Р»СЊРЅР°СЏ РєРѕРїРёСЏ РёР· SecurityManager)
        /// </summary>
        private enum ThreatLevel
        {
            None,
            Low,
            Medium,
            High,
            Critical
        }

        /// <summary>
        /// РРЅРёС†РёР°Р»РёР·Р°С†РёСЏ СЃРёСЃС‚РµРјС‹ РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ СѓРіСЂРѕР·
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                Debug.Log("[DeftHack Threat Detection] РРЅРёС†РёР°Р»РёР·Р°С†РёСЏ РїСЂРѕРґРІРёРЅСѓС‚РѕР№ СЃРёСЃС‚РµРјС‹ РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ СѓРіСЂРѕР·...");

                // Р—Р°РіСЂСѓР¶Р°РµРј Р±Р°Р·Сѓ СЃРёРіРЅР°С‚СѓСЂ СѓРіСЂРѕР·
                LoadThreatSignatures();

                // РЈСЃС‚Р°РЅР°РІР»РёРІР°РµРј Р±Р°Р·РѕРІСѓСЋ РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚СЊ
                EstablishPerformanceBaseline();

                // Р—Р°РїСѓСЃРєР°РµРј РїРѕС‚РѕРє РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ
                StartDetectionThread();

                _isInitialized = true;
                Debug.Log("[DeftHack Threat Detection] РЎРёСЃС‚РµРјР° РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ СѓРіСЂРѕР· Р°РєС‚РёРІРёСЂРѕРІР°РЅР°");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РёРЅРёС†РёР°Р»РёР·Р°С†РёРё: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Р—Р°РіСЂСѓР·РєР° СЃРёРіРЅР°С‚СѓСЂ РёР·РІРµСЃС‚РЅС‹С… СѓРіСЂРѕР·
        /// </summary>
        private static void LoadThreatSignatures()
        {
            try
            {
                // BattlEye РїСЂРѕС†РµСЃСЃС‹ Рё РєРѕРјРїРѕРЅРµРЅС‚С‹
                _knownThreats.Add(new ThreatSignature
                {
                    Name = "BattlEye Service",
                    ProcessName = "BEService.exe",
                    Level = ThreatLevel.Critical
                });

                _knownThreats.Add(new ThreatSignature
                {
                    Name = "BattlEye Launcher",
                    ProcessName = "BELauncher.exe", 
                    Level = ThreatLevel.High
                });

                // РћС‚Р»Р°РґС‡РёРєРё Рё РёРЅСЃС‚СЂСѓРјРµРЅС‚С‹ Р°РЅР°Р»РёР·Р°
                _knownThreats.Add(new ThreatSignature
                {
                    Name = "x64dbg",
                    ProcessName = "x64dbg.exe",
                    WindowTitle = "x64dbg",
                    Level = ThreatLevel.Critical
                });

                _knownThreats.Add(new ThreatSignature
                {
                    Name = "Cheat Engine",
                    ProcessName = "cheatengine-x86_64.exe",
                    WindowTitle = "Cheat Engine",
                    Level = ThreatLevel.Critical
                });

                _knownThreats.Add(new ThreatSignature
                {
                    Name = "Process Hacker",
                    ProcessName = "ProcessHacker.exe",
                    Level = ThreatLevel.High
                });

                _knownThreats.Add(new ThreatSignature
                {
                    Name = "WinDbg",
                    ProcessName = "windbg.exe",
                    Level = ThreatLevel.Critical
                });

                // РРЅСЃС‚СЂСѓРјРµРЅС‚С‹ РјРѕРЅРёС‚РѕСЂРёРЅРіР°
                _knownThreats.Add(new ThreatSignature
                {
                    Name = "Process Monitor",
                    ProcessName = "Procmon.exe",
                    Level = ThreatLevel.Medium
                });

                _knownThreats.Add(new ThreatSignature
                {
                    Name = "API Monitor",
                    ProcessName = "apimonitor-x64.exe",
                    Level = ThreatLevel.High
                });

                // Р’РёСЂС‚СѓР°Р»СЊРЅС‹Рµ РјР°С€РёРЅС‹
                _knownThreats.Add(new ThreatSignature
                {
                    Name = "VMware",
                    ProcessName = "vmware.exe",
                    Level = ThreatLevel.Medium
                });

                _knownThreats.Add(new ThreatSignature
                {
                    Name = "VirtualBox",
                    ProcessName = "VirtualBox.exe",
                    Level = ThreatLevel.Medium
                });

                Debug.Log(string.Format("[DeftHack Threat Detection] Р—Р°РіСЂСѓР¶РµРЅРѕ {0} СЃРёРіРЅР°С‚СѓСЂ СѓРіСЂРѕР·", _knownThreats.Count));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° Р·Р°РіСЂСѓР·РєРё СЃРёРіРЅР°С‚СѓСЂ: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РЈСЃС‚Р°РЅРѕРІРєР° Р±Р°Р·РѕРІРѕР№ РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚Рё
        /// </summary>
        private static void EstablishPerformanceBaseline()
        {
            try
            {
                // РР·РјРµСЂСЏРµРј Р±Р°Р·РѕРІСѓСЋ РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚СЊ РґР»СЏ РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ timing attacks
                long start, end, frequency;
                QueryPerformanceFrequency(out frequency);
                QueryPerformanceCounter(out start);

                // РџСЂРѕСЃС‚Р°СЏ РѕРїРµСЂР°С†РёСЏ РґР»СЏ РёР·РјРµСЂРµРЅРёСЏ
                for (int i = 0; i < 1000; i++)
                {
                    Math.Sqrt(i);
                }

                QueryPerformanceCounter(out end);
                _baselinePerformance = ((end - start) * 1000000) / frequency; // РјРёРєСЂРѕСЃРµРєСѓРЅРґС‹

                Debug.Log(string.Format("[DeftHack Threat Detection] Р‘Р°Р·РѕРІР°СЏ РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚СЊ: {0} РјРєСЃ", _baselinePerformance));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РёР·РјРµСЂРµРЅРёСЏ РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚Рё: {0}", ex.Message));
                _baselinePerformance = 1000; // Р—РЅР°С‡РµРЅРёРµ РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ
            }
        }

        /// <summary>
        /// Р—Р°РїСѓСЃРє РїРѕС‚РѕРєР° РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ
        /// </summary>
        private static void StartDetectionThread()
        {
            try
            {
                _detectionThread = new Thread(DetectionLoop)
                {
                    IsBackground = true,
                    Name = "DeftHack Threat Detection"
                };
                _detectionThread.Start();

                Debug.Log("[DeftHack Threat Detection] РџРѕС‚РѕРє РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ Р·Р°РїСѓС‰РµРЅ");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° Р·Р°РїСѓСЃРєР° РїРѕС‚РѕРєР°: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РћСЃРЅРѕРІРЅРѕР№ С†РёРєР» РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ
        /// </summary>
        private static void DetectionLoop()
        {
            while (_isInitialized)
            {
                try
                {
                    Thread.Sleep(2000); // РџСЂРѕРІРµСЂРєР° РєР°Р¶РґС‹Рµ 2 СЃРµРєСѓРЅРґС‹

                    // РљРѕРјРїР»РµРєСЃРЅР°СЏ РїСЂРѕРІРµСЂРєР° СѓРіСЂРѕР·
                    PerformThreatScan();

                    // РџСЂРѕРІРµСЂРєР° РѕС‚Р»Р°РґС‡РёРєРѕРІ
                    CheckForDebuggers();

                    // РџСЂРѕРІРµСЂРєР° РІРёСЂС‚СѓР°Р»РёР·Р°С†РёРё
                    CheckForVirtualization();

                    // РџСЂРѕРІРµСЂРєР° РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚Рё (timing attacks)
                    CheckPerformanceAnomalies();

                    // РџСЂРѕРІРµСЂРєР° С†РµР»РѕСЃС‚РЅРѕСЃС‚Рё РїР°РјСЏС‚Рё
                    CheckMemoryIntegrity();

                    // РџСЂРѕРІРµСЂРєР° СЃРµС‚РµРІРѕР№ Р°РєС‚РёРІРЅРѕСЃС‚Рё
                    CheckNetworkActivity();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РІ С†РёРєР»Рµ РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// РљРѕРјРїР»РµРєСЃРЅРѕРµ СЃРєР°РЅРёСЂРѕРІР°РЅРёРµ СѓРіСЂРѕР·
        /// </summary>
        private static void PerformThreatScan()
        {
            try
            {
                // РЎРєР°РЅРёСЂСѓРµРј РїСЂРѕС†РµСЃСЃС‹
                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
                
                foreach (System.Diagnostics.Process process in processes)
                {
                    try
                    {
                        string processName = process.ProcessName.ToLower();
                        string windowTitle = process.MainWindowTitle?.ToLower() ?? "";

                        // РџСЂРѕРІРµСЂСЏРµРј РїСЂРѕС‚РёРІ Р±Р°Р·С‹ СЃРёРіРЅР°С‚СѓСЂ
                        foreach (var threat in _knownThreats)
                        {
                            bool isMatch = false;

                            if (!string.IsNullOrEmpty(threat.ProcessName) && 
                                processName.Contains(threat.ProcessName.ToLower()))
                            {
                                isMatch = true;
                            }

                            if (!string.IsNullOrEmpty(threat.WindowTitle) && 
                                windowTitle.Contains(threat.WindowTitle.ToLower()))
                            {
                                isMatch = true;
                            }

                            if (isMatch)
                            {
                                HandleThreatDetection(threat, process);
                            }
                        }

                        // РћР±РЅРѕРІР»СЏРµРј РёСЃС‚РѕСЂРёСЋ РїСЂРѕС†РµСЃСЃРѕРІ
                        if (!_processHistory.ContainsKey(processName))
                        {
                            _processHistory[processName] = DateTime.Now;
                        }
                    }
                    catch
                    {
                        // РРіРЅРѕСЂРёСЂСѓРµРј РѕС€РёР±РєРё РґРѕСЃС‚СѓРїР° Рє РїСЂРѕС†РµСЃСЃР°Рј
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° СЃРєР°РЅРёСЂРѕРІР°РЅРёСЏ РїСЂРѕС†РµСЃСЃРѕРІ: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РћР±СЂР°Р±РѕС‚РєР° РѕР±РЅР°СЂСѓР¶РµРЅРЅРѕР№ СѓРіСЂРѕР·С‹
        /// </summary>
        private static void HandleThreatDetection(ThreatSignature threat, System.Diagnostics.Process process)
        {
            try
            {
                string threatInfo = string.Format("{0} (PID: {1})", threat.Name, process.Id);
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] рџљЁ РЈР“Р РћР—Рђ РћР‘РќРђР РЈР–Р•РќРђ: {0}", threatInfo));

                // Р РµРіРёСЃС‚СЂРёСЂСѓРµРј СѓРіСЂРѕР·Сѓ РІ SecurityManager
                SecurityManager.RegisterThreat("process_threat", threatInfo);

                // РџСЂРёРјРµРЅСЏРµРј РєРѕРЅС‚СЂРјРµСЂС‹ РІ Р·Р°РІРёСЃРёРјРѕСЃС‚Рё РѕС‚ СѓСЂРѕРІРЅСЏ СѓРіСЂРѕР·С‹
                switch (threat.Level)
                {
                    case ThreatLevel.Critical:
                        Debug.LogError(string.Format("[DeftHack Threat Detection] вљ пёЏ РљР РРўРР§Р•РЎРљРђРЇ РЈР“Р РћР—Рђ: {0}", threat.Name));
                        // РђРєС‚РёРІРёСЂСѓРµРј РјР°РєСЃРёРјР°Р»СЊРЅСѓСЋ Р·Р°С‰РёС‚Сѓ
                        SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Paranoid);
                        break;

                    case ThreatLevel.High:
                        Debug.LogWarning(string.Format("[DeftHack Threat Detection] вљ пёЏ Р’Р«РЎРћРљРђРЇ РЈР“Р РћР—Рђ: {0}", threat.Name));
                        // РџРѕРІС‹С€Р°РµРј СѓСЂРѕРІРµРЅСЊ Р·Р°С‰РёС‚С‹
                        if (SecurityManager.CurrentMode == SecurityManager.SecurityMode.Basic)
                        {
                            SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Advanced);
                        }
                        break;

                    case ThreatLevel.Medium:
                        Debug.Log(string.Format("[DeftHack Threat Detection] вљ пёЏ РЎСЂРµРґРЅСЏСЏ СѓРіСЂРѕР·Р°: {0}", threat.Name));
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РѕР±СЂР°Р±РѕС‚РєРё СѓРіСЂРѕР·С‹: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РџСЂРѕРІРµСЂРєР° РѕС‚Р»Р°РґС‡РёРєРѕРІ
        /// </summary>
        private static void CheckForDebuggers()
        {
            try
            {
                // 1. IsDebuggerPresent
                if (IsDebuggerPresent())
                {
                    SecurityManager.RegisterThreat("debugger", "IsDebuggerPresent() РІРµСЂРЅСѓР» true");
                    return;
                }

                // 2. CheckRemoteDebuggerPresent
                bool isRemoteDebugger = false;
                if (CheckRemoteDebuggerPresent(GetCurrentProcess(), ref isRemoteDebugger) && isRemoteDebugger)
                {
                    SecurityManager.RegisterThreat("debugger", "РћР±РЅР°СЂСѓР¶РµРЅ СѓРґР°Р»РµРЅРЅС‹Р№ РѕС‚Р»Р°РґС‡РёРє");
                    return;
                }

                // 3. NtQueryInformationProcess
                PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                int returnLength;
                int status = NtQueryInformationProcess(GetCurrentProcess(), 7, ref pbi, Marshal.SizeOf(pbi), out returnLength);
                
                if (status == 0 && pbi.Reserved1 != IntPtr.Zero)
                {
                    SecurityManager.RegisterThreat("debugger", "NtQueryInformationProcess РѕР±РЅР°СЂСѓР¶РёР» РѕС‚Р»Р°РґС‡РёРє");
                    return;
                }

                // 4. РџСЂРѕРІРµСЂРєР° BeingDebugged С„Р»Р°РіР° РІ PEB
                CheckPEBDebuggingFlags();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РїСЂРѕРІРµСЂРєРё РѕС‚Р»Р°РґС‡РёРєРѕРІ: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РџСЂРѕРІРµСЂРєР° С„Р»Р°РіРѕРІ РѕС‚Р»Р°РґРєРё РІ PEB
        /// </summary>
        private static void CheckPEBDebuggingFlags()
        {
            try
            {
                // РџРѕР»СѓС‡Р°РµРј Р°РґСЂРµСЃ PEB С‡РµСЂРµР· NtQueryInformationProcess
                PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                int returnLength;
                int status = NtQueryInformationProcess(GetCurrentProcess(), 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);

                if (status == 0 && pbi.PebBaseAddress != IntPtr.Zero)
                {
                    // Р§РёС‚Р°РµРј BeingDebugged С„Р»Р°Рі (offset 0x02 РІ PEB)
                    byte beingDebugged = Marshal.ReadByte(pbi.PebBaseAddress, 0x02);
                    if (beingDebugged != 0)
                    {
                        SecurityManager.RegisterThreat("debugger", "PEB BeingDebugged С„Р»Р°Рі СѓСЃС‚Р°РЅРѕРІР»РµРЅ");
                    }

                    // Р§РёС‚Р°РµРј NtGlobalFlag (offset 0x68 РІ PEB РґР»СЏ x64)
                    uint ntGlobalFlag = (uint)Marshal.ReadInt32(pbi.PebBaseAddress, 0x68);
                    if ((ntGlobalFlag & 0x70) != 0) // FLG_HEAP_ENABLE_TAIL_CHECK | FLG_HEAP_ENABLE_FREE_CHECK | FLG_HEAP_VALIDATE_PARAMETERS
                    {
                        SecurityManager.RegisterThreat("debugger", "PEB NtGlobalFlag СѓРєР°Р·С‹РІР°РµС‚ РЅР° РѕС‚Р»Р°РґРєСѓ");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РїСЂРѕРІРµСЂРєРё PEB: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РџСЂРѕРІРµСЂРєР° РІРёСЂС‚СѓР°Р»РёР·Р°С†РёРё
        /// </summary>
        private static void CheckForVirtualization()
        {
            try
            {
                // РџСЂРѕРІРµСЂСЏРµРј С‡РµСЂРµР· WMI
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString()?.ToLower() ?? "";
                        string model = obj["Model"]?.ToString()?.ToLower() ?? "";

                        if (manufacturer.Contains("vmware") || model.Contains("vmware") ||
                            manufacturer.Contains("virtualbox") || model.Contains("virtualbox") ||
                            manufacturer.Contains("microsoft corporation") && model.Contains("virtual"))
                        {
                            SecurityManager.RegisterThreat("virtualization", string.Format("РћР±РЅР°СЂСѓР¶РµРЅР° РІРёСЂС‚СѓР°Р»РёР·Р°С†РёСЏ: {0} {1}", manufacturer, model));
                            return;
                        }
                    }
                }

                // РџСЂРѕРІРµСЂСЏРµРј РїСЂРѕС†РµСЃСЃС‹ РІРёСЂС‚СѓР°Р»РёР·Р°С†РёРё
                string[] vmProcesses = { "vmtoolsd", "vboxservice", "vboxtray" };
                foreach (string vmProcess in vmProcesses)
                {
                    System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(vmProcess);
                    if (processes.Length > 0)
                    {
                        SecurityManager.RegisterThreat("virtualization", string.Format("РћР±РЅР°СЂСѓР¶РµРЅ РїСЂРѕС†РµСЃСЃ РІРёСЂС‚СѓР°Р»РёР·Р°С†РёРё: {0}", vmProcess));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РїСЂРѕРІРµСЂРєРё РІРёСЂС‚СѓР°Р»РёР·Р°С†РёРё: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РџСЂРѕРІРµСЂРєР° Р°РЅРѕРјР°Р»РёР№ РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚Рё
        /// </summary>
        private static void CheckPerformanceAnomalies()
        {
            try
            {
                long start, end, frequency;
                QueryPerformanceFrequency(out frequency);
                QueryPerformanceCounter(out start);

                // РўР° Р¶Рµ РѕРїРµСЂР°С†РёСЏ С‡С‚Рѕ Рё РІ baseline
                for (int i = 0; i < 1000; i++)
                {
                    Math.Sqrt(i);
                }

                QueryPerformanceCounter(out end);
                long currentPerformance = ((end - start) * 1000000) / frequency;

                // Р•СЃР»Рё РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚СЊ Р·РЅР°С‡РёС‚РµР»СЊРЅРѕ РѕС‚Р»РёС‡Р°РµС‚СЃСЏ, РІРѕР·РјРѕР¶РЅР° РІРёСЂС‚СѓР°Р»РёР·Р°С†РёСЏ РёР»Рё РѕС‚Р»Р°РґРєР°
                if (Math.Abs(currentPerformance - _baselinePerformance) > _baselinePerformance * 0.5) // 50% РѕС‚РєР»РѕРЅРµРЅРёРµ
                {
                    SecurityManager.RegisterThreat("timing_attack", 
                        string.Format("РђРЅРѕРјР°Р»РёСЏ РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚Рё: {0} РјРєСЃ (Р±Р°Р·РѕРІР°СЏ: {1} РјРєСЃ)", currentPerformance, _baselinePerformance));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РїСЂРѕРІРµСЂРєРё РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚Рё: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РџСЂРѕРІРµСЂРєР° С†РµР»РѕСЃС‚РЅРѕСЃС‚Рё РїР°РјСЏС‚Рё
        /// </summary>
        private static void CheckMemoryIntegrity()
        {
            try
            {
                // РџСЂРѕРІРµСЂСЏРµРј, РЅРµ РјРѕРґРёС„РёС†РёСЂРѕРІР°РЅС‹ Р»Рё РєСЂРёС‚РёС‡РµСЃРєРёРµ РѕР±Р»Р°СЃС‚Рё РїР°РјСЏС‚Рё
                // Р’ СЂРµР°Р»СЊРЅРѕР№ СЂРµР°Р»РёР·Р°С†РёРё Р·РґРµСЃСЊ Р±С‹Р»Р° Р±С‹ РїСЂРѕРІРµСЂРєР° С…РµС€РµР№ РєСЂРёС‚РёС‡РµСЃРєРёС… С„СѓРЅРєС†РёР№
                
                // РџСЂРѕСЃС‚Р°СЏ РїСЂРѕРІРµСЂРєР° - СЂР°Р·РјРµСЂ РєСѓС‡Рё
                long memoryBefore = GC.GetTotalMemory(false);
                GC.Collect();
                long memoryAfter = GC.GetTotalMemory(true);
                
                // Р•СЃР»Рё РїР°РјСЏС‚СЊ РЅРµ РѕСЃРІРѕР±РѕРґРёР»Р°СЃСЊ РєР°Рє РѕР¶РёРґР°Р»РѕСЃСЊ, РІРѕР·РјРѕР¶РЅР° РёРЅР¶РµРєС†РёСЏ
                if (memoryBefore - memoryAfter < memoryBefore * 0.1) // РњРµРЅРµРµ 10% РѕСЃРІРѕР±РѕР¶РґРµРЅРѕ
                {
                    SecurityManager.RegisterThreat("memory_anomaly", 
                        string.Format("РђРЅРѕРјР°Р»РёСЏ РїР°РјСЏС‚Рё: РґРѕ GC {0}, РїРѕСЃР»Рµ GC {1}", memoryBefore, memoryAfter));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РїСЂРѕРІРµСЂРєРё РїР°РјСЏС‚Рё: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РџСЂРѕРІРµСЂРєР° СЃРµС‚РµРІРѕР№ Р°РєС‚РёРІРЅРѕСЃС‚Рё
        /// </summary>
        private static void CheckNetworkActivity()
        {
            try
            {
                // РџСЂРѕРІРµСЂСЏРµРј РїРѕРґРѕР·СЂРёС‚РµР»СЊРЅС‹Рµ СЃРµС‚РµРІС‹Рµ СЃРѕРµРґРёРЅРµРЅРёСЏ
                // Р’ СЂРµР°Р»СЊРЅРѕР№ СЂРµР°Р»РёР·Р°С†РёРё Р·РґРµСЃСЊ Р°РЅР°Р»РёР·РёСЂРѕРІР°Р»РёСЃСЊ Р±С‹ Р°РєС‚РёРІРЅС‹Рµ СЃРѕРµРґРёРЅРµРЅРёСЏ
                
                // РџСЂРѕСЃС‚Р°СЏ РїСЂРѕРІРµСЂРєР° - РєРѕР»РёС‡РµСЃС‚РІРѕ СЃРµС‚РµРІС‹С… РїСЂРѕС†РµСЃСЃРѕРІ
                int networkProcessCount = 0;
                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
                
                foreach (System.Diagnostics.Process process in processes)
                {
                    try
                    {
                        if (process.ProcessName.ToLower().Contains("net") || 
                            process.ProcessName.ToLower().Contains("tcp") ||
                            process.ProcessName.ToLower().Contains("http"))
                        {
                            networkProcessCount++;
                        }
                    }
                    catch
                    {
                        // РРіРЅРѕСЂРёСЂСѓРµРј РѕС€РёР±РєРё РґРѕСЃС‚СѓРїР°
                    }
                }

                // Р•СЃР»Рё СЃР»РёС€РєРѕРј РјРЅРѕРіРѕ СЃРµС‚РµРІС‹С… РїСЂРѕС†РµСЃСЃРѕРІ, РІРѕР·РјРѕР¶РЅР° РїРѕРґРѕР·СЂРёС‚РµР»СЊРЅР°СЏ Р°РєС‚РёРІРЅРѕСЃС‚СЊ
                if (networkProcessCount > 20)
                {
                    SecurityManager.RegisterThreat("network_anomaly", 
                        string.Format("РџРѕРґРѕР·СЂРёС‚РµР»СЊРЅР°СЏ СЃРµС‚РµРІР°СЏ Р°РєС‚РёРІРЅРѕСЃС‚СЊ: {0} СЃРµС‚РµРІС‹С… РїСЂРѕС†РµСЃСЃРѕРІ", networkProcessCount));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РїСЂРѕРІРµСЂРєРё СЃРµС‚Рё: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РћСЃС‚Р°РЅРѕРІРєР° СЃРёСЃС‚РµРјС‹ РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ СѓРіСЂРѕР·
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                UnityEngine.Debug.Log("[DeftHack Threat Detection] РћСЃС‚Р°РЅРѕРІРєР° СЃРёСЃС‚РµРјС‹ РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ СѓРіСЂРѕР·...");

                _isInitialized = false;
                _detectionThread?.Abort();

                _knownThreats.Clear();
                _processHistory.Clear();

                UnityEngine.Debug.Log("[DeftHack Threat Detection] РЎРёСЃС‚РµРјР° РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ СѓРіСЂРѕР· РѕСЃС‚Р°РЅРѕРІР»РµРЅР°");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("[DeftHack Threat Detection] РћС€РёР±РєР° РѕСЃС‚Р°РЅРѕРІРєРё: {0}", ex.Message));
            }
        }

        /// <summary>
        /// РђРєС‚РёРІРЅР° Р»Рё СЃРёСЃС‚РµРјР° РѕР±РЅР°СЂСѓР¶РµРЅРёСЏ СѓРіСЂРѕР·
        /// </summary>
        public static bool IsActive { get { return _isInitialized; } }

        /// <summary>
        /// РљРѕР»РёС‡РµСЃС‚РІРѕ РёР·РІРµСЃС‚РЅС‹С… СѓРіСЂРѕР·
        /// </summary>
        public static int KnownThreatsCount { get { return _knownThreats.Count; } }

        /// <summary>
        /// РљРѕР»РёС‡РµСЃС‚РІРѕ РѕС‚СЃР»РµР¶РёРІР°РµРјС‹С… РїСЂРѕС†РµСЃСЃРѕРІ
        /// </summary>
        public static int TrackedProcessesCount { get { return _processHistory.Count; } }
    }
}


using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace DeftHack.Components.UI
{
    /// <summary>
    /// Система верификации через разделяемую память (Shared Memory)
    /// Создает область памяти доступную внешним процессам для проверки инжекции
    /// </summary>
    public class SharedMemoryVerifier : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct InjectionData
        {
            public bool isInjected;
            public int processId;
            public long moduleBase;
            public long timestamp;
            public float uptime;
            public int playerCount;
            public bool espEnabled;
            public bool aimbotEnabled;
            public bool flightEnabled;
            public bool panicMode;
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string moduleName;
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string processName;
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string mapName;
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string lastError;
        }
        
        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;
        private Thread updateThread;
        private bool isRunning = false;
        private const string MEMORY_NAME = "Local\\UnturnedDeftHackInjectionData";
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            InitializeSharedMemory();
            
            Debug.Log("[SharedMemoryVerifier] Shared memory initialized");
        }
        
        void InitializeSharedMemory()
        {
            try
            {
                int dataSize = Marshal.SizeOf<InjectionData>();
                
                // Создаем область разделяемой памяти
                mmf = MemoryMappedFile.CreateNew(MEMORY_NAME, dataSize, MemoryMappedFileAccess.ReadWrite);
                accessor = mmf.CreateViewAccessor(0, dataSize);
                
                // Инициализируем данные
                var initialData = new InjectionData
                {
                    isInjected = true,
                    processId = System.Diagnostics.Process.GetCurrentProcess().Id,
                    moduleBase = System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress.ToInt64(),
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    uptime = 0f,
                    playerCount = 0,
                    espEnabled = false,
                    aimbotEnabled = false,
                    flightEnabled = false,
                    panicMode = false,
                    moduleName = "UnityEngine.FileSystemModule.dll",
                    processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                    mapName = "Unknown",
                    lastError = "None"
                };
                
                WriteDataToMemory(initialData);
                
                // Запускаем поток обновления
                isRunning = true;
                updateThread = new Thread(UpdateMemoryWorker);
                updateThread.IsBackground = true;
                updateThread.Start();
                
                Debug.Log($"[SharedMemoryVerifier] Shared memory created: {MEMORY_NAME}, size: {dataSize} bytes");
                
                // Создаем файл-маркер для внешних процессов
                CreateMemoryMarkerFile();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SharedMemoryVerifier] Failed to initialize shared memory: {ex.Message}");
                
                // Записываем ошибку в файл
                WriteErrorToFile($"SharedMemory initialization failed: {ex}");
            }
        }
        
        void UpdateMemoryWorker()
        {
            while (isRunning)
            {
                try
                {
                    var data = new InjectionData
                    {
                        isInjected = true,
                        processId = System.Diagnostics.Process.GetCurrentProcess().Id,
                        moduleBase = System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress.ToInt64(),
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        uptime = Time.realtimeSinceStartup,
                        playerCount = GetPlayerCount(),
                        espEnabled = GetESPStatus(),
                        aimbotEnabled = GetAimbotStatus(),
                        flightEnabled = GetFlightStatus(),
                        panicMode = GetPanicModeStatus(),
                        moduleName = "UnityEngine.FileSystemModule.dll",
                        processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                        mapName = GetCurrentMapName(),
                        lastError = "None"
                    };
                    
                    WriteDataToMemory(data);
                    
                    // Обновляем каждые 2 секунды
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SharedMemoryVerifier] Update error: {ex.Message}");
                    
                    // Записываем ошибку в память
                    try
                    {
                        var errorData = ReadDataFromMemory();
                        errorData.lastError = ex.Message;
                        WriteDataToMemory(errorData);
                    }
                    catch { }
                    
                    Thread.Sleep(5000);
                }
            }
        }
        
        void WriteDataToMemory(InjectionData data)
        {
            try
            {
                if (accessor != null)
                {
                    byte[] dataBytes = StructToBytes(data);
                    accessor.WriteArray(0, dataBytes, 0, dataBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SharedMemoryVerifier] Write error: {ex.Message}");
            }
        }
        
        InjectionData ReadDataFromMemory()
        {
            try
            {
                if (accessor != null)
                {
                    int dataSize = Marshal.SizeOf<InjectionData>();
                    byte[] dataBytes = new byte[dataSize];
                    accessor.ReadArray(0, dataBytes, 0, dataSize);
                    return BytesToStruct<InjectionData>(dataBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SharedMemoryVerifier] Read error: {ex.Message}");
            }
            
            return new InjectionData();
        }
        
        byte[] StructToBytes<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            
            try
            {
                Marshal.StructureToPtr(structure, ptr, true);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            
            return bytes;
        }
        
        T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            
            try
            {
                Marshal.Copy(bytes, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        
        int GetPlayerCount()
        {
            try
            {
                return SDG.Unturned.Provider.clients?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }
        
        bool GetESPStatus()
        {
            try
            {
                return ESPOptions.Enabled;
            }
            catch
            {
                return false;
            }
        }
        
        bool GetAimbotStatus()
        {
            try
            {
                return AimbotOptions.Enabled;
            }
            catch
            {
                return false;
            }
        }
        
        bool GetFlightStatus()
        {
            try
            {
                return MiscOptions.PlayerFlight;
            }
            catch
            {
                return false;
            }
        }
        
        bool GetPanicModeStatus()
        {
            try
            {
                return MiscOptions.PanicMode;
            }
            catch
            {
                return false;
            }
        }
        
        string GetCurrentMapName()
        {
            try
            {
                return SDG.Unturned.Level.info?.name ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
        
        void CreateMemoryMarkerFile()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string markerFile = Path.Combine(tempPath, "defthack_shared_memory_info.txt");
                
                string markerContent = $@"DEFTHACK SHARED MEMORY INFORMATION
Memory Name: {MEMORY_NAME}
Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}
Process Name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}
Data Size: {Marshal.SizeOf<InjectionData>()} bytes
Creation Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Status: ACTIVE

=== USAGE INSTRUCTIONS ===
External processes can access this shared memory to verify injection status.
Use MemoryMappedFile.OpenExisting(""{MEMORY_NAME}"") to access the data.

=== DATA STRUCTURE ===
- isInjected: bool - Injection status
- processId: int - Target process ID
- moduleBase: long - Module base address
- timestamp: long - Unix timestamp
- uptime: float - Application uptime
- playerCount: int - Current player count
- espEnabled: bool - ESP status
- aimbotEnabled: bool - Aimbot status
- flightEnabled: bool - Flight status
- panicMode: bool - Panic mode status
- moduleName: string[256] - Module name
- processName: string[256] - Process name
- mapName: string[256] - Current map name
- lastError: string[512] - Last error message";

                File.WriteAllText(markerFile, markerContent);
                
                Debug.Log($"[SharedMemoryVerifier] Marker file created: {markerFile}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SharedMemoryVerifier] Failed to create marker file: {ex.Message}");
            }
        }
        
        void WriteErrorToFile(string error)
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string errorFile = Path.Combine(tempPath, "defthack_shared_memory_errors.log");
                string errorEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {error}\n";
                File.AppendAllText(errorFile, errorEntry);
            }
            catch { }
        }
        
        void OnDestroy()
        {
            try
            {
                isRunning = false;
                
                if (updateThread != null && updateThread.IsAlive)
                {
                    updateThread.Join(1000);
                }
                
                if (accessor != null)
                {
                    accessor.Dispose();
                }
                
                if (mmf != null)
                {
                    mmf.Dispose();
                }
                
                Debug.Log("[SharedMemoryVerifier] Shared memory disposed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SharedMemoryVerifier] Dispose error: {ex.Message}");
            }
        }
    }
}
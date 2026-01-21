using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using UnityEngine;

namespace DeftHack.Components.UI
{
    /// <summary>
    /// Система верификации инжекции через Named Pipes
    /// Создает двустороннюю связь между DLL и внешними процессами
    /// </summary>
    public class NamedPipeVerifier : MonoBehaviour
    {
        private NamedPipeServerStream pipeServer;
        private Thread pipeThread;
        private bool isRunning = false;
        private string pipeName = "UnturnedDeftHackPipe";
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            StartPipeServer();
            
            // Отправляем сигнал о успешной инжекции
            SendInjectionSignal();
            
            Debug.Log("[NamedPipeVerifier] Pipe server started");
        }
        
        void StartPipeServer()
        {
            try
            {
                isRunning = true;
                pipeThread = new Thread(PipeServerWorker);
                pipeThread.IsBackground = true;
                pipeThread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NamedPipeVerifier] Failed to start pipe server: {ex.Message}");
            }
        }
        
        void PipeServerWorker()
        {
            while (isRunning)
            {
                try
                {
                    using (pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1))
                    {
                        Debug.Log("[NamedPipeVerifier] Waiting for client connection...");
                        
                        // Ждем подключения клиента с таймаутом
                        var connectTask = pipeServer.WaitForConnectionAsync();
                        if (connectTask.Wait(5000)) // 5 секунд таймаут
                        {
                            Debug.Log("[NamedPipeVerifier] Client connected!");
                            
                            // Читаем запрос от клиента
                            byte[] buffer = new byte[1024];
                            int bytesRead = pipeServer.Read(buffer, 0, buffer.Length);
                            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            
                            Debug.Log($"[NamedPipeVerifier] Received request: {request}");
                            
                            // Формируем ответ
                            string response = ProcessRequest(request);
                            
                            // Отправляем ответ
                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            pipeServer.Write(responseBytes, 0, responseBytes.Length);
                            pipeServer.Flush();
                            
                            Debug.Log($"[NamedPipeVerifier] Sent response: {response.Substring(0, Math.Min(100, response.Length))}...");
                        }
                        else
                        {
                            Debug.Log("[NamedPipeVerifier] Connection timeout");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NamedPipeVerifier] Pipe server error: {ex.Message}");
                }
                
                // Небольшая пауза перед следующим циклом
                Thread.Sleep(1000);
            }
        }
        
        string ProcessRequest(string request)
        {
            try
            {
                switch (request.ToUpper())
                {
                    case "STATUS":
                        return GetStatusInfo();
                        
                    case "INJECTION_CHECK":
                        return GetInjectionInfo();
                        
                    case "PLAYER_INFO":
                        return GetPlayerInfo();
                        
                    case "CHEAT_STATUS":
                        return GetCheatStatus();
                        
                    case "PING":
                        return "PONG";
                        
                    case "DETAILED_STATUS":
                        return GetDetailedStatus();
                        
                    default:
                        return $"UNKNOWN_REQUEST:{request}";
                }
            }
            catch (Exception ex)
            {
                return $"ERROR:{ex.Message}";
            }
        }
        
        string GetStatusInfo()
        {
            return $@"STATUS:OK
PROCESS_ID:{System.Diagnostics.Process.GetCurrentProcess().Id}
PROCESS_NAME:{System.Diagnostics.Process.GetCurrentProcess().ProcessName}
UPTIME:{Time.realtimeSinceStartup:F1}
UNITY_VERSION:{Application.unityVersion}
TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        
        string GetInjectionInfo()
        {
            return $@"INJECTION:SUCCESS
DLL_LOADED:TRUE
UNITY_INTEGRATION:SUCCESS
DEFTHACK_ACTIVE:TRUE
COMPONENTS_LOADED:{gameObject.GetComponents<MonoBehaviour>().Length}
TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        
        string GetPlayerInfo()
        {
            try
            {
                var player = SDG.Unturned.Player.instance;
                if (player != null)
                {
                    return $@"PLAYER:AVAILABLE
POSITION:{player.transform.position}
HEALTH:{player.life.health}
FOOD:{player.life.food}
WATER:{player.life.water}
STAMINA:{player.life.stamina}
TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                }
                else
                {
                    return $@"PLAYER:NOT_AVAILABLE
REASON:Player instance is null
TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                }
            }
            catch (Exception ex)
            {
                return $@"PLAYER:ERROR
ERROR:{ex.Message}
TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
        }
        
        string GetCheatStatus()
        {
            try
            {
                return $@"CHEATS:ACTIVE
ESP:{(ESPOptions.Enabled ? "ON" : "OFF")}
AIMBOT:{(AimbotOptions.Enabled ? "ON" : "OFF")}
FLIGHT:{(MiscOptions.PlayerFlight ? "ON" : "OFF")}
VEHICLE_FLY:{(MiscOptions.VehicleFly ? "ON" : "OFF")}
NIGHT_VISION:{(MiscOptions.NightVision ? "ON" : "OFF")}
HANG_MODE:{(MiscOptions.hang ? "ON" : "OFF")}
FREECAM:{(MiscOptions.Freecam ? "ON" : "OFF")}
PANIC_MODE:{(MiscOptions.PanicMode ? "ACTIVE" : "OFF")}
TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
            catch (Exception ex)
            {
                return $@"CHEATS:ERROR
ERROR:{ex.Message}
TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
        }
        
        string GetDetailedStatus()
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var player = SDG.Unturned.Player.instance;
                
                return $@"DETAILED_STATUS:SUCCESS
=== PROCESS INFO ===
PID:{process.Id}
NAME:{process.ProcessName}
MEMORY_USAGE:{process.WorkingSet64 / 1024 / 1024}MB
THREADS:{process.Threads.Count}
MODULES:{process.Modules.Count}

=== UNITY INFO ===
VERSION:{Application.unityVersion}
PLATFORM:{Application.platform}
IS_PLAYING:{Application.isPlaying}
TARGET_FRAMERATE:{Application.targetFrameRate}
UPTIME:{Time.realtimeSinceStartup:F1}s

=== GAME INFO ===
PROVIDER_CLIENTS:{SDG.Unturned.Provider.clients?.Count ?? 0}
LEVEL_NAME:{SDG.Unturned.Level.info?.name ?? "Unknown"}
GAME_VERSION:{SDG.Unturned.Provider.APP_VERSION}

=== PLAYER INFO ===
PLAYER_AVAILABLE:{(player != null ? "YES" : "NO")}
{(player != null ? $@"POSITION:{player.transform.position}
HEALTH:{player.life.health}/100
FOOD:{player.life.food}/100
WATER:{player.life.water}/100" : "PLAYER_DATA:NOT_AVAILABLE")}

=== CHEAT STATUS ===
ESP:{(ESPOptions.Enabled ? "ENABLED" : "DISABLED")}
AIMBOT:{(AimbotOptions.Enabled ? "ENABLED" : "DISABLED")}
FLIGHT:{(MiscOptions.PlayerFlight ? "ENABLED" : "DISABLED")}
VEHICLE_FLY:{(MiscOptions.VehicleFly ? "ENABLED" : "DISABLED")}
NIGHT_VISION:{(MiscOptions.NightVision ? "ENABLED" : "DISABLED")}
HANG_MODE:{(MiscOptions.hang ? "ENABLED" : "DISABLED")}
FREECAM:{(MiscOptions.Freecam ? "ENABLED" : "DISABLED")}
PANIC_MODE:{(MiscOptions.PanicMode ? "ACTIVE" : "INACTIVE")}

=== COMPONENTS ===
TOTAL_COMPONENTS:{gameObject.GetComponents<MonoBehaviour>().Length}
DEFTHACK_COMPONENTS:{CountDeftHackComponents()}

TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
            catch (Exception ex)
            {
                return $@"DETAILED_STATUS:ERROR
ERROR:{ex.Message}
TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
        }
        
        int CountDeftHackComponents()
        {
            int count = 0;
            var components = gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                if (component.GetType().Namespace != null && 
                    component.GetType().Namespace.Contains("DeftHack"))
                {
                    count++;
                }
            }
            return count;
        }
        
        void SendInjectionSignal()
        {
            try
            {
                // Создаем клиентский pipe для отправки сигнала внешним верификаторам
                using (var client = new NamedPipeClientStream(".", "UnturnedInjectionVerifier", PipeDirection.Out))
                {
                    client.Connect(1000); // 1 секунда таймаут
                    
                    string signal = $@"INJECTION_SUCCESS
PID:{System.Diagnostics.Process.GetCurrentProcess().Id}
TIME:{DateTime.Now:yyyy-MM-dd HH:mm:ss}
STATUS:DLL_ACTIVE";
                    
                    byte[] signalBytes = Encoding.UTF8.GetBytes(signal);
                    client.Write(signalBytes, 0, signalBytes.Length);
                    client.Flush();
                    
                    Debug.Log("[NamedPipeVerifier] Injection signal sent to external verifier");
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[NamedPipeVerifier] Could not send injection signal: {ex.Message}");
                // Это нормально, если внешний верификатор не запущен
            }
        }
        
        void OnDestroy()
        {
            try
            {
                isRunning = false;
                
                if (pipeServer != null && pipeServer.IsConnected)
                {
                    pipeServer.Disconnect();
                }
                
                if (pipeThread != null && pipeThread.IsAlive)
                {
                    pipeThread.Join(1000); // Ждем 1 секунду
                }
                
                Debug.Log("[NamedPipeVerifier] Pipe server stopped");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NamedPipeVerifier] Error stopping pipe server: {ex.Message}");
            }
        }
    }
}
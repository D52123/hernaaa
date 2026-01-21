using System;
using System.Threading;
using UnityEngine;

namespace DeftHack.Security
{
    /// <summary>
    /// Центральный менеджер системы безопасности DeftHack
    /// Координирует все методы обхода BattlEye
    /// </summary>
    public static class SecurityManager
    {
        public enum SecurityMode
        {
            Disabled,       // Отключено
            Basic,          // Базовая защита
            Advanced,       // Продвинутая защита
            Paranoid,       // Параноидальный режим
            Adaptive        // Адаптивный режим
        }

        public enum ThreatLevel
        {
            None,           // Угроз нет
            Low,            // Низкий уровень
            Medium,         // Средний уровень
            High,           // Высокий уровень
            Critical        // Критический уровень
        }

        private static bool _isInitialized = false;
        private static SecurityMode _currentMode = SecurityMode.Advanced;
        private static ThreatLevel _currentThreatLevel = ThreatLevel.None;
        private static Thread _monitoringThread;
        private static readonly object _lockObject = new object();

        // Статистика
        private static int _threatsDetected = 0;
        private static int _screenshotsBlocked = 0;
        private static int _debuggersDetected = 0;
        private static DateTime _lastThreatTime = DateTime.MinValue;

        /// <summary>
        /// Инициализация системы безопасности
        /// </summary>
        public static void Initialize(SecurityMode mode = SecurityMode.Advanced)
        {
            if (_isInitialized) return;

            lock (_lockObject)
            {
                if (_isInitialized) return;

                try
                {
                    Debug.Log("[DeftHack Security Manager] Инициализация системы безопасности...");
                    
                    _currentMode = mode;
                    
                    // Инициализируем компоненты в зависимости от режима
                    InitializeSecurityComponents();
                    
                    // Запускаем мониторинг
                    StartThreatMonitoring();
                    
                    _isInitialized = true;
                    
                    Debug.Log(string.Format("[DeftHack Security Manager] Система безопасности активирована в режиме: {0}", mode));
                    LogSecurityStatus();
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("[DeftHack Security Manager] Критическая ошибка инициализации: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Инициализация компонентов безопасности
        /// </summary>
        private static void InitializeSecurityComponents()
        {
            try
            {
                switch (_currentMode)
                {
                    case SecurityMode.Disabled:
                        Debug.Log("[DeftHack Security Manager] Режим безопасности отключен");
                        break;

                    case SecurityMode.Basic:
                        Debug.Log("[DeftHack Security Manager] Инициализация базовой защиты...");
                        ModernBypass.Initialize();
                        AdvancedScreenshotBypass.Initialize();
                        AdvancedThreatDetection.Initialize();
                        break;

                    case SecurityMode.Advanced:
                        Debug.Log("[DeftHack Security Manager] Инициализация продвинутой защиты...");
                        ModernBypass.Initialize();
                        HyperVBypass.Initialize();
                        AdvancedScreenshotBypass.Initialize();
                        AdvancedThreatDetection.Initialize();
                        
                        // Пытаемся инициализировать kernel-mode обход
                        KernelBypass.Initialize();
                        if (!KernelBypass.IsActive)
                        {
                            Debug.LogWarning("[DeftHack Security Manager] Kernel bypass недоступен, используем user-mode");
                        }
                        break;

                    case SecurityMode.Paranoid:
                        Debug.Log("[DeftHack Security Manager] Инициализация параноидального режима...");
                        ModernBypass.Initialize();
                        HyperVBypass.Initialize();
                        AdvancedScreenshotBypass.Initialize();
                        AdvancedThreatDetection.Initialize();
                        KernelBypass.Initialize();
                        
                        // Стелс-гипервизор - максимальная скрытность
                        StealthHypervisor.Initialize(5); // Максимальный уровень скрытности
                        if (!StealthHypervisor.IsActive)
                        {
                            Debug.LogWarning("[DeftHack Security Manager] Стелс-гипервизор недоступен, используем обычный");
                            HypervisorBypass.Initialize();
                        }
                        
                        // Внешний анализ как дополнительная защита
                        ExternalAnalysis.Initialize();
                        EnableParanoidMode();
                        break;

                    case SecurityMode.Adaptive:
                        Debug.Log("[DeftHack Security Manager] Инициализация адаптивного режима...");
                        ModernBypass.Initialize();
                        AdvancedScreenshotBypass.Initialize();
                        AdvancedThreatDetection.Initialize();
                        
                        // В адаптивном режиме начинаем с внешнего анализа (самый безопасный)
                        ExternalAnalysis.Initialize();
                        
                        // Остальные системы включаем по необходимости
                        break;
                }
                
                // Логируем статус всех систем
                LogSecurityComponentStatus();
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Security Manager] Ошибка инициализации компонентов: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Включение параноидального режима
        /// </summary>
        private static void EnableParanoidMode()
        {
            try
            {
                Debug.Log("[DeftHack Security Manager] Активация параноидальных мер...");
                
                // Дополнительные проверки каждые 2 секунды вместо 5
                // Более агрессивная очистка памяти
                // Дополнительные методы обнаружения
                
                Debug.Log("[DeftHack Security Manager] Параноидальный режим активирован");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Security Manager] Ошибка параноидального режима: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Запуск мониторинга угроз
        /// </summary>
        private static void StartThreatMonitoring()
        {
            try
            {
                _monitoringThread = new Thread(ThreatMonitoringLoop)
                {
                    IsBackground = true,
                    Name = "DeftHack Security Monitor"
                };
                _monitoringThread.Start();
                
                Debug.Log("[DeftHack Security Manager] Мониторинг угроз запущен");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Security Manager] Ошибка запуска мониторинга: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Основной цикл мониторинга угроз
        /// </summary>
        private static void ThreatMonitoringLoop()
        {
            while (_isInitialized)
            {
                try
                {
                    // Интервал проверки зависит от режима
                    int checkInterval = GetCheckInterval();
                    Thread.Sleep(checkInterval);

                    // Оценка текущего уровня угрозы
                    ThreatLevel newThreatLevel = AssessThreatLevel();
                    
                    if (newThreatLevel != _currentThreatLevel)
                    {
                        HandleThreatLevelChange(newThreatLevel);
                    }

                    // Адаптивная настройка в зависимости от угроз
                    if (_currentMode == SecurityMode.Adaptive)
                    {
                        AdaptSecurityMeasures();
                    }

                    // Периодическая очистка
                    PerformMaintenanceTasks();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(string.Format("[DeftHack Security Manager] Ошибка в мониторинге: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Получение интервала проверки
        /// </summary>
        private static int GetCheckInterval()
        {
            switch (_currentMode)
            {
                case SecurityMode.Basic: return 10000;      // 10 секунд
                case SecurityMode.Advanced: return 5000;    // 5 секунд
                case SecurityMode.Paranoid: return 2000;    // 2 секунды
                case SecurityMode.Adaptive: return 3000;    // 3 секунды
                default: return 5000;
            }
        }

        /// <summary>
        /// Оценка уровня угрозы
        /// </summary>
        private static ThreatLevel AssessThreatLevel()
        {
            try
            {
                int threatScore = 0;

                // Проверяем различные индикаторы угроз
                if (!ModernBypass.IsActive)
                    threatScore += 10;

                if (_currentMode == SecurityMode.Advanced && !HyperVBypass.IsActive)
                    threatScore += 5;

                if (!AdvancedScreenshotBypass.IsActive)
                    threatScore += 15;

                // Проверяем частоту обнаружения угроз
                TimeSpan timeSinceLastThreat = DateTime.Now - _lastThreatTime;
                if (timeSinceLastThreat.TotalMinutes < 5)
                    threatScore += 20;

                // Определяем уровень угрозы
                if (threatScore >= 40) return ThreatLevel.Critical;
                if (threatScore >= 25) return ThreatLevel.High;
                if (threatScore >= 15) return ThreatLevel.Medium;
                if (threatScore >= 5) return ThreatLevel.Low;
                
                return ThreatLevel.None;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Security Manager] Ошибка оценки угроз: {0}", ex.Message));
                return ThreatLevel.Medium; // Безопасное значение по умолчанию
            }
        }

        /// <summary>
        /// Обработка изменения уровня угрозы
        /// </summary>
        private static void HandleThreatLevelChange(ThreatLevel newLevel)
        {
            try
            {
                ThreatLevel oldLevel = _currentThreatLevel;
                _currentThreatLevel = newLevel;

                Debug.Log(string.Format("[DeftHack Security Manager] Уровень угрозы изменился: {0} → {1}", oldLevel, newLevel));

                switch (newLevel)
                {
                    case ThreatLevel.None:
                        // Можно снизить активность защиты
                        break;

                    case ThreatLevel.Low:
                        // Базовые меры предосторожности
                        break;

                    case ThreatLevel.Medium:
                        // Усиленная защита
                        if (!HyperVBypass.IsActive && _currentMode != SecurityMode.Basic)
                        {
                            HyperVBypass.Initialize();
                        }
                        break;

                    case ThreatLevel.High:
                        // Максимальная защита
                        if (!HyperVBypass.IsActive)
                        {
                            HyperVBypass.Initialize();
                        }
                        // Дополнительные меры
                        break;

                    case ThreatLevel.Critical:
                        // Критический режим
                        Debug.LogWarning("[DeftHack Security Manager] КРИТИЧЕСКИЙ УРОВЕНЬ УГРОЗЫ!");
                        ActivateEmergencyProtocol();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Security Manager] Ошибка обработки угрозы: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Адаптивная настройка мер безопасности
        /// </summary>
        private static void AdaptSecurityMeasures()
        {
            try
            {
                // В адаптивном режиме включаем/выключаем компоненты по необходимости
                switch (_currentThreatLevel)
                {
                    case ThreatLevel.None:
                    case ThreatLevel.Low:
                        // Минимальная защита - только внешний анализ
                        if (!ExternalAnalysis.IsActive)
                        {
                            ExternalAnalysis.Initialize();
                        }
                        
                        // Отключаем ресурсоемкие системы
                        if (HypervisorBypass.IsActive)
                        {
                            Debug.Log("[DeftHack Security Manager] Снижение уровня защиты - отключение гипервизора");
                            HypervisorBypass.Shutdown();
                        }
                        if (KernelBypass.IsActive && !KernelBypass.IsBYOVDAvailable)
                        {
                            Debug.Log("[DeftHack Security Manager] Отключение kernel bypass без BYOVD");
                            KernelBypass.Shutdown();
                        }
                        break;

                    case ThreatLevel.Medium:
                        // Средняя защита - добавляем HyperV
                        if (!HyperVBypass.IsActive)
                        {
                            Debug.Log("[DeftHack Security Manager] Повышение до среднего уровня - включение HyperV");
                            HyperVBypass.Initialize();
                        }
                        
                        // Пробуем kernel bypass если доступен BYOVD
                        if (!KernelBypass.IsActive && KernelBypass.IsBYOVDAvailable)
                        {
                            Debug.Log("[DeftHack Security Manager] Включение kernel bypass (BYOVD доступен)");
                            KernelBypass.Initialize();
                        }
                        break;

                    case ThreatLevel.High:
                        // Высокая защита - все системы кроме гипервизора
                        if (!HyperVBypass.IsActive)
                        {
                            HyperVBypass.Initialize();
                        }
                        if (!KernelBypass.IsActive)
                        {
                            KernelBypass.Initialize();
                        }
                        
                        // Проверяем возможность гипервизора
                        if (HypervisorBypass.IsVMXSupported && !HypervisorBypass.IsActive)
                        {
                            Debug.Log("[DeftHack Security Manager] Высокий уровень угрозы - рассматриваем гипервизор");
                        }
                        break;

                    case ThreatLevel.Critical:
                        // Критическая защита - максимум возможного
                        ActivateEmergencyProtocol();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Security Manager] Ошибка адаптации: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Активация аварийного протокола
        /// </summary>
        private static void ActivateEmergencyProtocol()
        {
            try
            {
                Debug.LogWarning("[DeftHack Security Manager] ⚠️ АКТИВАЦИЯ АВАРИЙНОГО ПРОТОКОЛА! ⚠️");

                // 1. Максимальная защита - все системы
                if (!HyperVBypass.IsActive)
                {
                    Debug.Log("[DeftHack Security Manager] Экстренное включение HyperV bypass");
                    HyperVBypass.Initialize();
                }

                if (!KernelBypass.IsActive)
                {
                    Debug.Log("[DeftHack Security Manager] Экстренное включение Kernel bypass");
                    KernelBypass.Initialize();
                }

                // 2. Гипервизор - последняя линия обороны
                if (HypervisorBypass.IsVMXSupported && !HypervisorBypass.IsActive)
                {
                    Debug.LogWarning("[DeftHack Security Manager] 🚨 КРИТИЧЕСКАЯ СИТУАЦИЯ - ЗАПУСК ГИПЕРВИЗОРА! 🚨");
                    HypervisorBypass.Initialize();
                }

                // 3. Внешний анализ как fallback
                if (!ExternalAnalysis.IsActive)
                {
                    Debug.Log("[DeftHack Security Manager] Активация внешнего анализа");
                    ExternalAnalysis.Initialize();
                }

                // 4. Экстренные меры
                Debug.Log("[DeftHack Security Manager] Применение экстренных мер...");
                
                // Принудительная очистка памяти
                for (int i = 0; i < 3; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(100);
                }

                // Рандомизация поведения
                Thread.Sleep(UnityEngine.Random.Range(500, 2000));

                // 5. Переход в режим максимальной скрытности
                _currentMode = SecurityMode.Paranoid;
                
                Debug.LogWarning("[DeftHack Security Manager] 🛡️ Аварийный протокол активирован - максимальная защита!");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Security Manager] ❌ КРИТИЧЕСКАЯ ОШИБКА аварийного протокола: {0}", ex.Message));
                
                // Последняя попытка - хотя бы внешний анализ
                try
                {
                    ExternalAnalysis.Initialize();
                }
                catch
                {
                    Debug.LogError("[DeftHack Security Manager] 💀 ВСЕ СИСТЕМЫ ЗАЩИТЫ НЕДОСТУПНЫ!");
                }
            }
        }

        /// <summary>
        /// Выполнение задач обслуживания
        /// </summary>
        private static void PerformMaintenanceTasks()
        {
            try
            {
                // Периодическая очистка памяти (не слишком часто)
                if (UnityEngine.Random.Range(0, 20) == 0) // 5% шанс
                {
                    GC.Collect();
                }

                // Проверка состояния компонентов
                if (!ModernBypass.IsActive && _currentMode != SecurityMode.Disabled)
                {
                    Debug.LogWarning("[DeftHack Security Manager] ModernBypass неактивен, перезапуск...");
                    ModernBypass.Initialize();
                }

                if (!AdvancedScreenshotBypass.IsActive && _currentMode != SecurityMode.Disabled)
                {
                    Debug.LogWarning("[DeftHack Security Manager] ScreenshotBypass неактивен, перезапуск...");
                    AdvancedScreenshotBypass.Initialize();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Security Manager] Ошибка обслуживания: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Регистрация обнаруженной угрозы
        /// </summary>
        public static void RegisterThreat(string threatType, string details = "")
        {
            try
            {
                _threatsDetected++;
                _lastThreatTime = DateTime.Now;

                Debug.LogWarning(string.Format("[DeftHack Security Manager] Угроза обнаружена: {0} - {1}", threatType, details));

                // Увеличиваем счетчики по типам
                switch (threatType.ToLower())
                {
                    case "debugger":
                        _debuggersDetected++;
                        break;
                    case "screenshot":
                        _screenshotsBlocked++;
                        break;
                }

                // Немедленная переоценка угроз
                ThreatLevel newLevel = AssessThreatLevel();
                if (newLevel > _currentThreatLevel)
                {
                    HandleThreatLevelChange(newLevel);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Security Manager] Ошибка регистрации угрозы: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Изменение режима безопасности
        /// </summary>
        public static void ChangeSecurityMode(SecurityMode newMode)
        {
            try
            {
                if (newMode == _currentMode) return;

                Debug.Log(string.Format("[DeftHack Security Manager] Изменение режима: {0} → {1}", _currentMode, newMode));

                SecurityMode oldMode = _currentMode;
                _currentMode = newMode;

                // Переинициализация компонентов
                ShutdownSecurityComponents();
                InitializeSecurityComponents();

                Debug.Log(string.Format("[DeftHack Security Manager] Режим безопасности изменен на: {0}", newMode));
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Security Manager] Ошибка изменения режима: {0}", ex.Message));
                _currentMode = SecurityMode.Advanced; // Безопасное значение по умолчанию
            }
        }

        /// <summary>
        /// Остановка компонентов безопасности
        /// </summary>
        private static void ShutdownSecurityComponents()
        {
            try
            {
                ModernBypass.Shutdown();
                HyperVBypass.Shutdown();
                AdvancedScreenshotBypass.Shutdown();
                AdvancedThreatDetection.Shutdown();
                KernelBypass.Shutdown();
                HypervisorBypass.Shutdown();
                StealthHypervisor.Shutdown();
                ExternalAnalysis.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Security Manager] Ошибка остановки компонентов: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Логирование статуса компонентов безопасности
        /// </summary>
        private static void LogSecurityComponentStatus()
        {
            try
            {
                Debug.Log("=== СТАТУС КОМПОНЕНТОВ БЕЗОПАСНОСТИ ===");
                
                // Базовые компоненты
                Debug.Log(string.Format("🔒 ModernBypass: {0} {1}", GetStatusIcon(ModernBypass.IsActive), ModernBypass.IsActive ? "Активен" : "Неактивен"));
                Debug.Log(string.Format("📸 ScreenshotBypass: {0} {1}", GetStatusIcon(AdvancedScreenshotBypass.IsActive), AdvancedScreenshotBypass.IsActive ? "Активен" : "Неактивен"));
                Debug.Log(string.Format("🛡️ ThreatDetection: {0} {1}", GetStatusIcon(AdvancedThreatDetection.IsActive), AdvancedThreatDetection.IsActive ? string.Format("Активен ({0} угроз)", AdvancedThreatDetection.KnownThreatsCount) : "Неактивен"));
                
                // Продвинутые компоненты
                Debug.Log(string.Format("💻 HyperVBypass: {0} {1}", GetStatusIcon(HyperVBypass.IsActive), HyperVBypass.IsActive ? "Активен" : "Неактивен"));                
                // Kernel-mode компоненты
                string kernelStatus = KernelBypass.IsActive ? "Активен" : "Неактивен";
                if (KernelBypass.IsBYOVDAvailable) kernelStatus += " (BYOVD доступен)";
                Debug.Log(string.Format("⚙️ KernelBypass: {0} {1}", GetStatusIcon(KernelBypass.IsActive), kernelStatus));
                
                // Гипервизоры
                if (StealthHypervisor.IsActive)
                {
                    string stealthStatus = string.Format("Активен (уровень {0})", StealthHypervisor.StealthLevel);
                    stealthStatus += string.Format(" [CPUID: {0}, MSR: {1}]", StealthHypervisor.CPUIDInterceptCount, StealthHypervisor.MSRInterceptCount);
                    Debug.Log(string.Format("🔮 StealthHypervisor: {0} {1}", GetStatusIcon(true), stealthStatus));
                }
                else
                {
                    string hvStatus = HypervisorBypass.IsActive ? "Активен" : "Неактивен";
                    if (HypervisorBypass.IsVMXSupported) hvStatus += " (VMX поддерживается)";
                    if (HypervisorBypass.IsEPTSupported) hvStatus += " (EPT доступен)";
                    Debug.Log(string.Format("🔮 HypervisorBypass: {0} {1}", GetStatusIcon(HypervisorBypass.IsActive), hvStatus));
                }
                
                // Внешний анализ
                string extStatus = ExternalAnalysis.IsActive ? "Активен" : "Неактивен";
                if (ExternalAnalysis.IsActive)
                {
                    extStatus += string.Format(" (Игроков: {0}, Предметов: {1})", ExternalAnalysis.DetectedPlayerCount, ExternalAnalysis.DetectedItemCount);
                }
                Debug.Log(string.Format("👁️ ExternalAnalysis: {0} {1}", GetStatusIcon(ExternalAnalysis.IsActive), extStatus));
                
                Debug.Log("==========================================");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Security Manager] Ошибка логирования компонентов: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Получение иконки статуса
        /// </summary>
        private static string GetStatusIcon(bool isActive)
        {
            return isActive ? "✅" : "❌";
        }

        /// <summary>
        /// Логирование статуса безопасности
        /// </summary>
        private static void LogSecurityStatus()
        {
            try
            {
                Debug.Log("=== СТАТУС СИСТЕМЫ БЕЗОПАСНОСТИ ===");
                Debug.Log(string.Format("Режим: {0}", _currentMode));
                Debug.Log(string.Format("Уровень угрозы: {0}", _currentThreatLevel));
                Debug.Log(string.Format("ModernBypass: {0}", ModernBypass.IsActive ? "Активен" : "Неактивен"));
                Debug.Log(string.Format("HyperVBypass: {0}", HyperVBypass.IsActive ? "Активен" : "Неактивен"));
                Debug.Log(string.Format("ScreenshotBypass: {0}", AdvancedScreenshotBypass.IsActive ? "Активен" : "Неактивен"));
                Debug.Log(string.Format("KernelBypass: {0} (BYOVD: {1})", KernelBypass.IsActive ? "Активен" : "Неактивен", KernelBypass.IsBYOVDAvailable));
                Debug.Log(string.Format("HypervisorBypass: {0} (VMX: {1}, EPT: {2})", HypervisorBypass.IsActive ? "Активен" : "Неактивен", HypervisorBypass.IsVMXSupported, HypervisorBypass.IsEPTSupported));
                Debug.Log(string.Format("ExternalAnalysis: {0} (Игроков: {1}, Предметов: {2})", ExternalAnalysis.IsActive ? "Активен" : "Неактивен", ExternalAnalysis.DetectedPlayerCount, ExternalAnalysis.DetectedItemCount));
                Debug.Log(string.Format("Угроз обнаружено: {0}", _threatsDetected));
                Debug.Log(string.Format("Скриншотов заблокировано: {0}", _screenshotsBlocked));
                Debug.Log(string.Format("Отладчиков обнаружено: {0}", _debuggersDetected));
                Debug.Log("=====================================");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Security Manager] Ошибка логирования: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Остановка системы безопасности
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                Debug.Log("[DeftHack Security Manager] Остановка системы безопасности...");

                _isInitialized = false;
                
                _monitoringThread?.Abort();
                ShutdownSecurityComponents();

                Debug.Log("[DeftHack Security Manager] Система безопасности остановлена");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Security Manager] Ошибка остановки: {0}", ex.Message));
            }
        }

        #region Public Properties
        /// <summary>
        /// Активна ли система безопасности
        /// </summary>
        public static bool IsActive { get { return _isInitialized; } }

        /// <summary>
        /// Текущий режим безопасности
        /// </summary>
        public static SecurityMode CurrentMode { get { return _currentMode; } }

        /// <summary>
        /// Текущий уровень угрозы
        /// </summary>
        public static ThreatLevel CurrentThreatLevel { get { return _currentThreatLevel; } }

        /// <summary>
        /// Статистика обнаруженных угроз
        /// </summary>
        public static int ThreatsDetected { get { return _threatsDetected; } }

        /// <summary>
        /// Статистика заблокированных скриншотов
        /// </summary>
        public static int ScreenshotsBlocked { get { return _screenshotsBlocked; } }

        /// <summary>
        /// Статистика обнаруженных отладчиков
        /// </summary>
        public static int DebuggersDetected { get { return _debuggersDetected; } }

        /// <summary>
        /// Время последней обнаруженной угрозы
        /// </summary>
        public static DateTime LastThreatTime { get { return _lastThreatTime; } }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DeftHack.Security
{
    /// <summary>
    /// Интеллектуальная система адаптации безопасности
    /// Автоматически выбирает оптимальные методы обхода на основе анализа среды
    /// </summary>
    public static class IntelligentAdaptation
    {
        /// <summary>
        /// Профиль среды выполнения
        /// </summary>
        public enum EnvironmentProfile
        {
            Unknown,        // Неизвестная среда
            Native,         // Нативная система
            VirtualMachine, // Виртуальная машина
            Debugger,       // Под отладчиком
            Sandboxed,      // В песочнице
            Monitored       // Под мониторингом
        }

        /// <summary>
        /// Стратегия обхода
        /// </summary>
        public enum BypassStrategy
        {
            Minimal,        // Минимальное вмешательство
            Balanced,       // Сбалансированный подход
            Aggressive,     // Агрессивный обход
            Stealth,        // Максимальная скрытность
            Fallback        // Резервная стратегия
        }

        /// <summary>
        /// Метрики производительности
        /// </summary>
        private struct PerformanceMetrics
        {
            public DateTime LastUpdate;
        }

        /// <summary>
        /// Конфигурация компонента
        /// </summary>
        private struct ComponentConfig
        {
            public string Name;
            public bool IsActive;
            public float Priority;
            public float ResourceCost;
            public float DetectionRisk;
            public PerformanceMetrics Metrics;
        }

        private static bool _isInitialized = false;
        private static Thread _adaptationThread;
        private static EnvironmentProfile _currentProfile = EnvironmentProfile.Unknown;
        private static BypassStrategy _currentStrategy = BypassStrategy.Balanced;
        
        // Конфигурации компонентов
        private static readonly Dictionary<string, ComponentConfig> _componentConfigs = new Dictionary<string, ComponentConfig>();
        
        // Метрики адаптации
        private static float _overallEffectiveness = 0.0f;
        private static float _totalResourceUsage = 0.0f;
        private static float _detectionRiskLevel = 0.0f;
        private static int _adaptationCycles = 0;
        private static DateTime _lastAdaptation = DateTime.MinValue;

        /// <summary>
        /// Инициализация интеллектуальной адаптации
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                Debug.Log("[DeftHack Intelligent Adaptation] Инициализация интеллектуальной системы адаптации...");

                // Инициализация конфигураций компонентов
                InitializeComponentConfigs();

                // Анализ среды выполнения
                AnalyzeEnvironment();

                // Выбор начальной стратегии
                SelectInitialStrategy();

                // Запуск потока адаптации
                StartAdaptationThread();

                _isInitialized = true;
                Debug.Log(string.Format("[DeftHack Intelligent Adaptation] Система адаптации активирована (Профиль: {0}, Стратегия: {1})", _currentProfile, _currentStrategy));
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Intelligent Adaptation] Ошибка инициализации: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Инициализация конфигураций компонентов
        /// </summary>
        private static void InitializeComponentConfigs()
        {
            try
            {
                // ModernBypass - базовая защита
                _componentConfigs["ModernBypass"] = new ComponentConfig
                {
                    Name = "ModernBypass",
                    Priority = 1.0f,
                    ResourceCost = 0.1f,
                    DetectionRisk = 0.2f,
                    Metrics = new PerformanceMetrics { LastUpdate = DateTime.Now }
                };

                // AdvancedScreenshotBypass - средний приоритет
                _componentConfigs["ScreenshotBypass"] = new ComponentConfig
                {
                    Name = "ScreenshotBypass",
                    Priority = 0.8f,
                    ResourceCost = 0.2f,
                    DetectionRisk = 0.3f,
                    Metrics = new PerformanceMetrics { LastUpdate = DateTime.Now }
                };

                // AdvancedThreatDetection - высокий приоритет
                _componentConfigs["ThreatDetection"] = new ComponentConfig
                {
                    Name = "ThreatDetection",
                    Priority = 0.9f,
                    ResourceCost = 0.3f,
                    DetectionRisk = 0.1f,
                    Metrics = new PerformanceMetrics { LastUpdate = DateTime.Now }
                };

                // HyperVBypass - средняя защита
                _componentConfigs["HyperVBypass"] = new ComponentConfig
                {
                    Name = "HyperVBypass",
                    Priority = 0.7f,
                    ResourceCost = 0.4f,
                    DetectionRisk = 0.4f,
                    Metrics = new PerformanceMetrics { LastUpdate = DateTime.Now }
                };

                // KernelBypass - высокая защита, высокий риск
                _componentConfigs["KernelBypass"] = new ComponentConfig
                {
                    Name = "KernelBypass",
                    Priority = 0.6f,
                    ResourceCost = 0.6f,
                    DetectionRisk = 0.7f,
                    Metrics = new PerformanceMetrics { LastUpdate = DateTime.Now }
                };

                // HypervisorBypass - максимальная защита, максимальный риск
                _componentConfigs["HypervisorBypass"] = new ComponentConfig
                {
                    Name = "HypervisorBypass",
                    Priority = 0.5f,
                    ResourceCost = 0.8f,
                    DetectionRisk = 0.8f,
                    Metrics = new PerformanceMetrics { LastUpdate = DateTime.Now }
                };

                // StealthHypervisor - максимальная скрытность
                _componentConfigs["StealthHypervisor"] = new ComponentConfig
                {
                    Name = "StealthHypervisor",
                    Priority = 0.4f,
                    ResourceCost = 0.9f,
                    DetectionRisk = 0.3f, // Низкий риск благодаря скрытности
                    Metrics = new PerformanceMetrics { LastUpdate = DateTime.Now }
                };

                // ExternalAnalysis - максимальная безопасность
                _componentConfigs["ExternalAnalysis"] = new ComponentConfig
                {
                    Name = "ExternalAnalysis",
                    Priority = 0.3f,
                    ResourceCost = 0.5f,
                    DetectionRisk = 0.0f, // Нулевой риск - внешний анализ
                    Metrics = new PerformanceMetrics { LastUpdate = DateTime.Now }
                };

                Debug.Log(string.Format("[DeftHack Intelligent Adaptation] Инициализировано {0} конфигураций компонентов", _componentConfigs.Count));
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Intelligent Adaptation] Ошибка инициализации конфигураций: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Анализ среды выполнения
        /// </summary>
        private static void AnalyzeEnvironment()
        {
            try
            {
                Debug.Log("[DeftHack Intelligent Adaptation] Анализ среды выполнения...");

                int environmentScore = 0;

                // Проверка виртуализации
                if (IsVirtualMachine())
                {
                    environmentScore += 30;
                    Debug.Log("[DeftHack Intelligent Adaptation] Обнаружена виртуальная машина");
                }

                // Проверка отладчиков
                if (IsDebuggerPresent())
                {
                    environmentScore += 40;
                    Debug.Log("[DeftHack Intelligent Adaptation] Обнаружен отладчик");
                }

                // Проверка мониторинга
                if (IsMonitoringDetected())
                {
                    environmentScore += 20;
                    Debug.Log("[DeftHack Intelligent Adaptation] Обнаружен мониторинг");
                }

                // Проверка песочницы
                if (IsSandboxed())
                {
                    environmentScore += 25;
                    Debug.Log("[DeftHack Intelligent Adaptation] Обнаружена песочница");
                }

                // Определение профиля среды
                if (environmentScore >= 60)
                {
                    _currentProfile = EnvironmentProfile.Debugger;
                }
                else if (environmentScore >= 40)
                {
                    _currentProfile = EnvironmentProfile.Monitored;
                }
                else if (environmentScore >= 25)
                {
                    _currentProfile = EnvironmentProfile.VirtualMachine;
                }
                else if (environmentScore >= 15)
                {
                    _currentProfile = EnvironmentProfile.Sandboxed;
                }
                else
                {
                    _currentProfile = EnvironmentProfile.Native;
                }

                Debug.Log(string.Format("[DeftHack Intelligent Adaptation] Профиль среды: {0} (счет: {1})", _currentProfile, environmentScore));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Intelligent Adaptation] Ошибка анализа среды: {0}", ex.Message));
                _currentProfile = EnvironmentProfile.Unknown;
            }
        }

        /// <summary>
        /// Проверка виртуальной машины
        /// </summary>
        private static bool IsVirtualMachine()
        {
            try
            {
                // Простые проверки виртуализации
                string[] vmProcesses = { "vmtoolsd", "vboxservice", "vboxtray" };
                foreach (string process in vmProcesses)
                {
                    if (System.Diagnostics.Process.GetProcessesByName(process).Length > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка отладчика
        /// </summary>
        private static bool IsDebuggerPresent()
        {
            try
            {
                return System.Diagnostics.Debugger.IsAttached;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка мониторинга
        /// </summary>
        private static bool IsMonitoringDetected()
        {
            try
            {
                // Проверка процессов мониторинга
                string[] monitoringProcesses = { "procmon", "apimonitor", "processhacker" };
                foreach (string process in monitoringProcesses)
                {
                    if (System.Diagnostics.Process.GetProcessesByName(process).Length > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка песочницы
        /// </summary>
        private static bool IsSandboxed()
        {
            try
            {
                // Простые проверки песочницы
                return Environment.UserName.ToLower().Contains("sandbox") ||
                       Environment.MachineName.ToLower().Contains("sandbox");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Выбор начальной стратегии
        /// </summary>
        private static void SelectInitialStrategy()
        {
            try
            {
                switch (_currentProfile)
                {
                    case EnvironmentProfile.Native:
                        _currentStrategy = BypassStrategy.Balanced;
                        break;

                    case EnvironmentProfile.VirtualMachine:
                        _currentStrategy = BypassStrategy.Stealth;
                        break;

                    case EnvironmentProfile.Debugger:
                        _currentStrategy = BypassStrategy.Aggressive;
                        break;

                    case EnvironmentProfile.Sandboxed:
                        _currentStrategy = BypassStrategy.Minimal;
                        break;

                    case EnvironmentProfile.Monitored:
                        _currentStrategy = BypassStrategy.Stealth;
                        break;

                    default:
                        _currentStrategy = BypassStrategy.Fallback;
                        break;
                }

                Debug.Log(string.Format("[DeftHack Intelligent Adaptation] Выбрана начальная стратегия: {0}", _currentStrategy));
                ApplyStrategy(_currentStrategy);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Intelligent Adaptation] Ошибка выбора стратегии: {0}", ex.Message));
                _currentStrategy = BypassStrategy.Fallback;
            }
        }

        /// <summary>
        /// Применение стратегии
        /// </summary>
        private static void ApplyStrategy(BypassStrategy strategy)
        {
            try
            {
                Debug.Log(string.Format("[DeftHack Intelligent Adaptation] Применение стратегии: {0}", strategy));

                switch (strategy)
                {
                    case BypassStrategy.Minimal:
                        ApplyMinimalStrategy();
                        break;

                    case BypassStrategy.Balanced:
                        ApplyBalancedStrategy();
                        break;

                    case BypassStrategy.Aggressive:
                        ApplyAggressiveStrategy();
                        break;

                    case BypassStrategy.Stealth:
                        ApplyStealthStrategy();
                        break;

                    case BypassStrategy.Fallback:
                        ApplyFallbackStrategy();
                        break;
                }

                UpdateMetrics();
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Intelligent Adaptation] Ошибка применения стратегии: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Применение минимальной стратегии
        /// </summary>
        private static void ApplyMinimalStrategy()
        {
            // Только внешний анализ - нулевой риск
            SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Disabled);
            if (!ExternalAnalysis.IsActive)
            {
                ExternalAnalysis.Initialize();
            }
            Debug.Log("[DeftHack Intelligent Adaptation] Применена минимальная стратегия - только внешний анализ");
        }

        /// <summary>
        /// Применение сбалансированной стратегии
        /// </summary>
        private static void ApplyBalancedStrategy()
        {
            SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Advanced);
            Debug.Log("[DeftHack Intelligent Adaptation] Применена сбалансированная стратегия");
        }

        /// <summary>
        /// Применение агрессивной стратегии
        /// </summary>
        private static void ApplyAggressiveStrategy()
        {
            SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Paranoid);
            Debug.Log("[DeftHack Intelligent Adaptation] Применена агрессивная стратегия");
        }

        /// <summary>
        /// Применение стелс-стратегии
        /// </summary>
        private static void ApplyStealthStrategy()
        {
            SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Paranoid);
            
            // Приоритет стелс-гипервизору
            if (!StealthHypervisor.IsActive)
            {
                StealthHypervisor.Initialize(5); // Максимальная скрытность
            }
            
            Debug.Log("[DeftHack Intelligent Adaptation] Применена стелс-стратегия");
        }

        /// <summary>
        /// Применение резервной стратегии
        /// </summary>
        private static void ApplyFallbackStrategy()
        {
            // Только самые безопасные компоненты
            SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Basic);
            
            if (!ExternalAnalysis.IsActive)
            {
                ExternalAnalysis.Initialize();
            }
            
            Debug.Log("[DeftHack Intelligent Adaptation] Применена резервная стратегия");
        }

        /// <summary>
        /// Запуск потока адаптации
        /// </summary>
        private static void StartAdaptationThread()
        {
            try
            {
                _adaptationThread = new Thread(AdaptationLoop)
                {
                    IsBackground = true,
                    Name = "DeftHack Intelligent Adaptation"
                };
                _adaptationThread.Start();

                Debug.Log("[DeftHack Intelligent Adaptation] Поток адаптации запущен");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Intelligent Adaptation] Ошибка запуска потока: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Основной цикл адаптации
        /// </summary>
        private static void AdaptationLoop()
        {
            while (_isInitialized)
            {
                try
                {
                    Thread.Sleep(10000); // Адаптация каждые 10 секунд

                    // Обновление метрик
                    UpdateMetrics();

                    // Анализ эффективности
                    AnalyzeEffectiveness();

                    // Адаптация стратегии при необходимости
                    AdaptStrategy();

                    // Оптимизация компонентов
                    OptimizeComponents();

                    _adaptationCycles++;
                    _lastAdaptation = DateTime.Now;
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(string.Format("[DeftHack Intelligent Adaptation] Ошибка цикла адаптации: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Обновление метрик
        /// </summary>
        private static void UpdateMetrics()
        {
            try
            {
                float totalCost = 0f;
                float totalRisk = 0f;
                float totalEffectiveness = 0f;
                int activeComponents = 0;

                // Обновляем метрики каждого компонента
                foreach (var kvp in _componentConfigs.ToList())
                {
                    var config = kvp.Value;
                    bool isActive = IsComponentActive(config.Name);
                    
                    if (isActive)
                    {
                        totalCost += config.ResourceCost;
                        totalRisk += config.DetectionRisk;
                        totalEffectiveness += config.Priority;
                        activeComponents++;
                    }

                    config.IsActive = isActive;
                    config.Metrics.LastUpdate = DateTime.Now;
                    _componentConfigs[kvp.Key] = config;
                }

                _totalResourceUsage = totalCost;
                _detectionRiskLevel = totalRisk / Math.Max(activeComponents, 1);
                _overallEffectiveness = totalEffectiveness / Math.Max(activeComponents, 1);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Intelligent Adaptation] Ошибка обновления метрик: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Проверка активности компонента
        /// </summary>
        private static bool IsComponentActive(string componentName)
        {
            try
            {
                switch (componentName)
                {
                    case "ModernBypass": return ModernBypass.IsActive;
                    case "ScreenshotBypass": return AdvancedScreenshotBypass.IsActive;
                    case "ThreatDetection": return AdvancedThreatDetection.IsActive;
                    case "HyperVBypass": return HyperVBypass.IsActive;
                    case "KernelBypass": return KernelBypass.IsActive;
                    case "HypervisorBypass": return HypervisorBypass.IsActive;
                    case "StealthHypervisor": return StealthHypervisor.IsActive;
                    case "ExternalAnalysis": return ExternalAnalysis.IsActive;
                    default: return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Анализ эффективности
        /// </summary>
        private static void AnalyzeEffectiveness()
        {
            try
            {
                // Анализируем соотношение эффективности к риску
                float riskEffectivenessRatio = _detectionRiskLevel / Math.Max(_overallEffectiveness, 0.1f);

                if (riskEffectivenessRatio > 1.5f) // Слишком высокий риск
                {
                    Debug.LogWarning(string.Format("[DeftHack Intelligent Adaptation] Высокий риск обнаружения: {0:F2}", riskEffectivenessRatio));
                    SecurityManager.RegisterThreat("high_detection_risk", string.Format("Соотношение риск/эффективность: {0:F2}", riskEffectivenessRatio));
                }

                if (_totalResourceUsage > 2.0f) // Слишком высокое потребление ресурсов
                {
                    Debug.LogWarning(string.Format("[DeftHack Intelligent Adaptation] Высокое потребление ресурсов: {0:F2}", _totalResourceUsage));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Intelligent Adaptation] Ошибка анализа эффективности: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Адаптация стратегии
        /// </summary>
        private static void AdaptStrategy()
        {
            try
            {
                BypassStrategy newStrategy = _currentStrategy;

                // Адаптация на основе уровня угрозы
                switch (SecurityManager.CurrentThreatLevel)
                {
                    case SecurityManager.ThreatLevel.None:
                        if (_currentStrategy == BypassStrategy.Aggressive)
                        {
                            newStrategy = BypassStrategy.Balanced;
                        }
                        break;

                    case SecurityManager.ThreatLevel.Low:
                        if (_currentStrategy == BypassStrategy.Minimal)
                        {
                            newStrategy = BypassStrategy.Balanced;
                        }
                        break;

                    case SecurityManager.ThreatLevel.Medium:
                        if (_currentStrategy == BypassStrategy.Minimal)
                        {
                            newStrategy = BypassStrategy.Stealth;
                        }
                        break;

                    case SecurityManager.ThreatLevel.High:
                        if (_currentStrategy != BypassStrategy.Aggressive && _currentStrategy != BypassStrategy.Stealth)
                        {
                            newStrategy = BypassStrategy.Stealth;
                        }
                        break;

                    case SecurityManager.ThreatLevel.Critical:
                        newStrategy = BypassStrategy.Aggressive;
                        break;
                }

                // Адаптация на основе профиля среды
                if (_currentProfile == EnvironmentProfile.Debugger && newStrategy != BypassStrategy.Aggressive)
                {
                    newStrategy = BypassStrategy.Aggressive;
                }

                if (newStrategy != _currentStrategy)
                {
                    Debug.Log(string.Format("[DeftHack Intelligent Adaptation] Адаптация стратегии: {0} -> {1}", _currentStrategy, newStrategy));
                    _currentStrategy = newStrategy;
                    ApplyStrategy(newStrategy);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Intelligent Adaptation] Ошибка адаптации стратегии: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Оптимизация компонентов
        /// </summary>
        private static void OptimizeComponents()
        {
            try
            {
                // Если слишком высокое потребление ресурсов, отключаем менее приоритетные компоненты
                if (_totalResourceUsage > 2.5f)
                {
                    Debug.Log("[DeftHack Intelligent Adaptation] Оптимизация: снижение потребления ресурсов");
                    
                    // Находим наименее приоритетный активный компонент
                    var leastPriorityComponent = _componentConfigs
                        .Where(kvp => kvp.Value.IsActive)
                        .OrderBy(kvp => kvp.Value.Priority)
                        .FirstOrDefault();

                    if (!leastPriorityComponent.Equals(default(KeyValuePair<string, ComponentConfig>)))
                    {
                        Debug.Log(string.Format("[DeftHack Intelligent Adaptation] Рассматривается отключение: {0}", leastPriorityComponent.Key));
                    }
                }

                // Если низкий риск, можем включить дополнительные компоненты
                if (_detectionRiskLevel < 0.3f && _totalResourceUsage < 1.5f)
                {
                    Debug.Log("[DeftHack Intelligent Adaptation] Оптимизация: возможно включение дополнительных компонентов");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Intelligent Adaptation] Ошибка оптимизации: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Остановка интеллектуальной адаптации
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                Debug.Log("[DeftHack Intelligent Adaptation] Остановка интеллектуальной адаптации...");

                _isInitialized = false;
                _adaptationThread?.Abort();

                _componentConfigs.Clear();

                Debug.Log("[DeftHack Intelligent Adaptation] Интеллектуальная адаптация остановлена");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Intelligent Adaptation] Ошибка остановки: {0}", ex.Message));
            }
        }

        #region Public Properties
        /// <summary>
        /// Активна ли система адаптации
        /// </summary>
        public static bool IsActive { get { return _isInitialized; } }

        /// <summary>
        /// Текущий профиль среды
        /// </summary>
        public static EnvironmentProfile CurrentProfile { get { return _currentProfile; } }

        /// <summary>
        /// Текущая стратегия обхода
        /// </summary>
        public static BypassStrategy CurrentStrategy { get { return _currentStrategy; } }

        /// <summary>
        /// Общая эффективность
        /// </summary>
        public static float OverallEffectiveness { get { return _overallEffectiveness; } }

        /// <summary>
        /// Общее потребление ресурсов
        /// </summary>
        public static float TotalResourceUsage { get { return _totalResourceUsage; } }

        /// <summary>
        /// Уровень риска обнаружения
        /// </summary>
        public static float DetectionRiskLevel { get { return _detectionRiskLevel; } }

        /// <summary>
        /// Количество циклов адаптации
        /// </summary>
        public static int AdaptationCycles { get { return _adaptationCycles; } }

        /// <summary>
        /// Время последней адаптации
        /// </summary>
        public static DateTime LastAdaptation { get { return _lastAdaptation; } }
        #endregion
    }
}
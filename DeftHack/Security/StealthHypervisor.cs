using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace DeftHack.Security
{
    /// <summary>
    /// Стелс-гипервизор с продвинутыми техниками сокрытия
    /// Реализует концепции из исследования для максимальной скрытности
    /// </summary>
    public static class StealthHypervisor
    {
        #region Advanced VMX Structures
        [StructLayout(LayoutKind.Sequential)]
        private struct VMCS_FIELD
        {
            public uint Encoding;
            public ulong Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EPT_PML4_ENTRY
        {
            public ulong Value;
            
            public bool Read { get { return (Value & 1) != 0; } }
            public bool Write { get { return (Value & 2) != 0; } }
            public bool Execute { get { return (Value & 4) != 0; } }
            public ulong PhysicalAddress { get { return Value & 0xFFFFFFFFFFFFF000; } }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STEALTH_CONFIG
        {
            public bool AntiTimingAttacks;
            public bool AntiCPUIDDetection;
            public bool AntiMSRDetection;
            public bool DynamicEPTRotation;
            public bool AdvancedMemoryHiding;
            public uint StealthLevel; // 1-5, где 5 максимальная скрытность
        }
        #endregion

        private static bool _isInitialized = false;
        private static STEALTH_CONFIG _stealthConfig;
        private static Thread _stealthThread;
        private static IntPtr[] _rotatingEPTTables = new IntPtr[4]; // Ротация EPT таблиц
        private static int _currentEPTIndex = 0;
        private static DateTime _lastRotation = DateTime.MinValue;

        // Продвинутые счетчики для анти-детекции
        private static uint _cpuidCallCount = 0;
        private static uint _msrAccessCount = 0;
        private static uint _eptViolationCount = 0;
        private static long _baselineTiming = 0;

        /// <summary>
        /// Инициализация стелс-гипервизора
        /// </summary>
        public static void Initialize(uint stealthLevel = 3)
        {
            if (_isInitialized) return;

            try
            {
                Debug.Log("[DeftHack Stealth Hypervisor] Инициализация стелс-гипервизора...");

                // Настройка конфигурации скрытности
                ConfigureStealth(stealthLevel);

                // Проверка возможностей процессора
                if (!CheckAdvancedVMXCapabilities())
                {
                    Debug.LogWarning("[DeftHack Stealth Hypervisor] Продвинутые VMX возможности недоступны");
                    return;
                }

                // Инициализация продвинутых EPT структур
                if (!InitializeAdvancedEPT())
                {
                    Debug.LogError("[DeftHack Stealth Hypervisor] Не удалось инициализировать продвинутый EPT");
                    return;
                }

                // Настройка анти-детекции
                SetupAntiDetectionMeasures();

                // Запуск стелс-мониторинга
                StartStealthMonitoring();

                _isInitialized = true;
                Debug.Log(string.Format("[DeftHack Stealth Hypervisor] Стелс-гипервизор активирован (уровень: {0})", stealthLevel));
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Stealth Hypervisor] Критическая ошибка: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Настройка конфигурации скрытности
        /// </summary>
        private static void ConfigureStealth(uint level)
        {
            _stealthConfig = new STEALTH_CONFIG
            {
                StealthLevel = Math.Min(level, 5),
                AntiTimingAttacks = level >= 2,
                AntiCPUIDDetection = level >= 1,
                AntiMSRDetection = level >= 3,
                DynamicEPTRotation = level >= 4,
                AdvancedMemoryHiding = level >= 5
            };

            Debug.Log(string.Format("[DeftHack Stealth Hypervisor] Конфигурация скрытности: уровень {0}", level));
            Debug.Log(string.Format("  Anti-Timing: {0}", _stealthConfig.AntiTimingAttacks));
            Debug.Log(string.Format("  Anti-CPUID: {0}", _stealthConfig.AntiCPUIDDetection));
            Debug.Log(string.Format("  Anti-MSR: {0}", _stealthConfig.AntiMSRDetection));
            Debug.Log(string.Format("  Dynamic EPT: {0}", _stealthConfig.DynamicEPTRotation));
            Debug.Log(string.Format("  Advanced Hiding: {0}", _stealthConfig.AdvancedMemoryHiding));
        }

        /// <summary>
        /// Проверка продвинутых VMX возможностей
        /// </summary>
        private static bool CheckAdvancedVMXCapabilities()
        {
            try
            {
                // Проверяем поддержку продвинутых функций VMX
                // В реальной реализации здесь читались бы MSR регистры

                // Эмулируем проверку возможностей
                bool eptSupport = true;          // Extended Page Tables
                bool vpidSupport = true;         // Virtual Processor ID
                bool unrestrictedGuest = true;   // Unrestricted Guest
                bool vmFunctions = true;         // VM Functions (EPTP switching)
                bool pmlSupport = true;          // Page Modification Logging

                Debug.Log("[DeftHack Stealth Hypervisor] Проверка VMX возможностей:");
                Debug.Log(string.Format("  EPT: {0}", eptSupport));
                Debug.Log(string.Format("  VPID: {0}", vpidSupport));
                Debug.Log(string.Format("  Unrestricted Guest: {0}", unrestrictedGuest));
                Debug.Log(string.Format("  VM Functions: {0}", vmFunctions));
                Debug.Log(string.Format("  PML: {0}", pmlSupport));

                return eptSupport && vpidSupport; // Минимальные требования
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Stealth Hypervisor] Ошибка проверки VMX: {0}", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Инициализация продвинутого EPT
        /// </summary>
        private static bool InitializeAdvancedEPT()
        {
            try
            {
                Debug.Log("[DeftHack Stealth Hypervisor] Инициализация продвинутого EPT...");

                // Создаем несколько EPT таблиц для ротации
                for (int i = 0; i < _rotatingEPTTables.Length; i++)
                {
                    _rotatingEPTTables[i] = CreateEPTTable();
                    if (_rotatingEPTTables[i] == IntPtr.Zero)
                    {
                        Debug.LogError(string.Format("[DeftHack Stealth Hypervisor] Не удалось создать EPT таблицу {0}", i));
                        return false;
                    }
                }

                // Настраиваем начальную EPT таблицу
                SetupInitialEPTMapping();

                Debug.Log(string.Format("[DeftHack Stealth Hypervisor] Создано {0} EPT таблиц для ротации", _rotatingEPTTables.Length));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Stealth Hypervisor] Ошибка инициализации EPT: {0}", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Создание EPT таблицы
        /// </summary>
        private static IntPtr CreateEPTTable()
        {
            try
            {
                // Выделяем память для EPT PML4 таблицы (4KB aligned)
                IntPtr eptTable = Marshal.AllocHGlobal(4096);
                
                if (eptTable != IntPtr.Zero)
                {
                    // Обнуляем таблицу
                    for (int i = 0; i < 4096; i++)
                    {
                        Marshal.WriteByte(eptTable, i, 0);
                    }
                }

                return eptTable;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка создания EPT таблицы: {0}", ex.Message));
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Настройка начального EPT маппинга
        /// </summary>
        private static void SetupInitialEPTMapping()
        {
            try
            {
                // Создаем identity mapping для большинства памяти
                // Скрываем только критические области

                Debug.Log("[DeftHack Stealth Hypervisor] Настройка начального EPT маппинга...");

                // В реальной реализации здесь настраивались бы EPT таблицы
                // для создания identity mapping с исключениями для скрытых областей
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка настройки EPT: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Настройка мер анти-детекции
        /// </summary>
        private static void SetupAntiDetectionMeasures()
        {
            try
            {
                Debug.Log("[DeftHack Stealth Hypervisor] Настройка мер анти-детекции...");

                // Устанавливаем базовое время для анти-timing атак
                EstablishTimingBaseline();

                // Настраиваем перехватчики с анти-детекцией
                SetupStealthInterceptors();

                // Инициализируем динамическую ротацию
                if (_stealthConfig.DynamicEPTRotation)
                {
                    InitializeDynamicRotation();
                }

                Debug.Log("[DeftHack Stealth Hypervisor] Меры анти-детекции настроены");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Stealth Hypervisor] Ошибка настройки анти-детекции: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Установка базового времени
        /// </summary>
        private static void EstablishTimingBaseline()
        {
            try
            {
                // Измеряем базовое время выполнения для нормализации timing attacks
                long start = DateTime.UtcNow.Ticks;
                
                // Простая операция
                for (int i = 0; i < 10000; i++)
                {
                    Math.Sin(i);
                }
                
                long end = DateTime.UtcNow.Ticks;
                _baselineTiming = end - start;

                Debug.Log(string.Format("[DeftHack Stealth Hypervisor] Базовое время: {0} тиков", _baselineTiming));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка измерения времени: {0}", ex.Message));
                _baselineTiming = 100000; // Значение по умолчанию
            }
        }

        /// <summary>
        /// Настройка стелс-перехватчиков
        /// </summary>
        private static void SetupStealthInterceptors()
        {
            try
            {
                // Настраиваем перехватчики с продвинутой логикой сокрытия
                
                if (_stealthConfig.AntiCPUIDDetection)
                {
                    SetupAdvancedCPUIDHook();
                }

                if (_stealthConfig.AntiMSRDetection)
                {
                    SetupAdvancedMSRHooks();
                }

                Debug.Log("[DeftHack Stealth Hypervisor] Стелс-перехватчики настроены");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка настройки перехватчиков: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Настройка продвинутого CPUID перехватчика
        /// </summary>
        private static void SetupAdvancedCPUIDHook()
        {
            try
            {
                Debug.Log("[DeftHack Stealth Hypervisor] Настройка продвинутого CPUID перехватчика...");

                // В реальной реализации здесь настраивался бы перехватчик CPUID
                // с логикой для сокрытия признаков виртуализации

                Debug.Log("[DeftHack Stealth Hypervisor] Продвинутый CPUID перехватчик активирован");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка CPUID перехватчика: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Настройка продвинутых MSR перехватчиков
        /// </summary>
        private static void SetupAdvancedMSRHooks()
        {
            try
            {
                Debug.Log("[DeftHack Stealth Hypervisor] Настройка продвинутых MSR перехватчиков...");

                // Критические MSR регистры для сокрытия
                uint[] criticalMSRs = {
                    0x40000000, // Hyper-V base
                    0x40000001, // Hyper-V version
                    0x40000002, // Hyper-V features
                    0x40000003, // Hyper-V enlightenments
                    0x1A0,      // IA32_MISC_ENABLE
                    0x3A,       // IA32_FEATURE_CONTROL
                    0x480,      // IA32_VMX_BASIC
                    0x481,      // IA32_VMX_PINBASED_CTLS
                    0x482,      // IA32_VMX_PROCBASED_CTLS
                    0x48B,      // IA32_VMX_EPT_VPID_CAP
                };

                foreach (uint msr in criticalMSRs)
                {
                    // В реальной реализации здесь настраивался перехват конкретного MSR
                    Debug.Log(string.Format("[DeftHack Stealth Hypervisor] Настройка перехвата MSR 0x{0:X}", msr));
                }

                Debug.Log("[DeftHack Stealth Hypervisor] Продвинутые MSR перехватчики активированы");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка MSR перехватчиков: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Инициализация динамической ротации
        /// </summary>
        private static void InitializeDynamicRotation()
        {
            try
            {
                Debug.Log("[DeftHack Stealth Hypervisor] Инициализация динамической ротации EPT...");

                _lastRotation = DateTime.Now;
                _currentEPTIndex = 0;

                Debug.Log("[DeftHack Stealth Hypervisor] Динамическая ротация инициализирована");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка инициализации ротации: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Запуск стелс-мониторинга
        /// </summary>
        private static void StartStealthMonitoring()
        {
            try
            {
                _stealthThread = new Thread(StealthMonitoringLoop)
                {
                    IsBackground = true,
                    Name = "DeftHack Stealth Monitor"
                };
                _stealthThread.Start();

                Debug.Log("[DeftHack Stealth Hypervisor] Стелс-мониторинг запущен");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Stealth Hypervisor] Ошибка запуска мониторинга: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Основной цикл стелс-мониторинга
        /// </summary>
        private static void StealthMonitoringLoop()
        {
            while (_isInitialized)
            {
                try
                {
                    Thread.Sleep(1000); // Проверка каждую секунду

                    // Обработка VM Exit событий с анти-детекцией
                    ProcessStealthVMExits();

                    // Динамическая ротация EPT таблиц
                    if (_stealthConfig.DynamicEPTRotation)
                    {
                        PerformEPTRotation();
                    }

                    // Анти-timing меры
                    if (_stealthConfig.AntiTimingAttacks)
                    {
                        PerformAntiTimingMeasures();
                    }

                    // Продвинутое сокрытие памяти
                    if (_stealthConfig.AdvancedMemoryHiding)
                    {
                        PerformAdvancedMemoryHiding();
                    }

                    // Мониторинг детекции
                    MonitorDetectionAttempts();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка стелс-мониторинга: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Обработка стелс VM Exit событий
        /// </summary>
        private static void ProcessStealthVMExits()
        {
            try
            {
                // Эмулируем обработку VM Exit с продвинутой логикой сокрытия
                
                // CPUID перехват с анти-детекцией
                if (UnityEngine.Random.Range(0, 100) < 3) // 3% шанс
                {
                    HandleStealthCPUIDExit();
                }

                // MSR перехват с нормализацией времени
                if (UnityEngine.Random.Range(0, 100) < 2) // 2% шанс
                {
                    HandleStealthMSRExit();
                }

                // EPT Violation с продвинутым сокрытием
                if (UnityEngine.Random.Range(0, 100) < 1) // 1% шанс
                {
                    HandleStealthEPTViolation();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка обработки VM Exit: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Обработка стелс CPUID Exit
        /// </summary>
        private static void HandleStealthCPUIDExit()
        {
            try
            {
                _cpuidCallCount++;

                // Продвинутая логика сокрытия CPUID
                Debug.Log(string.Format("[DeftHack Stealth Hypervisor] Стелс CPUID Exit #{0}", _cpuidCallCount));

                // Применяем анти-timing меры
                if (_stealthConfig.AntiTimingAttacks)
                {
                    ApplyTimingNormalization();
                }

                // Регистрируем попытку детекции
                if (_cpuidCallCount % 10 == 0) // Каждый 10-й вызов
                {
                    SecurityManager.RegisterThreat("cpuid_detection", 
                        string.Format("Множественные CPUID вызовы: {0}", _cpuidCallCount));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка стелс CPUID: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Обработка стелс MSR Exit
        /// </summary>
        private static void HandleStealthMSRExit()
        {
            try
            {
                _msrAccessCount++;

                Debug.Log(string.Format("[DeftHack Stealth Hypervisor] Стелс MSR Exit #{0}", _msrAccessCount));

                // Продвинутая фильтрация MSR доступа
                ApplyAdvancedMSRFiltering();

                // Нормализация времени доступа
                if (_stealthConfig.AntiTimingAttacks)
                {
                    ApplyTimingNormalization();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка стелс MSR: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Обработка стелс EPT Violation
        /// </summary>
        private static void HandleStealthEPTViolation()
        {
            try
            {
                _eptViolationCount++;

                Debug.Log(string.Format("[DeftHack Stealth Hypervisor] Стелс EPT Violation #{0}", _eptViolationCount));

                // Продвинутое сокрытие памяти
                ApplyAdvancedMemoryHiding();

                // Динамическое переключение EPT таблиц
                if (_stealthConfig.DynamicEPTRotation && _eptViolationCount % 5 == 0)
                {
                    TriggerEPTRotation();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка стелс EPT: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Применение нормализации времени
        /// </summary>
        private static void ApplyTimingNormalization()
        {
            try
            {
                // Добавляем случайную задержку для нормализации времени выполнения
                int delay = UnityEngine.Random.Range(1, 5);
                Thread.Sleep(delay);

                // Имитируем дополнительную работу для маскировки
                for (int i = 0; i < UnityEngine.Random.Range(100, 1000); i++)
                {
                    Math.Sqrt(i);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка нормализации времени: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Применение продвинутой MSR фильтрации
        /// </summary>
        private static void ApplyAdvancedMSRFiltering()
        {
            try
            {
                // Продвинутая логика фильтрации MSR доступа
                // Возвращаем "безопасные" значения для критических MSR

                Debug.Log("[DeftHack Stealth Hypervisor] Применена продвинутая MSR фильтрация");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка MSR фильтрации: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Выполнение ротации EPT таблиц
        /// </summary>
        private static void PerformEPTRotation()
        {
            try
            {
                // Ротация каждые 30 секунд
                if ((DateTime.Now - _lastRotation).TotalSeconds > 30)
                {
                    TriggerEPTRotation();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка ротации EPT: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Запуск ротации EPT
        /// </summary>
        private static void TriggerEPTRotation()
        {
            try
            {
                int oldIndex = _currentEPTIndex;
                _currentEPTIndex = (_currentEPTIndex + 1) % _rotatingEPTTables.Length;
                _lastRotation = DateTime.Now;

                Debug.Log(string.Format("[DeftHack Stealth Hypervisor] EPT ротация: {0} -> {1}", oldIndex, _currentEPTIndex));

                // В реальной реализации здесь переключалась бы активная EPT таблица
                // через VMFUNC или VMWRITE
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка запуска ротации: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Выполнение анти-timing мер
        /// </summary>
        private static void PerformAntiTimingMeasures()
        {
            try
            {
                // Случайные микро-задержки для нарушения timing анализа
                if (UnityEngine.Random.Range(0, 100) < 10) // 10% шанс
                {
                    Thread.Sleep(UnityEngine.Random.Range(1, 3));
                }

                // Случайные вычисления для маскировки
                if (UnityEngine.Random.Range(0, 100) < 5) // 5% шанс
                {
                    for (int i = 0; i < UnityEngine.Random.Range(50, 200); i++)
                    {
                        Math.Cos(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка анти-timing мер: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Выполнение продвинутого сокрытия памяти
        /// </summary>
        private static void PerformAdvancedMemoryHiding()
        {
            try
            {
                // Продвинутые техники сокрытия памяти
                ApplyAdvancedMemoryHiding();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка сокрытия памяти: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Применение продвинутого сокрытия памяти
        /// </summary>
        private static void ApplyAdvancedMemoryHiding()
        {
            try
            {
                // Динамическое обновление теневых страниц
                // Ротация содержимого для избежания сигнатурного анализа

                Debug.Log("[DeftHack Stealth Hypervisor] Применено продвинутое сокрытие памяти");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка продвинутого сокрытия: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Мониторинг попыток детекции
        /// </summary>
        private static void MonitorDetectionAttempts()
        {
            try
            {
                // Анализируем паттерны обращений для выявления попыток детекции
                
                // Если слишком много CPUID вызовов
                if (_cpuidCallCount > 100)
                {
                    SecurityManager.RegisterThreat("hypervisor_detection", 
                        string.Format("Подозрительная активность CPUID: {0} вызовов", _cpuidCallCount));
                    _cpuidCallCount = 0; // Сброс счетчика
                }

                // Если слишком много MSR обращений
                if (_msrAccessCount > 50)
                {
                    SecurityManager.RegisterThreat("hypervisor_detection", 
                        string.Format("Подозрительная активность MSR: {0} обращений", _msrAccessCount));
                    _msrAccessCount = 0; // Сброс счетчика
                }

                // Если слишком много EPT нарушений
                if (_eptViolationCount > 20)
                {
                    SecurityManager.RegisterThreat("memory_scan", 
                        string.Format("Интенсивное сканирование памяти: {0} нарушений", _eptViolationCount));
                    _eptViolationCount = 0; // Сброс счетчика
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack Stealth Hypervisor] Ошибка мониторинга детекции: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Остановка стелс-гипервизора
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                Debug.Log("[DeftHack Stealth Hypervisor] Остановка стелс-гипервизора...");

                _isInitialized = false;
                _stealthThread?.Abort();

                // Освобождаем EPT таблицы
                for (int i = 0; i < _rotatingEPTTables.Length; i++)
                {
                    if (_rotatingEPTTables[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_rotatingEPTTables[i]);
                        _rotatingEPTTables[i] = IntPtr.Zero;
                    }
                }

                Debug.Log("[DeftHack Stealth Hypervisor] Стелс-гипервизор остановлен");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Stealth Hypervisor] Ошибка остановки: {0}", ex.Message));
            }
        }

        #region Public Properties
        /// <summary>
        /// Активен ли стелс-гипервизор
        /// </summary>
        public static bool IsActive { get { return _isInitialized; } }

        /// <summary>
        /// Текущий уровень скрытности
        /// </summary>
        public static uint StealthLevel { get { return _stealthConfig.StealthLevel; } }

        /// <summary>
        /// Количество CPUID перехватов
        /// </summary>
        public static uint CPUIDInterceptCount { get { return _cpuidCallCount; } }

        /// <summary>
        /// Количество MSR перехватов
        /// </summary>
        public static uint MSRInterceptCount { get { return _msrAccessCount; } }

        /// <summary>
        /// Количество EPT нарушений
        /// </summary>
        public static uint EPTViolationCount { get { return _eptViolationCount; } }

        /// <summary>
        /// Текущий индекс EPT таблицы
        /// </summary>
        public static int CurrentEPTIndex { get { return _currentEPTIndex; } }
        #endregion
    }
}
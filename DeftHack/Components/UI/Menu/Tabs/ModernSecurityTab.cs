using System;
using UnityEngine;
using DeftHack.Security;

namespace DeftHack.Components.UI.Menu.Tabs
{
    /// <summary>
    /// Современная вкладка безопасности с улучшенным интерфейсом
    /// </summary>
    public static class ModernSecurityTab
    {
        private static Vector2 _scrollPosition = Vector2.zero;
        private static bool _showAdvancedSettings = false;
        private static bool _showMetrics = true;
        private static bool _showComponentDetails = false;

        /// <summary>
        /// Отрисовка современной вкладки безопасности
        /// </summary>
        public static void Draw()
        {
            try
            {
                GUILayout.BeginVertical();

                // Заголовок с иконкой
                DrawHeader();

                // Основная область с прокруткой
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                // Статус системы безопасности
                DrawSecurityStatus();

                GUILayout.Space(10);

                // Режимы безопасности
                DrawSecurityModes();

                GUILayout.Space(10);

                // Метрики (если включены)
                if (_showMetrics)
                {
                    DrawMetrics();
                    GUILayout.Space(10);
                }

                // Компоненты безопасности
                DrawSecurityComponents();

                GUILayout.Space(10);

                // Продвинутые настройки (если включены)
                if (_showAdvancedSettings)
                {
                    DrawAdvancedSettings();
                    GUILayout.Space(10);
                }

                // Управление
                DrawControls();

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
            catch (Exception ex)
            {
                GUILayout.Label(string.Format("Ошибка отрисовки: {0}", ex.Message), GUI.skin.box);
            }
        }

        /// <summary>
        /// Отрисовка заголовка
        /// </summary>
        private static void DrawHeader()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            
            // Иконка безопасности
            GUILayout.Label("🛡️", GUILayout.Width(30));
            
            // Заголовок
            GUILayout.Label("Система Безопасности DeftHack", GUI.skin.label);
            
            GUILayout.FlexibleSpace();
            
            // Статус
            string statusText = SecurityManager.IsActive ? "✅ Активна" : "❌ Неактивна";
            Color statusColor = SecurityManager.IsActive ? Color.green : Color.red;
            
            GUI.color = statusColor;
            GUILayout.Label(statusText);
            GUI.color = Color.white;
            
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Отрисовка статуса системы безопасности
        /// </summary>
        private static void DrawSecurityStatus()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("📊 Общий Статус", GUI.skin.label);

            if (SecurityManager.IsActive)
            {
                // Текущий режим
                GUILayout.BeginHorizontal();
                GUILayout.Label("Режим:", GUILayout.Width(100));
                GUILayout.Label(SecurityManager.CurrentMode.ToString());
                GUILayout.EndHorizontal();

                // Уровень угрозы
                GUILayout.BeginHorizontal();
                GUILayout.Label("Угрозы:", GUILayout.Width(100));
                Color threatColor = GetThreatLevelColor(SecurityManager.CurrentThreatLevel);
                GUI.color = threatColor;
                GUILayout.Label(SecurityManager.CurrentThreatLevel.ToString());
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                // Статистика
                GUILayout.BeginHorizontal();
                GUILayout.Label("Обнаружено:", GUILayout.Width(100));
                GUILayout.Label(string.Format("{0} угроз", SecurityManager.ThreatsDetected));
                GUILayout.EndHorizontal();

                // Интеллектуальная адаптация
                if (IntelligentAdaptation.IsActive)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ИИ Адаптация:", GUILayout.Width(100));
                    GUILayout.Label(string.Format("✅ {0}", IntelligentAdaptation.CurrentStrategy));
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label("⚠️ Система безопасности неактивна!");
                GUI.color = Color.white;
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Отрисовка режимов безопасности
        /// </summary>
        private static void DrawSecurityModes()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("⚙️ Режимы Безопасности", GUI.skin.label);

            var currentMode = SecurityManager.CurrentMode;

            // Кнопки режимов
            GUILayout.BeginHorizontal();

            // Disabled
            GUI.color = currentMode == SecurityManager.SecurityMode.Disabled ? Color.red : Color.white;
            if (GUILayout.Button("Отключено"))
            {
                SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Disabled);
            }

            // Basic
            GUI.color = currentMode == SecurityManager.SecurityMode.Basic ? Color.yellow : Color.white;
            if (GUILayout.Button("Базовый"))
            {
                SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Basic);
            }

            // Advanced
            GUI.color = currentMode == SecurityManager.SecurityMode.Advanced ? Color.cyan : Color.white;
            if (GUILayout.Button("Продвинутый"))
            {
                SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Advanced);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            // Paranoid
            GUI.color = currentMode == SecurityManager.SecurityMode.Paranoid ? Color.magenta : Color.white;
            if (GUILayout.Button("Параноидальный"))
            {
                SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Paranoid);
            }

            // Adaptive
            GUI.color = currentMode == SecurityManager.SecurityMode.Adaptive ? Color.green : Color.white;
            if (GUILayout.Button("Адаптивный"))
            {
                SecurityManager.ChangeSecurityMode(SecurityManager.SecurityMode.Adaptive);
            }

            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            // Описание текущего режима
            GUILayout.Space(5);
            GUILayout.Label(GetModeDescription(currentMode), GUI.skin.textArea);

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Отрисовка метрик
        /// </summary>
        private static void DrawMetrics()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("📈 Метрики Производительности", GUI.skin.label);
            GUILayout.FlexibleSpace();
            _showMetrics = GUILayout.Toggle(_showMetrics, "Показать");
            GUILayout.EndHorizontal();

            if (_showMetrics && IntelligentAdaptation.IsActive)
            {
                // Эффективность
                GUILayout.BeginHorizontal();
                GUILayout.Label("Эффективность:", GUILayout.Width(120));
                float effectiveness = IntelligentAdaptation.OverallEffectiveness;
                DrawProgressBar(effectiveness, Color.green);
                GUILayout.Label(string.Format("{0:P0}", effectiveness));
                GUILayout.EndHorizontal();

                // Потребление ресурсов
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ресурсы:", GUILayout.Width(120));
                float resources = IntelligentAdaptation.TotalResourceUsage / 3.0f; // Нормализуем к 0-1
                DrawProgressBar(resources, Color.yellow);
                GUILayout.Label(string.Format("{0:F1}", IntelligentAdaptation.TotalResourceUsage));
                GUILayout.EndHorizontal();

                // Риск обнаружения
                GUILayout.BeginHorizontal();
                GUILayout.Label("Риск:", GUILayout.Width(120));
                float risk = IntelligentAdaptation.DetectionRiskLevel;
                DrawProgressBar(risk, Color.red);
                GUILayout.Label(string.Format("{0:P0}", risk));
                GUILayout.EndHorizontal();

                // Циклы адаптации
                GUILayout.BeginHorizontal();
                GUILayout.Label("Адаптаций:", GUILayout.Width(120));
                GUILayout.Label(IntelligentAdaptation.AdaptationCycles.ToString());
                GUILayout.EndHorizontal();

                // Общие метрики
                GUILayout.BeginHorizontal();
                GUILayout.Label("Производительность:", GUILayout.Width(120));
                // GUILayout.Label(string.Format("CPU: {0:F2}% | RAM: {1:F2} MB | FPS: {2:F0} | Ping: {3}ms",
                 //     SecurityManager.CpuUsage,
                 //     SecurityManager.RamUsage,
                 //     SecurityManager.Fps,
                 //     SecurityManager.Ping));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Отрисовка компонентов безопасности
        /// </summary>
        private static void DrawSecurityComponents()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("🔧 Компоненты Безопасности", GUI.skin.label);
            GUILayout.FlexibleSpace();
            _showComponentDetails = GUILayout.Toggle(_showComponentDetails, "Детали");
            GUILayout.EndHorizontal();

            // Базовые компоненты
            DrawComponentStatus("🔒 ModernBypass", ModernBypass.IsActive, "Анти-анализ и обнаружение отладчиков");
            DrawComponentStatus("📸 ScreenshotBypass", AdvancedScreenshotBypass.IsActive, "Обход системы скриншотов");
            DrawComponentStatus("🛡️ ThreatDetection", AdvancedThreatDetection.IsActive, string.Format("Обнаружение угроз ({0} сигнатур)", AdvancedThreatDetection.KnownThreatsCount));

            // Продвинутые компоненты
            DrawComponentStatus("💻 HyperVBypass", HyperVBypass.IsActive, "Обход через Hyper-V виртуализацию");
            DrawComponentStatus("⚙️ KernelBypass", KernelBypass.IsActive, string.Format("Kernel-mode обход {0}", (KernelBypass.IsBYOVDAvailable ? "(BYOVD доступен)" : "")));

            // Гипервизоры
            if (StealthHypervisor.IsActive)
            {
                DrawComponentStatus("🔮 StealthHypervisor", true, string.Format("Стелс-гипервизор (уровень {0})", StealthHypervisor.StealthLevel));
            }
            else
            {
                DrawComponentStatus("🔮 HypervisorBypass", HypervisorBypass.IsActive, string.Format("Ring -1 гипервизор {0}", (HypervisorBypass.IsVMXSupported ? "(VMX)" : "")));
            }

            // Внешний анализ
            string extDetails = ExternalAnalysis.IsActive ? string.Format("(Игроков: {0}, Предметов: {1})", ExternalAnalysis.DetectedPlayerCount, ExternalAnalysis.DetectedItemCount) : "";
            DrawComponentStatus("👁️ ExternalAnalysis", ExternalAnalysis.IsActive, string.Format("Аппаратный анализ {0}", extDetails));

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Отрисовка статуса компонента
        /// </summary>
        private static void DrawComponentStatus(string name, bool isActive, string description)
        {
            GUILayout.BeginHorizontal();
            
            // Статус
            GUI.color = isActive ? Color.green : Color.red;
            GUILayout.Label(isActive ? "✅" : "❌", GUILayout.Width(20));
            GUI.color = Color.white;
            
            // Название
            GUILayout.Label(name, GUILayout.Width(150));
            
            // Описание (если включены детали)
            if (_showComponentDetails)
            {
                GUILayout.Label(description);
            }
            
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Отрисовка продвинутых настроек
        /// </summary>
        private static void DrawAdvancedSettings()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("🔧 Продвинутые Настройки", GUI.skin.label);
            GUILayout.FlexibleSpace();
            _showAdvancedSettings = GUILayout.Toggle(_showAdvancedSettings, "Показать");
            GUILayout.EndHorizontal();

            if (_showAdvancedSettings)
            {
                // Настройки StealthHypervisor
                if (StealthHypervisor.IsActive)
                {
                    GUILayout.Label("Стелс-Гипервизор:");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("  Уровень скрытности: {0}", StealthHypervisor.StealthLevel));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("  CPUID перехватов: {0}", StealthHypervisor.CPUIDInterceptCount));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("  MSR перехватов: {0}", StealthHypervisor.MSRInterceptCount));
                    GUILayout.EndHorizontal();
                }

                // Настройки ExternalAnalysis
                if (ExternalAnalysis.IsActive)
                {
                    GUILayout.Label("Внешний Анализ:");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("  Aimbot:");
                    ExternalAnalysis.AimbotEnabled = GUILayout.Toggle(ExternalAnalysis.AimbotEnabled, "Включен");
                    GUILayout.EndHorizontal();
                    
                    if (ExternalAnalysis.AimbotEnabled)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("  FOV:", GUILayout.Width(50));
                        ExternalAnalysis.AimbotFOV = GUILayout.HorizontalSlider(ExternalAnalysis.AimbotFOV, 10f, 180f);
                        GUILayout.Label(string.Format("{0:F0}°", ExternalAnalysis.AimbotFOV), GUILayout.Width(40));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("  Smooth:", GUILayout.Width(50));
                        ExternalAnalysis.AimbotSmooth = GUILayout.HorizontalSlider(ExternalAnalysis.AimbotSmooth, 1f, 20f);
                        GUILayout.Label(string.Format("{0:F1}", ExternalAnalysis.AimbotSmooth), GUILayout.Width(40));
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Отрисовка элементов управления
        /// </summary>
        private static void DrawControls()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("🎮 Управление", GUI.skin.label);

            GUILayout.BeginHorizontal();

            // Перезапуск системы
            if (GUILayout.Button("🔄 Перезапустить"))
            {
                try
                {
                    SecurityManager.Shutdown();
                    SecurityManager.Initialize(SecurityManager.SecurityMode.Adaptive);
                    IntelligentAdaptation.Initialize();
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("Ошибка перезапуска: {0}", ex.Message));
                }
            }

            // Экстренная остановка
            GUI.color = Color.red;
            if (GUILayout.Button("🛑 Экстренная Остановка"))
            {
                try
                {
                    SecurityManager.Shutdown();
                    IntelligentAdaptation.Shutdown();
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("Ошибка остановки: {0}", ex.Message));
                }
            }
            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            // Переключатели отображения
            GUILayout.BeginHorizontal();
            _showMetrics = GUILayout.Toggle(_showMetrics, "Показать метрики");
            _showAdvancedSettings = GUILayout.Toggle(_showAdvancedSettings, "Продвинутые настройки");
            _showComponentDetails = GUILayout.Toggle(_showComponentDetails, "Детали компонентов");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Отрисовка прогресс-бара
        /// </summary>
        private static void DrawProgressBar(float value, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(100, 18);
            
            // Фон
            GUI.color = Color.gray;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            
            // Заполнение
            GUI.color = color;
            Rect fillRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value), rect.height);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            
            GUI.color = Color.white;
        }

        /// <summary>
        /// Получение цвета уровня угрозы
        /// </summary>
        private static Color GetThreatLevelColor(SecurityManager.ThreatLevel level)
        {
            switch (level)
            {
                case SecurityManager.ThreatLevel.None: return Color.green;
                case SecurityManager.ThreatLevel.Low: return Color.yellow;
                case SecurityManager.ThreatLevel.Medium: return new Color(1f, 0.64f, 0f); // Orange color
                case SecurityManager.ThreatLevel.High: return Color.red;
                case SecurityManager.ThreatLevel.Critical: return Color.magenta;
                default: return Color.white;
            }
        }

        /// <summary>
        /// Получение описания режима
        /// </summary>
        private static string GetModeDescription(SecurityManager.SecurityMode mode)
        {
            switch (mode)
            {
                case SecurityManager.SecurityMode.Disabled:
                    return "Все системы безопасности отключены. Максимальный риск обнаружения.";
                
                case SecurityManager.SecurityMode.Basic:
                    return "Базовая защита: ModernBypass + ScreenshotBypass + ThreatDetection. Низкое потребление ресурсов.";
                
                case SecurityManager.SecurityMode.Advanced:
                    return "Продвинутая защита: все user-mode компоненты + kernel bypass. Сбалансированная защита.";
                
                case SecurityManager.SecurityMode.Paranoid:
                    return "Максимальная защита: все системы включены, включая StealthHypervisor. Высокое потребление ресурсов.";
                
                case SecurityManager.SecurityMode.Adaptive:
                    return "Интеллектуальная адаптация: автоматический выбор оптимальной стратегии на основе анализа среды.";
                
                default:
                    return "Неизвестный режим.";
            }
        }
    }
}
using System;
using UnityEngine;
using DeftHack.Security;

namespace DeftHack.Components.UI.Modern
{
    /// <summary>
    /// Современная GUI система для DeftHack
    /// </summary>
    public static class ModernGUI
    {
        private static bool _isInitialized = false;
        private static GUIStyle _modernButtonStyle;
        private static GUIStyle _modernBoxStyle;
        private static GUIStyle _modernLabelStyle;
        private static GUIStyle _headerStyle;
        private static Texture2D _backgroundTexture;
        private static Texture2D _buttonTexture;

        /// <summary>
        /// Инициализация современного GUI
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                CreateTextures();
                CreateStyles();
                _isInitialized = true;
                Debug.Log("[DeftHack Modern GUI] Современный интерфейс инициализирован");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Modern GUI] Ошибка инициализации: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Создание текстур
        /// </summary>
        private static void CreateTextures()
        {
            // Создаем темную текстуру для фона
            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.9f));
            _backgroundTexture.Apply();

            // Создаем текстуру для кнопок
            _buttonTexture = new Texture2D(1, 1);
            _buttonTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 0.8f));
            _buttonTexture.Apply();
        }

        /// <summary>
        /// Создание стилей
        /// </summary>
        private static void CreateStyles()
        {
            // Современный стиль кнопок
            _modernButtonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = _buttonTexture, textColor = Color.white },
                hover = { background = _buttonTexture, textColor = Color.cyan },
                active = { background = _buttonTexture, textColor = Color.yellow },
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            // Современный стиль блоков
            _modernBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _backgroundTexture, textColor = Color.white },
                fontSize = 11,
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };

            // Современный стиль меток
            _modernLabelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.white },
                fontSize = 11,
                fontStyle = FontStyle.Normal
            };

            // Стиль заголовков
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.cyan },
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        /// <summary>
        /// Современная кнопка
        /// </summary>
        public static bool ModernButton(string text, params GUILayoutOption[] options)
        {
            if (!_isInitialized) Initialize();
            return GUILayout.Button(text, _modernButtonStyle, options);
        }

        /// <summary>
        /// Современная кнопка с цветом
        /// </summary>
        public static bool ModernButton(string text, Color color, params GUILayoutOption[] options)
        {
            if (!_isInitialized) Initialize();
            
            Color oldColor = GUI.color;
            GUI.color = color;
            bool result = GUILayout.Button(text, _modernButtonStyle, options);
            GUI.color = oldColor;
            
            return result;
        }

        /// <summary>
        /// Современный блок
        /// </summary>
        public static void ModernBox(System.Action content)
        {
            if (!_isInitialized) Initialize();
            
            GUILayout.BeginVertical(_modernBoxStyle);
            content?.Invoke();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Современная метка
        /// </summary>
        public static void ModernLabel(string text, params GUILayoutOption[] options)
        {
            if (!_isInitialized) Initialize();
            GUILayout.Label(text, _modernLabelStyle, options);
        }

        /// <summary>
        /// Заголовок
        /// </summary>
        public static void Header(string text, params GUILayoutOption[] options)
        {
            if (!_isInitialized) Initialize();
            GUILayout.Label(text, _headerStyle, options);
        }

        /// <summary>
        /// Статусная метка с цветом
        /// </summary>
        public static void StatusLabel(string text, Color color, params GUILayoutOption[] options)
        {
            if (!_isInitialized) Initialize();
            
            Color oldColor = GUI.color;
            GUI.color = color;
            GUILayout.Label(text, _modernLabelStyle, options);
            GUI.color = oldColor;
        }

        /// <summary>
        /// Прогресс-бар
        /// </summary>
        public static void ProgressBar(float value, Color color, string label = "", params GUILayoutOption[] options)
        {
            if (!_isInitialized) Initialize();

            Rect rect = GUILayoutUtility.GetRect(200, 20, options);
            
            // Фон
            GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            
            // Заполнение
            GUI.color = color;
            Rect fillRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value), rect.height);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            
            // Текст
            if (!string.IsNullOrEmpty(label))
            {
                GUI.color = Color.white;
                GUI.Label(rect, label, _modernLabelStyle);
            }
            
            GUI.color = Color.white;
        }

        /// <summary>
        /// Разделитель
        /// </summary>
        public static void Separator()
        {
            GUILayout.Space(5);
            Rect rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUILayout.Space(5);
        }

        /// <summary>
        /// Информационная панель
        /// </summary>
        public static void InfoPanel(string title, System.Action content)
        {
            if (!_isInitialized) Initialize();

            ModernBox(() =>
            {
                Header(title);
                Separator();
                content?.Invoke();
            });
        }

        /// <summary>
        /// Панель управления
        /// </summary>
        public static void ControlPanel(string title, System.Action content)
        {
            if (!_isInitialized) Initialize();

            GUILayout.BeginVertical(_modernBoxStyle);
            
            GUILayout.BeginHorizontal();
            Header(string.Format("🎮 {0}", title));
            GUILayout.EndHorizontal();
            
            Separator();
            content?.Invoke();
            
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Панель безопасности
        /// </summary>
        public static void SecurityPanel()
        {
            if (!_isInitialized) Initialize();

            InfoPanel("🛡️ Система Безопасности", () =>
            {
                // Статус
                GUILayout.BeginHorizontal();
                ModernLabel("Статус:");
                if (SecurityManager.IsActive)
                {
                    StatusLabel("✅ Активна", Color.green);
                }
                else
                {
                    StatusLabel("❌ Неактивна", Color.red);
                }
                GUILayout.EndHorizontal();

                // Режим
                if (SecurityManager.IsActive)
                {
                    GUILayout.BeginHorizontal();
                    ModernLabel("Режим:");
                    StatusLabel(SecurityManager.CurrentMode.ToString(), Color.cyan);
                    GUILayout.EndHorizontal();

                    // Уровень угрозы
                    GUILayout.BeginHorizontal();
                    ModernLabel("Угрозы:");
                    Color threatColor = GetThreatColor(SecurityManager.CurrentThreatLevel);
                    StatusLabel(SecurityManager.CurrentThreatLevel.ToString(), threatColor);
                    GUILayout.EndHorizontal();
                }

                // Интеллектуальная адаптация
                if (IntelligentAdaptation.IsActive)
                {
                    Separator();
                    ModernLabel("🧠 Интеллектуальная Адаптация:");
                    
                    GUILayout.BeginHorizontal();
                    ModernLabel("Стратегия:");
                    StatusLabel(IntelligentAdaptation.CurrentStrategy.ToString(), Color.yellow);
                    GUILayout.EndHorizontal();

                    // Метрики
                    ProgressBar(IntelligentAdaptation.OverallEffectiveness, Color.green, 
                        string.Format("Эффективность: {0:P0}", IntelligentAdaptation.OverallEffectiveness));
                    
                    ProgressBar(IntelligentAdaptation.DetectionRiskLevel, Color.red, 
                        string.Format("Риск: {0:P0}", IntelligentAdaptation.DetectionRiskLevel));
                }
            });
        }

        /// <summary>
        /// Панель компонентов
        /// </summary>
        public static void ComponentsPanel()
        {
            if (!_isInitialized) Initialize();

            InfoPanel("🔧 Компоненты", () =>
            {
                DrawComponentStatus("🔒 ModernBypass", ModernBypass.IsActive);
                DrawComponentStatus("📸 ScreenshotBypass", AdvancedScreenshotBypass.IsActive);
                DrawComponentStatus("🛡️ ThreatDetection", AdvancedThreatDetection.IsActive);
                DrawComponentStatus("💻 HyperVBypass", HyperVBypass.IsActive);
                DrawComponentStatus("⚙️ KernelBypass", KernelBypass.IsActive);
                
                if (StealthHypervisor.IsActive)
                {
                    DrawComponentStatus("🔮 StealthHypervisor", true);
                }
                else
                {
                    DrawComponentStatus("🔮 HypervisorBypass", HypervisorBypass.IsActive);
                }
                
                DrawComponentStatus("👁️ ExternalAnalysis", ExternalAnalysis.IsActive);
            });
        }

        /// <summary>
        /// Отрисовка статуса компонента
        /// </summary>
        private static void DrawComponentStatus(string name, bool isActive)
        {
            GUILayout.BeginHorizontal();
            StatusLabel(isActive ? "✅" : "❌", isActive ? Color.green : Color.red, GUILayout.Width(20));
            ModernLabel(name);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Получение цвета уровня угрозы
        /// </summary>
        private static Color GetThreatColor(SecurityManager.ThreatLevel level)
        {
            switch (level)
            {
                case SecurityManager.ThreatLevel.None: return Color.green;
                case SecurityManager.ThreatLevel.Low: return Color.yellow;
                case SecurityManager.ThreatLevel.Medium: return new Color(1f, 0.5f, 0f); // Orange
                case SecurityManager.ThreatLevel.High: return Color.red;
                case SecurityManager.ThreatLevel.Critical: return Color.magenta;
                default: return Color.white;
            }
        }

        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        public static void Cleanup()
        {
            try
            {
                if (_backgroundTexture != null)
                {
                    UnityEngine.Object.Destroy(_backgroundTexture);
                    _backgroundTexture = null;
                }

                if (_buttonTexture != null)
                {
                    UnityEngine.Object.Destroy(_buttonTexture);
                    _buttonTexture = null;
                }

                _isInitialized = false;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack Modern GUI] Ошибка очистки: {0}", ex.Message));
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DeftHack.Security
{
    public static class AdvancedScreenshotBypass
    {
        private static bool _isInitialized = false;
        private static bool _screenshotInProgress = false;
        private static List<Texture2D> _decoyScreenshots = new List<Texture2D>();

        public static bool IsActive => _isInitialized;

        public static void Initialize()
        {
            if (_isInitialized)
                return;

            LoadDecoyScreenshots();
            _isInitialized = true;
            Debug.Log("[DeftHack Screenshot] Продвинутый обход скриншотов инициализирован.");
        }

        private static void LoadDecoyScreenshots()
        {
            string decoyPath = Path.Combine(Directory.GetCurrentDirectory(), "decoy_screenshots");

            if (!Directory.Exists(decoyPath))
            {
                Directory.CreateDirectory(decoyPath);
                Debug.LogWarning("[DeftHack Screenshot] Папка для ложных скриншотов не найдена, создана новая. Поместите в нее .jpg файлы.");
                return;
            }

            string[] decoyFiles = Directory.GetFiles(decoyPath, "*.jpg");
            foreach (string file in decoyFiles)
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(file);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(fileData))
                    {
                        _decoyScreenshots.Add(tex);
                        Debug.Log($"[DeftHack Screenshot] Загружен ложный скриншот: {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DeftHack Screenshot] Не удалось загрузить ложный скриншот {file}: {ex.Message}");
                }
            }
        }

        public static IEnumerator HandleScreenshotRequest()
        {
            if (_screenshotInProgress)
                yield break;

            _screenshotInProgress = true;
            Debug.Log("[DeftHack Screenshot] Обработка запроса скриншота.");

            IEnumerator routine = HandleMethodDecoyImage();
            
            while (true)
            {
                bool moveNext;
                try
                {
                    moveNext = routine.MoveNext();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DeftHack Screenshot] Ошибка выполнения корутины: {ex.Message}");
                    break;
                }

                if (moveNext)
                {
                    yield return routine.Current;
                }
                else
                {
                    break;
                }
            }

            _screenshotInProgress = false;
            Debug.Log("[DeftHack Screenshot] Обработка запроса скриншота завершена.");
        }

        private static IEnumerator HandleMethodDecoyImage()
        {
            if (_decoyScreenshots.Count == 0)
            {
                Debug.LogWarning("[DeftHack Screenshot] Нет ложных скриншотов для отправки.");
                yield break;
            }

            int randomIndex = UnityEngine.Random.Range(0, _decoyScreenshots.Count);
            Texture2D decoyTexture = _decoyScreenshots[randomIndex];

            if (decoyTexture == null)
            {
                yield break;
            }

            byte[] imageData = null;
            try
            {
                imageData = decoyTexture.EncodeToJPG(75);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeftHack Screenshot] Ошибка кодирования ложного изображения: {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.1f);

            Debug.Log($"[DeftHack Screenshot] Возвращен ложный скриншот ({imageData.Length} байт)");
        }

        /// <summary>
        /// Остановка системы обхода скриншотов
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                _isInitialized = false;
                _screenshotInProgress = false;
                
                // Очистка ложных скриншотов
                foreach (var tex in _decoyScreenshots)
                {
                    if (tex != null)
                    {
                        UnityEngine.Object.Destroy(tex);
                    }
                }
                _decoyScreenshots.Clear();
                
                Debug.Log("[DeftHack Screenshot] Система обхода скриншотов остановлена");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeftHack Screenshot] Ошибка остановки: {ex.Message}");
            }
        }
    }
}

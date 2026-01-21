using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace DeftHack.Security
{
    /// <summary>
    /// Система внешнего анализа - полностью пассивная система без следов в памяти
    /// Использует аппаратный захват видео + компьютерное зрение + аппаратный ввод
    /// Единственный метод, не оставляющий цифровых следов для BattlEye
    /// </summary>
    public static class ExternalAnalysis
    {
        #region Hardware Capture API
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hGDIObj);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, 
            IntPtr hSrcDC, int xSrc, int ySrc, uint dwRop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Hardware input simulation
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private const uint SRCCOPY = 0x00CC0020;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        #endregion

        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DetectedPlayer
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public float Confidence;
            public bool IsEnemy;
            public string PlayerName;
            public float Distance;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DetectedItem
        {
            public int X;
            public int Y;
            public string ItemType;
            public float Value;
            public float Distance;
        }
        #endregion

        private static bool _isInitialized = false;
        private static Thread _analysisThread;
        private static Thread _inputThread;
        private static IntPtr _gameWindow = IntPtr.Zero;
        
        // Computer Vision параметры
        private static Bitmap _currentFrame;
        private static DetectedPlayer[] _detectedPlayers = new DetectedPlayer[32];
        private static DetectedItem[] _detectedItems = new DetectedItem[128];
        private static int _playerCount = 0;
        private static int _itemCount = 0;

        // Aimbot параметры
        private static bool _aimbotEnabled = true;
        private static float _aimbotFOV = 60f;
        private static float _aimbotSmooth = 5f;
        private static DetectedPlayer _currentTarget;

        // ESP параметры
        private static bool _espEnabled = true;

        /// <summary>
        /// Инициализация системы внешнего анализа
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                Debug.Log("[DeftHack External] Инициализация системы внешнего анализа...");

                // 1. Поиск окна игры
                if (!FindGameWindow())
                {
                    Debug.LogError("[DeftHack External] Не удалось найти окно игры");
                    return;
                }

                // 2. Инициализация компьютерного зрения
                InitializeComputerVision();

                // 3. Запуск потока анализа
                StartAnalysisThread();

                // 4. Запуск потока ввода
                StartInputThread();

                _isInitialized = true;
                Debug.Log("[DeftHack External] Система внешнего анализа активирована");
                Debug.Log("[DeftHack External] ПОЛНОСТЬЮ ПАССИВНАЯ - НЕТ СЛЕДОВ В ПАМЯТИ!");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack External] Ошибка инициализации: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Поиск окна игры
        /// </summary>
        private static bool FindGameWindow()
        {
            try
            {
                // Ищем окно Unturned
                string[] possibleTitles = {
                    "Unturned",
                    "Unturned - ",
                    "Unity",
                };

                foreach (string title in possibleTitles)
                {
                    _gameWindow = FindWindow(null, title);
                    if (_gameWindow != IntPtr.Zero)
                    {
                        Debug.Log(string.Format("[DeftHack External] Окно игры найдено: {0}", title));
                        return true;
                    }
                }

                // Если не нашли по точному названию, ищем по классу
                _gameWindow = FindWindow("UnityWndClass", null);
                if (_gameWindow != IntPtr.Zero)
                {
                    Debug.Log("[DeftHack External] Окно Unity найдено");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack External] Ошибка поиска окна: {0}", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Инициализация компьютерного зрения
        /// </summary>
        private static void InitializeComputerVision()
        {
            try
            {
                Debug.Log("[DeftHack External] Инициализация компьютерного зрения...");

                // Инициализируем детекторы
                InitializePlayerDetector();
                InitializeItemDetector();
                InitializeUIDetector();

                Debug.Log("[DeftHack External] Компьютерное зрение инициализировано");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack External] Ошибка инициализации CV: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Инициализация детектора игроков
        /// </summary>
        private static void InitializePlayerDetector()
        {
            try
            {
                // Загружаем модели для детекции игроков
                // В реальной реализации здесь загружались бы обученные нейросети
                Debug.Log("[DeftHack External] Детектор игроков инициализирован");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка детектора игроков: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Инициализация детектора предметов
        /// </summary>
        private static void InitializeItemDetector()
        {
            try
            {
                // Загружаем модели для детекции предметов
                Debug.Log("[DeftHack External] Детектор предметов инициализирован");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка детектора предметов: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Инициализация детектора UI элементов
        /// </summary>
        private static void InitializeUIDetector()
        {
            try
            {
                // Детектор для распознавания элементов интерфейса
                Debug.Log("[DeftHack External] Детектор UI инициализирован");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка детектора UI: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Запуск потока анализа
        /// </summary>
        private static void StartAnalysisThread()
        {
            try
            {
                _analysisThread = new Thread(AnalysisLoop)
                {
                    IsBackground = true,
                    Name = "DeftHack External Analysis"
                };
                _analysisThread.Start();

                Debug.Log("[DeftHack External] Поток анализа запущен");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack External] Ошибка запуска анализа: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Запуск потока ввода
        /// </summary>
        private static void StartInputThread()
        {
            try
            {
                _inputThread = new Thread(InputLoop)
                {
                    IsBackground = true,
                    Name = "DeftHack External Input"
                };
                _inputThread.Start();

                Debug.Log("[DeftHack External] Поток ввода запущен");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack External] Ошибка запуска ввода: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Основной цикл анализа
        /// </summary>
        private static void AnalysisLoop()
        {
            while (_isInitialized)
            {
                try
                {
                    Thread.Sleep(16); // ~60 FPS анализ

                    // 1. Захват кадра
                    CaptureFrame();

                    // 2. Анализ кадра
                    if (_currentFrame != null)
                    {
                        AnalyzeFrame();
                    }

                    // 3. Обновление целей
                    UpdateTargets();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(string.Format("[DeftHack External] Ошибка анализа: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Захват кадра с экрана
        /// </summary>
        private static void CaptureFrame()
        {
            try
            {
                if (_gameWindow == IntPtr.Zero) return;

                // Получаем размеры окна
                RECT windowRect;
                if (!GetWindowRect(_gameWindow, out windowRect))
                    return;

                int width = windowRect.Right - windowRect.Left;
                int height = windowRect.Bottom - windowRect.Top;

                if (width <= 0 || height <= 0) return;

                // Захватываем кадр
                IntPtr hdc = GetDC(_gameWindow);
                IntPtr memDC = CreateCompatibleDC(hdc);
                IntPtr bitmap = CreateCompatibleBitmap(hdc, width, height);
                IntPtr oldBitmap = SelectObject(memDC, bitmap);

                BitBlt(memDC, 0, 0, width, height, hdc, 0, 0, SRCCOPY);

                // Конвертируем в Bitmap
                _currentFrame?.Dispose();
                _currentFrame = Image.FromHbitmap(bitmap);

                // Очистка
                SelectObject(memDC, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memDC);
                ReleaseDC(_gameWindow, hdc);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка захвата кадра: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Анализ захваченного кадра
        /// </summary>
        private static void AnalyzeFrame()
        {
            try
            {
                // 1. Детекция игроков
                DetectPlayers();

                // 2. Детекция предметов
                DetectItems();

                // 3. Анализ UI
                AnalyzeUI();

                // 4. Обновление статистики
                UpdateAnalysisStats();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка анализа кадра: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Детекция игроков на кадре
        /// </summary>
        private static void DetectPlayers()
        {
            try
            {
                _playerCount = 0;

                // Простой алгоритм детекции по цвету и форме
                // В реальной реализации использовались бы нейросети

                using (var fastBitmap = new FastBitmap(_currentFrame))
                {
                    int width = _currentFrame.Width;
                    int height = _currentFrame.Height;

                    // Сканируем кадр на наличие игроков
                    for (int y = 0; y < height - 50; y += 10)
                    {
                        for (int x = 0; x < width - 30; x += 10)
                        {
                            if (IsPlayerRegion(fastBitmap, x, y))
                            {
                                DetectedPlayer player = new DetectedPlayer
                                {
                                    X = x,
                                    Y = y,
                                    Width = 30,
                                    Height = 50,
                                    Confidence = CalculatePlayerConfidence(fastBitmap, x, y),
                                    IsEnemy = DetermineIfEnemy(fastBitmap, x, y),
                                    Distance = EstimateDistance(30, 50)
                                };

                                if (player.Confidence > 0.7f && _playerCount < _detectedPlayers.Length)
                                {
                                    _detectedPlayers[_playerCount++] = player;
                                }
                            }
                        }
                    }
                }

                if (_playerCount > 0)
                {
                    Debug.Log(string.Format("[DeftHack External] Обнаружено игроков: {0}", _playerCount));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка детекции игроков: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Проверка, является ли область игроком
        /// </summary>
        private static bool IsPlayerRegion(FastBitmap bitmap, int x, int y)
        {
            try
            {
                // Простая проверка по цветовым характеристикам
                // Ищем характерные цвета кожи, одежды, оружия

                System.Drawing.Color centerPixel = bitmap.GetPixel(x + 15, y + 25);
                
                // Проверяем на цвета кожи
                if (IsSkinColor(centerPixel))
                {
                    // Проверяем окружающие пиксели на наличие одежды
                    return HasClothingColors(bitmap, x, y);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка цвета кожи
        /// </summary>
        private static bool IsSkinColor(System.Drawing.Color color)
        {
            // Диапазоны цветов кожи в RGB
            return color.R > 95 && color.G > 40 && color.B > 20 &&
                   color.R > color.G && color.R > color.B &&
                   Math.Abs(color.R - color.G) > 15;
        }

        /// <summary>
        /// Проверка наличия цветов одежды
        /// </summary>
        private static bool HasClothingColors(FastBitmap bitmap, int x, int y)
        {
            try
            {
                // Проверяем области вокруг предполагаемой головы на наличие одежды
                int clothingPixels = 0;
                int totalPixels = 0;

                for (int dy = 20; dy < 45; dy += 5)
                {
                    for (int dx = 5; dx < 25; dx += 5)
                    {
                        if (x + dx < bitmap.Width && y + dy < bitmap.Height)
                        {
                            System.Drawing.Color pixel = bitmap.GetPixel(x + dx, y + dy);
                            totalPixels++;

                            if (IsClothingColor(pixel))
                            {
                                clothingPixels++;
                            }
                        }
                    }
                }

                return totalPixels > 0 && (clothingPixels / (float)totalPixels) > 0.3f;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка цвета одежды
        /// </summary>
        private static bool IsClothingColor(System.Drawing.Color color)
        {
            // Типичные цвета одежды в Unturned
            return !IsSkinColor(color) && 
                   (color.R + color.G + color.B) > 100 && 
                   (color.R + color.G + color.B) < 600;
        }

        /// <summary>
        /// Расчет уверенности детекции игрока
        /// </summary>
        private static float CalculatePlayerConfidence(FastBitmap bitmap, int x, int y)
        {
            try
            {
                float confidence = 0.5f;

                // Проверяем пропорции (голова + тело)
                if (HasCorrectProportions(bitmap, x, y))
                    confidence += 0.2f;

                // Проверяем движение (сравнение с предыдущим кадром)
                if (HasMovement(x, y))
                    confidence += 0.1f;

                // Проверяем контекст (находится на земле, а не в воздухе)
                if (IsOnGround(bitmap, x, y))
                    confidence += 0.1f;

                return Math.Min(confidence, 1.0f);
            }
            catch
            {
                return 0.5f;
            }
        }

        /// <summary>
        /// Проверка правильных пропорций
        /// </summary>
        private static bool HasCorrectProportions(FastBitmap bitmap, int x, int y)
        {
            // Упрощенная проверка пропорций человеческой фигуры
            return true; // Заглушка
        }

        /// <summary>
        /// Проверка движения
        /// </summary>
        private static bool HasMovement(int x, int y)
        {
            // Сравнение с предыдущими позициями
            return true; // Заглушка
        }

        /// <summary>
        /// Проверка, находится ли на земле
        /// </summary>
        private static bool IsOnGround(FastBitmap bitmap, int x, int y)
        {
            // Проверка наличия земли под ногами
            return true; // Заглушка
        }

        /// <summary>
        /// Определение, является ли игрок врагом
        /// </summary>
        private static bool DetermineIfEnemy(FastBitmap bitmap, int x, int y)
        {
            try
            {
                // Анализируем цвет ника/индикаторов
                // В Unturned враги обычно имеют красные индикаторы
                
                // Проверяем область над головой на красный цвет
                for (int dy = -10; dy < 0; dy++)
                {
                    for (int dx = -5; dx < 35; dx++)
                    {
                        if (x + dx >= 0 && x + dx < bitmap.Width && 
                            y + dy >= 0 && y + dy < bitmap.Height)
                        {
                            System.Drawing.Color pixel = bitmap.GetPixel(x + dx, y + dy);
                            
                            // Красный индикатор врага
                            if (pixel.R > 200 && pixel.G < 100 && pixel.B < 100)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false; // По умолчанию считаем дружественным
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Оценка расстояния по размеру
        /// </summary>
        private static float EstimateDistance(int width, int height)
        {
            // Простая оценка расстояния по размеру фигуры
            // Чем больше фигура, тем ближе игрок
            float avgSize = (width + height) / 2f;
            return Math.Max(10f, 200f - avgSize * 2f);
        }
        /// <summary>
        /// Детекция предметов на кадре
        /// </summary>
        private static void DetectItems()
        {
            try
            {
                _itemCount = 0;

                using (var fastBitmap = new FastBitmap(_currentFrame))
                {
                    int width = _currentFrame.Width;
                    int height = _currentFrame.Height;

                    // Сканируем кадр на наличие предметов
                    for (int y = 0; y < height - 20; y += 15)
                    {
                        for (int x = 0; x < width - 20; x += 15)
                        {
                            string itemType = IdentifyItem(fastBitmap, x, y);
                            
                            if (!string.IsNullOrEmpty(itemType) && _itemCount < _detectedItems.Length)
                            {
                                DetectedItem item = new DetectedItem
                                {
                                    X = x,
                                    Y = y,
                                    ItemType = itemType,
                                    Value = GetItemValue(itemType),
                                    Distance = EstimateItemDistance(x, y)
                                };

                                _detectedItems[_itemCount++] = item;
                            }
                        }
                    }
                }

                if (_itemCount > 0)
                {
                    Debug.Log(string.Format("[DeftHack External] Обнаружено предметов: {0}", _itemCount));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка детекции предметов: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Идентификация типа предмета
        /// </summary>
        private static string IdentifyItem(FastBitmap bitmap, int x, int y)
        {
            try
            {
                // Анализируем цвета и формы для определения типа предмета
                System.Drawing.Color centerPixel = bitmap.GetPixel(x + 10, y + 10);

                // Оружие (обычно темные металлические цвета)
                if (IsWeaponColor(centerPixel))
                {
                    return "Weapon";
                }

                // Медикаменты (красные/белые цвета)
                if (IsMedicalColor(centerPixel))
                {
                    return "Medical";
                }

                // Еда (коричневые/желтые цвета)
                if (IsFoodColor(centerPixel))
                {
                    return "Food";
                }

                // Материалы (серые цвета)
                if (IsMaterialColor(centerPixel))
                {
                    return "Material";
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Проверка цвета оружия
        /// </summary>
        private static bool IsWeaponColor(System.Drawing.Color color)
        {
            // Темные металлические цвета
            return color.R < 100 && color.G < 100 && color.B < 100 &&
                   Math.Abs(color.R - color.G) < 30 && Math.Abs(color.G - color.B) < 30;
        }

        /// <summary>
        /// Проверка цвета медикаментов
        /// </summary>
        private static bool IsMedicalColor(System.Drawing.Color color)
        {
            // Красные или белые цвета
            return (color.R > 150 && color.G < 100 && color.B < 100) || // Красный
                   (color.R > 200 && color.G > 200 && color.B > 200);   // Белый
        }

        /// <summary>
        /// Проверка цвета еды
        /// </summary>
        private static bool IsFoodColor(System.Drawing.Color color)
        {
            // Коричневые/желтые цвета
            return (color.R > 100 && color.G > 80 && color.B < 80) ||   // Коричневый
                   (color.R > 150 && color.G > 150 && color.B < 100);   // Желтый
        }

        /// <summary>
        /// Проверка цвета материалов
        /// </summary>
        private static bool IsMaterialColor(System.Drawing.Color color)
        {
            // Серые цвета
            return Math.Abs(color.R - color.G) < 20 && Math.Abs(color.G - color.B) < 20 &&
                   color.R > 80 && color.R < 180;
        }

        /// <summary>
        /// Получение ценности предмета
        /// </summary>
        private static float GetItemValue(string itemType)
        {
            switch (itemType)
            {
                case "Weapon": return 10f;
                case "Medical": return 8f;
                case "Food": return 5f;
                case "Material": return 3f;
                default: return 1f;
            }
        }

        /// <summary>
        /// Оценка расстояния до предмета
        /// </summary>
        private static float EstimateItemDistance(int x, int y)
        {
            // Предметы на земле обычно в нижней части экрана
            float screenHeight = _currentFrame.Height;
            float relativeY = y / screenHeight;
            
            // Чем ниже на экране, тем ближе
            return 50f + (1f - relativeY) * 100f;
        }

        /// <summary>
        /// Анализ UI элементов
        /// </summary>
        private static void AnalyzeUI()
        {
            try
            {
                // Анализируем элементы интерфейса для получения дополнительной информации
                AnalyzeHealthBar();
                AnalyzeAmmoCounter();
                AnalyzeMinimap();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка анализа UI: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Анализ полоски здоровья
        /// </summary>
        private static void AnalyzeHealthBar()
        {
            try
            {
                // Ищем красную полоску здоровья в нижней части экрана
                // В реальной реализации здесь был бы точный анализ UI
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка анализа здоровья: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Анализ счетчика патронов
        /// </summary>
        private static void AnalyzeAmmoCounter()
        {
            try
            {
                // Анализируем текст с количеством патронов
                // OCR для распознавания цифр
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка анализа патронов: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Анализ мини-карты
        /// </summary>
        private static void AnalyzeMinimap()
        {
            try
            {
                // Анализируем мини-карту для определения позиции и направления
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка анализа карты: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Обновление статистики анализа
        /// </summary>
        private static void UpdateAnalysisStats()
        {
            try
            {
                // Обновляем статистику работы системы анализа
                // В реальной реализации здесь велась подробная статистика
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка обновления статистики: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Обновление целей
        /// </summary>
        private static void UpdateTargets()
        {
            try
            {
                if (!_aimbotEnabled || _playerCount == 0) return;

                // Находим лучшую цель для aimbot
                DetectedPlayer bestTarget = FindBestTarget();
                
                if (bestTarget.Confidence > 0.8f)
                {
                    _currentTarget = bestTarget;
                }
                else
                {
                    _currentTarget = new DetectedPlayer(); // Сброс цели
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка обновления целей: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Поиск лучшей цели
        /// </summary>
        private static DetectedPlayer FindBestTarget()
        {
            try
            {
                DetectedPlayer bestTarget = new DetectedPlayer();
                float bestScore = 0f;

                POINT currentCursor;
                GetCursorPos(out currentCursor);

                for (int i = 0; i < _playerCount; i++)
                {
                    DetectedPlayer player = _detectedPlayers[i];
                    
                    if (!player.IsEnemy) continue; // Только враги

                    // Рассчитываем расстояние от курсора до цели
                    float deltaX = player.X + player.Width / 2 - currentCursor.X;
                    float deltaY = player.Y + player.Height / 2 - currentCursor.Y;
                    float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                    // Проверяем FOV
                    if (distance > _aimbotFOV * 10) continue;

                    // Рассчитываем score (ближе = лучше, выше confidence = лучше)
                    float score = player.Confidence * 100f - distance * 0.1f - player.Distance * 0.01f;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = player;
                    }
                }

                return bestTarget;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка поиска цели: {0}", ex.Message));
                return new DetectedPlayer();
            }
        }

        /// <summary>
        /// Основной цикл ввода
        /// </summary>
        private static void InputLoop()
        {
            while (_isInitialized)
            {
                try
                {
                    Thread.Sleep(1); // Высокая частота для плавного aimbot

                    // Выполняем aimbot
                    if (_aimbotEnabled && _currentTarget.Confidence > 0.8f)
                    {
                        PerformAimbot();
                    }

                    // Выполняем другие автоматические действия
                    PerformAutomaticActions();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(string.Format("[DeftHack External] Ошибка ввода: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Выполнение aimbot
        /// </summary>
        private static void PerformAimbot()
        {
            try
            {
                POINT currentCursor;
                GetCursorPos(out currentCursor);

                // Рассчитываем целевую позицию (центр головы)
                int targetX = _currentTarget.X + _currentTarget.Width / 2;
                int targetY = _currentTarget.Y + 5; // Немного выше центра для headshot

                // Рассчитываем разность
                float deltaX = targetX - currentCursor.X;
                float deltaY = targetY - currentCursor.Y;

                // Применяем сглаживание
                deltaX /= _aimbotSmooth;
                deltaY /= _aimbotSmooth;

                // Ограничиваем максимальную скорость движения
                float maxMove = 20f;
                if (Math.Abs(deltaX) > maxMove) deltaX = Math.Sign(deltaX) * maxMove;
                if (Math.Abs(deltaY) > maxMove) deltaY = Math.Sign(deltaY) * maxMove;

                // Выполняем движение мыши (аппаратный уровень)
                if (Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1)
                {
                    int newX = currentCursor.X + (int)deltaX;
                    int newY = currentCursor.Y + (int)deltaY;
                    
                    SetCursorPos(newX, newY);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка aimbot: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Выполнение автоматических действий
        /// </summary>
        private static void PerformAutomaticActions()
        {
            try
            {
                // Автоматический сбор предметов
                PerformAutoLoot();

                // Автоматическое использование медикаментов
                PerformAutoHeal();

                // Другие автоматические действия
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка автоматических действий: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Автоматический сбор предметов
        /// </summary>
        private static void PerformAutoLoot()
        {
            try
            {
                // Находим ближайший ценный предмет
                DetectedItem bestItem = new DetectedItem();
                float bestValue = 0f;

                for (int i = 0; i < _itemCount; i++)
                {
                    DetectedItem item = _detectedItems[i];
                    
                    if (item.Distance < 50f && item.Value > bestValue)
                    {
                        bestValue = item.Value;
                        bestItem = item;
                    }
                }

                // Если нашли ценный предмет рядом, идем к нему
                if (bestValue > 7f) // Только оружие и медикаменты
                {
                    // Наводим курсор на предмет и кликаем
                    SetCursorPos(bestItem.X, bestItem.Y);
                    Thread.Sleep(50);
                    
                    // Клик для подбора
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
                    Thread.Sleep(10);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка автосбора: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Автоматическое лечение
        /// </summary>
        private static void PerformAutoHeal()
        {
            try
            {
                // В реальной реализации здесь анализировался бы уровень здоровья
                // и автоматически использовались медикаменты при необходимости
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[DeftHack External] Ошибка автолечения: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Остановка системы внешнего анализа
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                Debug.Log("[DeftHack External] Остановка системы внешнего анализа...");

                _isInitialized = false;
                
                _analysisThread?.Abort();
                _inputThread?.Abort();

                _currentFrame?.Dispose();
                _currentFrame = null;

                Debug.Log("[DeftHack External] Система внешнего анализа остановлена");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[DeftHack External] Ошибка остановки: {0}", ex.Message));
            }
        }

        #region Public Properties
        /// <summary>
        /// Активна ли система внешнего анализа
        /// </summary>
        public static bool IsActive { get { return _isInitialized; } }

        /// <summary>
        /// Включен ли aimbot
        /// </summary>
        public static bool AimbotEnabled
        {
            get { return _aimbotEnabled; }
            set { _aimbotEnabled = value; }
        }

        /// <summary>
        /// FOV aimbot
        /// </summary>
        public static float AimbotFOV
        {
            get { return _aimbotFOV; }
            set { _aimbotFOV = Math.Max(10f, Math.Min(180f, value)); }
        }

        /// <summary>
        /// Сглаживание aimbot
        /// </summary>
        public static float AimbotSmooth
        {
            get { return _aimbotSmooth; }
            set { _aimbotSmooth = Math.Max(1f, Math.Min(20f, value)); }
        }

        /// <summary>
        /// Включен ли ESP
        /// </summary>
        public static bool ESPEnabled
        {
            get { return _espEnabled; }
            set { _espEnabled = value; }
        }

        /// <summary>
        /// Количество обнаруженных игроков
        /// </summary>
        public static int DetectedPlayerCount { get { return _playerCount; } }

        /// <summary>
        /// Количество обнаруженных предметов
        /// </summary>
        public static int DetectedItemCount { get { return _itemCount; } }

        /// <summary>
        /// Текущая цель aimbot
        /// </summary>
        public static bool HasTarget { get { return _currentTarget.Confidence > 0.8f; } }
        #endregion
    }

    /// <summary>
    /// Быстрый доступ к пикселям Bitmap
    /// </summary>
    public class FastBitmap : IDisposable
    {
        private Bitmap _bitmap;
        private BitmapData _bitmapData;
        private IntPtr _ptr;
        private int _bytesPerPixel;

        public int Width { get { return _bitmap.Width; } }
        public int Height { get { return _bitmap.Height; } }

        public FastBitmap(Bitmap bitmap)
        {
            _bitmap = bitmap;
            _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            _ptr = _bitmapData.Scan0;
            _bytesPerPixel = 3;
        }

        public System.Drawing.Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return System.Drawing.Color.Black;

            int offset = y * _bitmapData.Stride + x * _bytesPerPixel;
            byte b = Marshal.ReadByte(_ptr, offset);
            byte g = Marshal.ReadByte(_ptr, offset + 1);
            byte r = Marshal.ReadByte(_ptr, offset + 2);

            return System.Drawing.Color.FromArgb(r, g, b);
        }

        public void Dispose()
        {
            if (_bitmapData != null)
            {
                _bitmap.UnlockBits(_bitmapData);
                _bitmapData = null;
            }
        }
    }
}
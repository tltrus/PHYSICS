using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DrawingVisualApp.Models;
using DrawingVisualApp.Services;

namespace DrawingVisualApp.ViewModels
{
    /// <summary>
    /// Основная ViewModel приложения
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // Компоненты симуляции
        private PhysicsEngine _physicsEngine;
        private RenderingService _renderingService;

        // Состояние симуляции
        private int _settleFrames = 10;  // Кадры до включения гравитации

        // Таймер для обновления
        private DispatcherTimer _renderTimer;

        // Данные симуляции
        public WriteableBitmap Bitmap { get; private set; }
        public List<VerletPoint> Points { get; } = new List<VerletPoint>();
        public List<Stick> Sticks { get; } = new List<Stick>();

        // Размеры области отрисовки
        public double CanvasWidth { get; set; }
        public double CanvasHeight { get; set; }

        // Платформа
        private Rect _platform;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
        }

        /// <summary>
        /// Инициализирует WriteableBitmap
        /// </summary>
        public void InitializeBitmap(int width, int height)
        {
            Bitmap = new WriteableBitmap(
                width,
                height,
                96, 96,
                PixelFormats.Pbgra32,
                null);
        }

        /// <summary>
        /// Инициализирует симуляцию
        /// </summary>
        public void Initialize()
        {
            if (Bitmap == null)
                throw new InvalidOperationException("Bitmap must be initialized first");

            // Создаем платформу
            _platform = CreatePlatform();

            // Инициализируем сетку точек
            InitializeGrid(_platform);

            // Создаем физический движок
            _physicsEngine = new PhysicsEngine(Points, Sticks, CanvasWidth, CanvasHeight)
            {
                Platform = _platform
            };

            // Создаем сервис отрисовки
            _renderingService = new RenderingService(Bitmap, Points, Sticks, _platform);

            // Инициализируем таймер для рендеринга
            InitializeRenderTimer();
        }

        /// <summary>
        /// Инициализирует таймер рендеринга
        /// </summary>
        private void InitializeRenderTimer()
        {
            _renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.0 / 60.0) // 60 FPS
            };
            _renderTimer.Tick += OnRenderTick;
        }

        /// <summary>
        /// Создает платформу для симуляции
        /// </summary>
        private Rect CreatePlatform()
        {
            return new Rect(
                150,                        // X позиция
                CanvasHeight * 0.6,         // Y позиция (60% высоты)
                CanvasWidth - 500,          // Ширина
                30                          // Высота
            );
        }

        /// <summary>
        /// Инициализирует сетку точек и связей
        /// </summary>
        private void InitializeGrid(Rect platform)
        {
            ClearSimulationData();

            // Жесткость связей
            const double horizontalVerticalStiffness = 0.65;
            const double diagonalStiffness = 0.5;

            // Позиция начала сетки
            double gridStartX = platform.X + platform.Width / 2 -
                               (Constants.GridSize * Constants.Spacing) / 2;
            double gridStartY = platform.Y - Constants.GridSize * Constants.Spacing * 1.1;

            // Создаем точки сетки
            CreateGridPoints(gridStartX, gridStartY);

            // Создаем связи между точками
            CreateGridSticks(horizontalVerticalStiffness, diagonalStiffness);
        }

        /// <summary>
        /// Очищает данные симуляции
        /// </summary>
        private void ClearSimulationData()
        {
            Points.Clear();
            Sticks.Clear();
        }

        /// <summary>
        /// Создает точки сетки
        /// </summary>
        private void CreateGridPoints(double startX, double startY)
        {
            for (int y = 0; y < Constants.GridSize; y++)
            {
                for (int x = 0; x < Constants.GridSize; x++)
                {
                    Points.Add(new VerletPoint(
                        new Point(
                            startX + x * Constants.Spacing,
                            startY + y * Constants.Spacing
                        )
                    ));
                }
            }
        }

        /// <summary>
        /// Создает связи между точками сетки
        /// </summary>
        private void CreateGridSticks(double hvStiffness, double diagStiffness)
        {
            for (int y = 0; y < Constants.GridSize; y++)
            {
                for (int x = 0; x < Constants.GridSize; x++)
                {
                    int currentIndex = y * Constants.GridSize + x;

                    // Горизонтальные связи
                    if (x < Constants.GridSize - 1)
                    {
                        Sticks.Add(new Stick(
                            Points[currentIndex],
                            Points[currentIndex + 1],
                            hvStiffness
                        ));
                    }

                    // Вертикальные связи
                    if (y < Constants.GridSize - 1)
                    {
                        Sticks.Add(new Stick(
                            Points[currentIndex],
                            Points[currentIndex + Constants.GridSize],
                            hvStiffness
                        ));
                    }
                    // Диагональные связи (право-вниз)
                    if (x < Constants.GridSize - 1 && y < Constants.GridSize - 1)
                    {
                        Sticks.Add(new Stick(
                            Points[currentIndex],
                            Points[currentIndex + Constants.GridSize + 1],
                            diagStiffness
                        ));
                    }

                    // Диагональные связи (лево-вниз)
                    if (x > 0 && y < Constants.GridSize - 1)
                    {
                        Sticks.Add(new Stick(
                            Points[currentIndex],
                            Points[currentIndex + Constants.GridSize - 1],
                            diagStiffness
                        ));
                    }
                }
            }
        }

        #region Управление симуляцией

        public void StartSimulation()
        {
            _renderTimer.Start();
        }

        public void StopSimulation()
        {
            _renderTimer.Stop();
        }

        /// <summary>
        /// Полный сброс симуляции (новая сетка)
        /// </summary>
        public void ResetSimulation()
        {
            _settleFrames = 10;

            // Восстанавливаем исходную позицию платформы
            _platform = CreatePlatform();

            // Переинициализируем сетку
            InitializeGrid(_platform);

            // Обновляем физический движок
            _physicsEngine = new PhysicsEngine(Points, Sticks, CanvasWidth, CanvasHeight)
            {
                Platform = _platform
            };

            // Обновляем сервис отрисовки
            _renderingService = new RenderingService(Bitmap, Points, Sticks, _platform);
        }

        /// <summary>
        /// Перезапуск рендеринга (очистка экрана)
        /// </summary>
        public void RestartRender()
        {
            // Очищаем bitmap
            _renderingService.ClearBitmap();
        }

        private void OnRenderTick(object sender, EventArgs e)
        {
            bool applyGravity = _settleFrames-- <= 0;

            _physicsEngine.UpdatePhysics(applyGravity);
            _renderingService.UpdatePlatformPosition(_physicsEngine.Platform);
            _renderingService.Render();
        }

        #endregion

        #region Обработка ввода

        /// <summary>
        /// Обрабатывает нажатие кнопки мыши
        /// </summary>
        public bool HandleMouseDown(Point position, MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return _physicsEngine.StartPlatformDrag(position);

                case MouseButton.Right:
                    _physicsEngine.CreateExplosion(position, Points, Sticks);
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Обрабатывает перемещение мыши
        /// </summary>
        public void HandleMouseMove(Point position)
        {
            _physicsEngine.UpdatePlatformDrag(position);
        }

        /// <summary>
        /// Обрабатывает отпускание кнопки мыши
        /// </summary>
        public void HandleMouseUp()
        {
            _physicsEngine.StopPlatformDrag();
        }

        #endregion

        #region INotifyPropertyChanged

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
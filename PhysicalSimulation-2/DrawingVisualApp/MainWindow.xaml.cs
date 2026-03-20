using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DrawingVisualApp.ViewModels;

namespace DrawingVisualApp
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private bool _isDraggingPlatform = false;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            Loaded += OnWindowLoaded;
        }

        /// <summary>
        /// Обработчик загрузки окна
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            InitializeCanvas();
            InitializeSimulation();
        }

        /// <summary>
        /// Инициализирует размеры канваса
        /// </summary>
        private void InitializeCanvas()
        {
            _viewModel.CanvasWidth = renderImage.Width;
            _viewModel.CanvasHeight = renderImage.Height;

            // Создаем и устанавливаем WriteableBitmap
            _viewModel.InitializeBitmap((int)renderImage.Width, (int)renderImage.Height);
            renderImage.Source = _viewModel.Bitmap;
        }

        /// <summary>
        /// Инициализирует и запускает симуляцию
        /// </summary>
        private void InitializeSimulation()
        {
            _viewModel.Initialize();
            _viewModel.StartSimulation();
        }

        /// <summary>
        /// Сброс всей симуляции (новая сетка)
        /// </summary>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.StopSimulation();
            _viewModel.ResetSimulation();
            _viewModel.StartSimulation();
        }

        #region Обработчики событий мыши

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(renderImage);

            if (e.ChangedButton == MouseButton.Left)
            {
                _isDraggingPlatform = _viewModel.HandleMouseDown(mousePosition, e.ChangedButton);
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                _viewModel.HandleMouseDown(mousePosition, e.ChangedButton);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingPlatform)
            {
                Point mousePosition = e.GetPosition(renderImage);
                _viewModel.HandleMouseMove(mousePosition);
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDraggingPlatform = false;
                _viewModel.HandleMouseUp();
            }
        }

        #endregion
    }
}
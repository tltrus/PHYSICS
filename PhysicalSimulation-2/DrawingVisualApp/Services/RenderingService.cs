using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DrawingVisualApp.Models;

namespace DrawingVisualApp.Services
{
    /// <summary>
    /// Сервис для отрисовки физической симуляции с использованием WriteableBitmapEx
    /// </summary>
    public class RenderingService
    {
        private readonly WriteableBitmap _bitmap;
        private readonly List<VerletPoint> _points;
        private readonly List<Stick> _sticks;
        private Rect _platform;

        // Цвета для отрисовки
        private readonly Color _backgroundColor = Colors.Black;
        private readonly Color _platformColor = Color.FromRgb(47, 79, 79); // DarkSlateGray
        private readonly Color _pointColor = Colors.White;
        private readonly Color _stickColor = Colors.Gray;

        // Кисть для заливки фона
        private readonly SolidColorBrush _backgroundBrush;

        public RenderingService(WriteableBitmap bitmap, List<VerletPoint> points,
                              List<Stick> sticks, Rect platform)
        {
            _bitmap = bitmap;
            _points = points;
            _sticks = sticks;
            _platform = platform;
            _backgroundBrush = new SolidColorBrush(_backgroundColor);
        }

        /// <summary>
        /// Отрисовывает текущее состояние симуляции
        /// </summary>
        public void Render()
        {
            try
            {
                _bitmap.Lock();

                // Очищаем bitmap
                _bitmap.Clear(_backgroundColor);

                // Отрисовываем платформу
                RenderPlatform();

                // Отрисовываем связи
                RenderSticks();

                // Отрисовываем точки
                RenderPoints();
            }
            finally
            {
                _bitmap.Unlock();
            }
        }

        /// <summary>
        /// Отрисовывает платформу
        /// </summary>
        private void RenderPlatform()
        {
            int left = (int)Math.Max(0, _platform.Left);
            int top = (int)Math.Max(0, _platform.Top);
            int right = (int)Math.Min(_bitmap.PixelWidth - 1, _platform.Right);
            int bottom = (int)Math.Min(_bitmap.PixelHeight - 1, _platform.Bottom);

            if (right <= left || bottom <= top)
                return;

            _bitmap.FillRectangle(left, top, right, bottom, _platformColor);
        }

        /// <summary>
        /// Отрисовывает связи между точками
        /// </summary>
        private void RenderSticks()
        {
            foreach (var stick in _sticks)
            {
                int x1 = (int)stick.PointA.Position.X;
                int y1 = (int)stick.PointA.Position.Y;
                int x2 = (int)stick.PointB.Position.X;
                int y2 = (int)stick.PointB.Position.Y;

                // Проверяем, находятся ли точки в пределах bitmap
                if (IsPointInBitmap(x1, y1) && IsPointInBitmap(x2, y2))
                {
                    _bitmap.DrawLine(x1, y1, x2, y2, _stickColor);
                }
            }
        }

        /// <summary>
        /// Отрисовывает физические точки
        /// </summary>
        private void RenderPoints()
        {
            int radius = (int)Constants.PointRadius;
            int diameter = radius * 2;

            foreach (var point in _points)
            {
                int centerX = (int)point.Position.X;
                int centerY = (int)point.Position.Y;

                if (IsPointInBitmap(centerX, centerY))
                {
                    _bitmap.FillEllipse(
                        centerX - radius,
                        centerY - radius,
                        centerX + radius,
                        centerY + radius,
                        _pointColor);
                }
            }
        }

        /// <summary>
        /// Проверяет, находится ли точка в пределах bitmap
        /// </summary>
        private bool IsPointInBitmap(int x, int y)
        {
            return x >= 0 && x < _bitmap.PixelWidth &&
                   y >= 0 && y < _bitmap.PixelHeight;
        }

        /// <summary>
        /// Обновляет позицию платформы для отрисовки
        /// </summary>
        public void UpdatePlatformPosition(Rect newPlatform)
        {
            _platform = newPlatform;
        }

        /// <summary>
        /// Очищает bitmap (черный фон)
        /// </summary>
        public void ClearBitmap()
        {
            try
            {
                _bitmap.Lock();
                _bitmap.Clear(_backgroundColor);
            }
            finally
            {
                _bitmap.Unlock();
            }
        }
    }
}
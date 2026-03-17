
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DrawingVisualApp
{
    public class DrawingVisualClass : Canvas
    {
        private readonly List<Visual> visuals = new List<Visual>();

        public DrawingVisualClass()
        {
            // Устанавливаем черный фон по умолчанию
            Background = Brushes.Black;

            // Важно! Отключаем кэширование для плавной анимации
            CacheMode = new BitmapCache();

            // Разрешаем обрезку содержимого
            ClipToBounds = true;
        }

        protected override int VisualChildrenCount => visuals.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= visuals.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return visuals[index];
        }

        public void AddVisual(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            visuals.Add(visual);
            AddVisualChild(visual);
            AddLogicalChild(visual);
        }

        public void RemoveVisual(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            if (visuals.Remove(visual))
            {
                RemoveVisualChild(visual);
                RemoveLogicalChild(visual);
            }
        }

        public void ClearVisuals()
        {
            foreach (var visual in visuals)
            {
                RemoveVisualChild(visual);
                RemoveLogicalChild(visual);
            }
            visuals.Clear();
        }

        // Переопределяем для корректного отображения
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (Visual visual in visuals)
            {
                if (visual is UIElement uiElement)
                {
                    uiElement.Arrange(new Rect(arrangeSize));
                }
            }
            return arrangeSize;
        }
    }
}

namespace DrawingVisualApp.Models
{
    /// <summary>
    /// Константы физического движка и графики
    /// </summary>
    public static class Constants
    {
        // Размеры сетки
        public const int GridSize = 25;          // Количество точек по каждой оси
        public const double Spacing = 16;        // Расстояние между точками в сетке
        public const double PointRadius = 3;     // Радиус отрисовки точки

        // Физические параметры
        public const double Gravity = 0.15;      // Сила гравитации
        public const double Damping = 0.995;     // Коэффициент затухания скорости

        // Трение
        public const double PlatformFriction = 0.95;  // Трение платформы
        public const double WallFriction = 0.92;     // Трение стен

        // Параметры коллизий
        public const double ContactEpsilon = 15.0;   // Погрешность контакта для захвата точек
        public const int ConstraintIterations = 5;  // Итераций решения ограничений за кадр

        // Дополнительные параметры
        public const double GrabRadius = 28;     // Радиус захвата точек (не используется)
        public const double BounceFactor = 0.6;  // Коэффициент отскока от границ
    }
}
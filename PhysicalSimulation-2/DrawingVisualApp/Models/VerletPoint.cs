using System.Windows;

namespace DrawingVisualApp.Models
{
    /// <summary>
    /// Точка в физической системе Verlet
    /// </summary>
    public class VerletPoint
    {
        // Текущая и предыдущая позиции для вычисления скорости
        public Point Position { get; set; }
        public Point PreviousPosition { get; set; }

        // Накопленная сила, применяемая к точке
        private Vector Force { get; set; }

        /// <summary>
        /// Создает новую точку
        /// </summary>
        public VerletPoint(Point position)
        {
            Position = position;
            PreviousPosition = position;
            Force = new Vector();
        }

        /// <summary>
        /// Применяет внешнюю силу к точке
        /// </summary>
        public void ApplyForce(Vector force) => Force += force;

        /// <summary>
        /// Применяет гравитацию к точке
        /// </summary>
        public void ApplyGravity(double gravity) =>
            Force = new Vector(Force.X, Force.Y + gravity);

        /// <summary>
        /// Обновляет позицию точки по методу Верле
        /// </summary>
        public void Update(double damping)
        {
            // скорость (учёт реального времени кадра)
            Vector velocity = (Position - PreviousPosition) * damping;

            PreviousPosition = Position;

            // масштабируем силу временем
            Position += velocity + Force;

            Force = new Vector();
        }

        /// <summary>
        /// Применяет импульс к точке (изменяет предыдущую позицию)
        /// </summary>
        public void ApplyImpulse(Vector impulse) => PreviousPosition -= impulse;
    }
}
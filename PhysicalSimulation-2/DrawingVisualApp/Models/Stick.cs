using System.Windows;

namespace DrawingVisualApp.Models
{
    /// <summary>
    /// Связь между двумя точками в физической системе
    /// </summary>
    public class Stick
    {
        public VerletPoint PointA { get; }
        public VerletPoint PointB { get; }

        private double RestLength { get; }  // Исходная длина связи
        private double Stiffness { get; }   // Жесткость связи (0-1)

        public Stick(VerletPoint pointA, VerletPoint pointB, double stiffness)
        {
            PointA = pointA;
            PointB = pointB;
            RestLength = (PointA.Position - PointB.Position).Length;
            Stiffness = stiffness;
        }

        /// <summary>
        /// Удовлетворяет ограничению длины связи
        /// </summary>
        public void Satisfy()
        {
            // Вектор между точками
            Vector delta = PointB.Position - PointA.Position;
            double currentLength = delta.Length;

            if (currentLength == 0) return;

            // Вычисляем разницу между текущей и исходной длиной
            double lengthDifference = (currentLength - RestLength) / currentLength;

            // Коррекция для каждой точки
            Vector correction = delta * 0.5 * lengthDifference * Stiffness;

            // Применяем коррекцию к точкам
            PointA.Position += correction;
            PointB.Position -= correction;
        }
    }
}
using System;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
    class Line
    {
        public Vector x1;
        public Vector x2;
        Brush brush;

        public Line(Brush brush)
        {
            this.brush = brush;
        }

        public void Rotate(double angle)
        {
            Vector sub = x1 - x2;
            Vector rotatedline = RotateVector(sub, angle);
            x2 = x1 + rotatedline;  // обновляем первую точку
        }

        public void Draw(DrawingContext dc)
        {
            Point p1 = new Point(x1.X, x1.Y);
            Point p2 = new Point(x2.X, x2.Y);
            dc.DrawLine(new Pen(brush, 4), p1, p2);
        }

        public Vector RotateVector(Vector v, double a)
        {
            var newHeading = Math.Atan2(v.Y, v.X) + a;
            var mag = Math.Sqrt(v.X * v.X + v.Y * v.Y);
            var cos = Math.Cos(newHeading) * mag;
            var sin = Math.Sin(newHeading) * mag;
            return new Vector(cos, sin);
        }
    }
}

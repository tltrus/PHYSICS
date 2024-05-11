using System;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
    class Pendulum
    {
        public int l = 100;   // length
        public int m;     // mass
        public double a = Math.PI/2;    // angle
        public Point c;     // hanging point
        public Point pos;   // position
        public double vel;
        public double acc;
        Brush brush;


        public Pendulum(double x, double y)
        {
            brush = Brushes.White;
            c = new Point(x, y);

            var x1 = c.X + l * Math.Sin(a); // Синус и косинус тут наоборот
            var y1 = c.Y + l * Math.Cos(a); // чтобы на 90 градусов развернуть положение маятников
            pos = new Point(x1, y1);
        }

        public void Show(DrawingContext dc)
        {
            pos.X = c.X + l * Math.Sin(a); // Синус и косинус тут наоборот
            pos.Y = c.Y + l * Math.Cos(a);
            Point p1 = new Point(pos.X, pos.Y);
            dc.DrawLine(new Pen(brush, 1), c, p1);

            dc.DrawEllipse(brush, null, p1, 7, 7);

            vel += acc;
            a += vel;
        }
    }
}

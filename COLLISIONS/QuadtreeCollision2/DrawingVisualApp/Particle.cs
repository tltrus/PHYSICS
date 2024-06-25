using System;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
    class Particle
    {
        public double x, y;
        public int r = 4;
        bool highlight;
        Brush color;

        public Particle(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Intersects(Particle other)
        {
            var d = Dist(x, y, other.x, other.y);
            return d < r + other.r;
        }

        public void SetHighlight(bool value) => highlight = value;

        public void Move()
        {
            x += MainWindow.rnd.Next(-1, 2);
            y += MainWindow.rnd.Next(-1, 2);
        }

        double Dist(double x1, double y1, double x2, double y2)
        {
            var val = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            return Math.Sqrt(val);
        }

        public void Show(DrawingContext dc)
        {
            if (highlight)
            {
                color = Brushes.Red;
            }
            else
            {
                color = Brushes.White;
            }
            dc.DrawEllipse(color, null, new Point(x, y), r, r);
        }
    }
}

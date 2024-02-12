using System;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{

    class Circle
    {
        public int radius;
        public Vector pos;
        public Vector vel;
        public Brush brush;
        int width, height;

        public Circle()
        {
            width = MainWindow.width;
            height = MainWindow.height;
        }

        public void Update()
        {
            pos += vel;

            if (pos.X >= width - radius && vel.X > 0) vel.X = -vel.X;
            if (pos.X <= radius && vel.X < 0) vel.X = -vel.X;
            if (pos.Y >= height - radius && vel.Y > 0) vel.Y = -vel.Y;
            if (pos.Y <= radius && vel.Y < 0) vel.Y = -vel.Y;
        }

        public void DetectCollision(Line line, Line leftNormal)
        {
            Vector VectorCircle = pos - line.x1;
            Vector VectorLineNormal = Vector.Subtract(leftNormal.x2, leftNormal.x1);
            VectorLineNormal.Normalize();
            double projection = VectorCircle * VectorLineNormal; // Dot product
            if (projection <= radius)
                vel.Y *= -1;
        }

        public void Draw(DrawingContext dc)
        {
            Point p = new Point(pos.X, pos.Y);
            dc.DrawEllipse(brush, null, p, radius, radius);
        }

        double Dist(Vector v1, Vector v2)
        {
            var sub = v1 - v2;
            var dist = Math.Sqrt(sub.X * sub.X + sub.Y * sub.Y);
            return dist;
        }
    }
}

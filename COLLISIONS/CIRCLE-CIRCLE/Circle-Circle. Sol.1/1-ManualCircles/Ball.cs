using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
    class Ball
    {
        public int radius;
        public Vector pos;
        public Vector vel;
        public Brush brush;
        DrawingContext dc;
        int width, height;

        public Ball()
        {
            width = MainWindow.width;
            height = MainWindow.height;
        }

        public void Update()
        {
            if (pos.X >= width - radius && vel.X > 0) vel.X = -vel.X;
            if (pos.X <= radius && vel.X < 0) vel.X = -vel.X;
            if (pos.Y >= height - radius && vel.Y > 0) vel.Y = -vel.Y;
            if (pos.Y <= radius && vel.Y < 0) vel.Y = -vel.Y;
        }

        public void Update(Vector mouse)
        {
            pos = mouse;
        }

        public bool isMouseinCircle(Vector mouse)
        {
            var dist = Dist(mouse, pos);

            if (dist > radius)
                return false;

            return true;
        }
        public void DetectCollision(List<Ball> circles)
        {
            foreach(var circle in circles)
            {
                if (circle != this)
                {
                    if (CircleIntersect(circle))
                    {
                        //balls have collided
                        DrawCollision(this, circle);

                        var normal = circle.pos - pos;
                        normal.Normalize();
                        var distance = Math.Sqrt(SquareDistance(this, circle));
                        var vRelativeVelocity = circle.vel - vel;
                        var speed = vRelativeVelocity * normal;

                        var overlap = 0.5 * (distance - circle.radius - radius);
                        pos += normal * overlap;
                        circle.pos -= normal * overlap;
                    }
                }
            }
        }
        private bool CircleIntersect(Ball circle) => SquareDistance(this, circle) < (radius + circle.radius) * (radius + circle.radius);
        private double SquareDistance(Ball c1, Ball c2) => (c1.pos.X - c2.pos.X) * (c1.pos.X - c2.pos.X) + (c1.pos.Y - c2.pos.Y) * (c1.pos.Y - c2.pos.Y);


        public void Draw(DrawingContext dc)
        {
            this.dc = dc;
            
            Point p = new Point(pos.X, pos.Y);
            dc.DrawEllipse(brush, null, p, radius, radius);
        }
        private void DrawCollision(Ball c1, Ball c2)
        {
            var x1 = c1.pos.X;
            var x2 = c2.pos.X;
            var y1 = c1.pos.Y;
            var y2 = c2.pos.Y;
            var r1 = c1.radius;
            var r2 = c2.radius;

            var collisionPoint = new Point((x1 * r2 + x2 * r1) / (r1 + r2), (y1 * r2 + y2 * r1) / (r1 + r2));

            dc.DrawEllipse(Brushes.Red, null, collisionPoint, 5, 5);
        }

        double Dist(Vector v1, Vector v2)
        {
            var sub = v1 - v2;
            var dist = Math.Sqrt(sub.X * sub.X + sub.Y * sub.Y);
            return dist;
        }
    }
}

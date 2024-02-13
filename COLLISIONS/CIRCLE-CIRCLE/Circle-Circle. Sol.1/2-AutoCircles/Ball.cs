using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace AutoMovableCircles
{
    // Based on:
    // https://spicyyoghurt.com/tutorials/html5-javascript-game-development/collision-detection-physics

    class Ball
    {
        public int radius, mass;
        public Vector pos, vel;
        public Brush brush;
        int width, height;
        Random rnd;
        double g = 0.00981;
        double restitution = 0.8;

        public Ball()
        {
            width = MainWindow.width;
            height = MainWindow.height;
            rnd = MainWindow.rnd;

            pos = new Vector(rnd.Next(30, width - 30), 40);
            vel = RandomVector();
            brush = Brushes.White;
            mass = 10; // rnd.Next(3, 20);
            radius = mass;
        }
        public void DetectEdgeCollisions()
        {
            vel.Y += g;

            if (pos.X >= width - radius && vel.X > 0) vel.X = -vel.X * restitution;
            if (pos.X <= radius && vel.X < 0) vel.X = -vel.X * restitution;
            if (pos.Y >= height - radius && vel.Y > 0) vel.Y = -vel.Y * restitution;
            if (pos.Y <= radius && vel.Y < 0) vel.Y = -vel.Y * restitution;
        }
        public void DetectCollision(List<Ball> circles)
        {
            foreach (var circle in circles)
            {
                if (circle != this)
                {
                    if (isCircleIntersect(circle))
                    {
                        var normal = circle.pos - pos;
                        normal.Normalize();
                        var distance = Math.Sqrt(SquareDistance(this, circle));
                        var vRelativeVelocity = vel - circle.vel;
                        // Calculate speed of the detected collision
                        var speed = vRelativeVelocity * normal;
                        // Apply restitution to the speed
                        speed *= Math.Min(restitution, circle.restitution);

                        if (speed < 0)
                            break;

                        // Add mass, impulse and momentum
                        var impulse = 2 * speed / (mass + circle.mass);
                        vel -= normal * circle.mass * impulse;
                        circle.vel += normal * mass * impulse;
                    }
                }
            }
        }
        public void Update() => pos += vel;

        private bool isCircleIntersect(Ball circle) => SquareDistance(this, circle) <= (radius + circle.radius) * (radius + circle.radius);
        private double SquareDistance(Ball c1, Ball c2) => (c1.pos.X - c2.pos.X) * (c1.pos.X - c2.pos.X) + (c1.pos.Y - c2.pos.Y) * (c1.pos.Y - c2.pos.Y);

        public void Draw(DrawingContext dc)
        {
            // Draw circle
            Point p = new Point(pos.X, pos.Y);
            var pen = new Pen(brush, 1);
            dc.DrawEllipse(null, pen, p, radius, radius);

            // Draw guider
            var guider = new Vector(vel.X, vel.Y);
            guider = SetMag(vel, radius);
            Point p2 = new Point(pos.X + guider.X, pos.Y + guider.Y);
            dc.DrawLine(new Pen(Brushes.Red, 2), p, p2);
        }

        private Vector RandomVector()
        {
            var angle = rnd.NextDouble() * Math.PI * 2;
            return new Vector(Math.Cos(angle), Math.Sin(angle));
        }

        public Vector SetMag(Vector vel, int n)
        {
            Vector v = new Vector(vel.X, vel.Y);
            v.Normalize();
            v *= n;
            return v;
        }
    }
}

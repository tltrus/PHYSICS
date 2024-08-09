using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DrawingVisualApp
{
    class Particle
    {
        public Vector2D position, velocity, acceleration;
        public int mass, id;
        public double r;
        public Brush brush;

        public Particle(double x, double y, int mass, int id, Brush brush)
        {
            position = new Vector2D(x, y);
            acceleration = new Vector2D(0, 0);
            velocity = Vector2D.Random2D();
            velocity.Mult(MainWindow.rnd.NextDouble());
            this.brush = brush;
            this.mass = mass;
            this.id = id;
            this.r = Math.Sqrt(mass) * 2;
        }

        public void ApplyForce(Vector2D force)
        {
            var f = force.CopyToVector();
            f.Div(mass);
            acceleration.Add(f);
        }
        public void Update()
        {
            velocity.Add(acceleration);
            position.Add(velocity);
            acceleration.Mult(0);
        }
        public void Collide(Particle other)
        {
            var impactVector = Vector2D.Sub(other.position, position);
            var d = impactVector.Mag();
            if (d < r + other.r)
            {
                // Push the particles out so that they are not overlapping
                var overlap = d - (r + other.r);
                var dir = impactVector.CopyToVector();
                dir.SetMag(overlap * 0.5);
                position.Add(dir);
                other.position.Sub(dir);

                // Correct the distance!
                d = r + other.r;
                impactVector.SetMag(d);

                var mSum = mass + other.mass;
                var vDiff = Vector2D.Sub(other.velocity, velocity);
                // Particle A (this)
                var num = vDiff.Dot(impactVector);
                var den = mSum * d * d;
                var deltaVA = impactVector.CopyToVector();
                deltaVA.Mult((2 * other.mass * num) / den);
                velocity.Add(deltaVA);
                // Particle B (other)
                var deltaVB = impactVector.CopyToVector();
                deltaVB.Mult((-2 * mass * num) / den);
                other.velocity.Add(deltaVB);
            }
        }
        public void Edges()
        {
            if (position.X > MainWindow.width - r)
            {
                position.X = MainWindow.width - r;
                velocity.X *= -1;
            }
            else if (position.X < r)
            {
                position.X = r;
                velocity.X *= -1;
            }

            if (position.Y > MainWindow.height - r)
            {
                position.Y = MainWindow.height - r;
                velocity.Y *= -1;
            }
            else if (position.Y < r)
            {
                position.Y = r;
                velocity.Y *= -1;
            }
        }
        public void Show(DrawingContext dc)
        {
            // Circle
            dc.DrawEllipse(brush, null, new Point(position.X, position.Y), r, r);
        }

        public void ShowVector(DrawingContext dc)
        {
            // Vector of direction
            var v = velocity.CopyToVector();
            v.SetMag(r);
            var pv = position + v;
            Point p0 = new Point(position.X, position.Y);
            Point p1 = new Point(pv.X, pv.Y);
            dc.DrawLine(new Pen(Brushes.LightGray, 1), p0, p1);
        }
    }
}

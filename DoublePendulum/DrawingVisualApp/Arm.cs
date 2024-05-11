using System;
using System.Windows.Media;

namespace DrawingVisualApp
{
    internal class Arm
    {
        Pendulum b1, b2;
        double G = 1;

        public Arm(double x, double y, int m)
        {
            b1 = new Pendulum(x, y);
            b2 = new Pendulum(b1.pos.X, b1.pos.Y);

            b1.m = m;
            b2.m = m + 2;
        }

        // Formula
        private void Calculation()
        {
            var num1 = -G * (2 * b1.m + b2.m) * Math.Sin(b1.a);
            var num2 = -b2.m * G * Math.Sin(b1.a - 2 * b2.a);
            var num3 = -2 * Math.Sin(b1.a - b2.a) * b2.m;
            var num4 = b2.vel * b2.vel * b2.l + b1.vel * b1.vel * b1.l * Math.Cos(b1.a - b2.a);
            var den = b1.l * (2 * b1.m + b2.m - b2.m * Math.Cos(2 * b1.a - 2 * b2.a));
            b1.acc = (num1 + num2 + num3 * num4) / den;

            num1 = 2 * Math.Sin(b1.a - b2.a);
            num2 = b1.vel * b1.vel * b1.l * (b1.m + b2.m);
            num3 = G * (b1.m + b2.m) * Math.Cos(b1.a);
            num4 = b2.vel * b2.vel * b2.l * b2.m * Math.Cos(b1.a - b2.a);
            den = b2.l * (2 * b1.m + b2.m - b2.m * Math.Cos(2 * b1.a - 2 * b2.a));
            b2.acc = (num1 * (num2 + num3 + num4)) / den;
        }

        public void Draw(DrawingContext dc)
        {
            Calculation();

            b1.Show(dc);

            b2.c = b1.pos;
            b2.Show(dc);
        }
    }
}

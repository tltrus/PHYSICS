using System.Windows.Media;


namespace DrawingVisualApp
{
    class Spring
    {
        int restLength;
        double k;
        Brush brush;
        Particle a, b;

        public Spring(double k, int restLength, Particle a, Particle b)
        {
            this.restLength = restLength;
            this.k = k;
            brush = Brushes.White;
            this.a = a;
            this.b = b;
        }

        public void Update()
        {
            var force = Vector2D.Sub(b.position, a.position);
            var x = force.Mag() - restLength;
            force.Normalize();
            force.Mult(k * x);
            a.ApplyForce(force);
            force.Mult(-1);
            b.ApplyForce(force);
        }

        public void Show(DrawingContext dc)
        {
            var p0 = a.position.ToPoint();
            var p1 = b.position.ToPoint();
            dc.DrawLine(new Pen(brush, 2), p0, p1);
        }
    }
}

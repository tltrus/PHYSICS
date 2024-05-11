using System.Windows.Media;


namespace DrawingVisualApp
{
    class Particle
    {
        public Vector2D position;
        public Vector2D velocity;
        Vector2D acceleration;
        Brush brush;
        public bool locked;
        int mass = 1;


        public Particle(double x, double y)
        {
            position = new Vector2D(x, y);
            velocity = new Vector2D();
            acceleration = new Vector2D();
            brush = Brushes.White;
        }

        public void Update()
        {
            if (!locked)
            {
                velocity.Mult(0.99);
                velocity.Add(acceleration);
                position.Add(velocity);
                acceleration.Mult(0);
            }
        }

        // Newton's law: F = M * A
        public void ApplyForce(Vector2D force)
        {
            var f = force.CopyToVector();
            f.Div(mass);
            acceleration.Add(f);
        }

        public void Show(DrawingContext dc)
        {
            dc.DrawEllipse(Brushes.Blue, new Pen(brush, 1), position.ToPoint(), 4, 4);
        }
    }
}

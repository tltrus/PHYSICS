using System.Collections.Generic;
using System.Windows.Media;


namespace DrawingVisualApp
{
    class Chain
    {
        List<Particle> particles = new List<Particle>();
        List<Spring> springs = new List<Spring>();
        public Particle tail;

        int spacing = 4;
        double k;
        Vector2D gravity;

        public Chain(int x, double k = 0.1)
        {
            this.k = k;
            
            for (int i = 0; i < 20; ++i)
            {
                particles.Add(new Particle(x, i * spacing));
                if (i != 0)
                {
                    var a = particles[i];
                    var b = particles[i - 1];
                    var spring = new Spring(k, spacing, a, b);
                    springs.Add(spring);
                }
            }

            particles[0].locked = true;

            gravity = new Vector2D(0, 0.1);
        }

        public void Draw(DrawingContext dc)
        {
            foreach (var s in springs)
            {
                s.Update();
                s.Show(dc);
            }

            foreach (var p in particles)
            {
                p.ApplyForce(gravity);
                p.Update();
            }

            tail = particles[particles.Count - 1];
            dc.DrawEllipse(Brushes.Yellow, null, tail.position.ToPoint(), 4, 4);
        }
    }
}

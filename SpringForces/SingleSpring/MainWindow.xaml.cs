using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawingVisualApp
{
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timerDraw;
        public static Random rnd = new Random();
        public static int width, height;

        DrawingVisual visual;
        DrawingContext dc;

        List<Particle> particles = new List<Particle>();
        List<Spring> springs = new List<Spring>();
        Particle tail;
        int spacing = 4;
        double k = 0.1;
        Vector2D mouse, gravity;


        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            timerDraw = new System.Windows.Threading.DispatcherTimer();
            timerDraw.Tick += new EventHandler(timerDrawTick);
            timerDraw.Interval = new TimeSpan(0, 0, 0, 0, 10);

            for (int i = 0; i < 20; ++i)
            {
                particles.Add(new Particle(width/2, i * spacing));
                if(i != 0)
                {
                    var a = particles[i];
                    var b = particles[i - 1];
                    var spring = new Spring(k, spacing, a, b);
                    springs.Add(spring);
                }
            }

            particles[0].locked = true;

            gravity = new Vector2D(0, 0.1);

            timerDraw.Start();
        }

        private void timerDrawTick(object sender, EventArgs e)
        {
            Drawing();
        }

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
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
                    p.Show(dc);
                }

                tail = particles[particles.Count - 1];
                dc.DrawEllipse(Brushes.Yellow, null, tail.position.ToPoint(), 4, 4);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void g_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mouse = e.GetPosition(g).ToVector2D();
                tail.position.Set(mouse.X, mouse.Y);
                tail.velocity.Set(0, 0);
            }
        }
    }

    public static class PointExtention
    {
        public static Vector2D ToVector2D(this Point p)
        {
            return new Vector2D(p.X, p.Y);
        }

        public static Point ToPoint(this Vector2D v)
        {
            return new Point(v.X, v.Y);
        }
    }
}

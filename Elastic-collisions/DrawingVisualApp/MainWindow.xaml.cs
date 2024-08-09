using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawingVisualApp
{
    // Based on #184 — Elastic Collisions https://thecodingtrain.com/challenges/184-elastic-collisions
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        public static Random rnd = new Random();
        public static int width, height;

        DrawingVisual visual;
        DrawingContext dc;
        List<Particle> particles = new List<Particle>();

        Brush[] brushes = new Brush[] { Brushes.Brown, Brushes.Red, Brushes.Green, Brushes.Yellow, Brushes.White, Brushes.Blue, Brushes.Pink };

        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 20);

            Init();
        }

        private void Init()
        {
            for (int i = 0; i < 50; ++i)
            {
                var x = rnd.Next(width);
                var y = rnd.Next(height);
                var brush = brushes[rnd.Next(brushes.Count())];
                var mass = rnd.Next(4, 260);
                var particle = new Particle(x, y, mass, i, brush);

                particles.Add(particle);
            }
            timer.Start();
        }


        private void timerTick(object sender, EventArgs e) => Drawing();

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                var boundary = new Rectangle(0, 0, width, height);
                var qtree = new QuadTree(boundary, 4);

                foreach (var p in particles)
                {
                    var point = new myPoint(p.position.X, p.position.Y, p);
                    qtree.Insert(point);

                    p.Update();
                    p.Edges();
                    p.Show(dc);

                    if (cbShowVectors.IsChecked == true)
                        p.ShowVector(dc);
                }

                foreach (var p in particles)
                {
                    var range = new Circle(p.position.X, p.position.Y, (int)p.r * 2);
                    var points = qtree.Query(range);
                    foreach (var point in points)
                    {
                        var other = point.userData;
                        if (p != other)
                        {
                            p.Collide(other);
                        }
                    }
                }

                if (cbShowQtree.IsChecked == true)
                    qtree.Show(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void g_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

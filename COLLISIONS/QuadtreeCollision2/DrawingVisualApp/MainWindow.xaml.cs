using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
    /// <summary>
    /// Based on Coding Challenge #98.1
    /// Quadtree
    /// https://thecodingtrain.com/CodingChallenges/098.3-quadtree
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        public static Random rnd = new Random();
        public static int width, height;

        DrawingVisual visual;
        DrawingContext dc;

        List<Particle> particles;
        int capacity, particles_num;

        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 60);
        }

        private void timerTick(object sender, EventArgs e) => Drawing();

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            capacity = int.Parse(tbCap.Text);
            particles_num = int.Parse(tbParts.Text);

            particles = new List<Particle>();

            for (var i = 0; i < particles_num; i++)
            {
                particles.Add(new Particle(rnd.Next(width), rnd.Next(height)));
            }


            timer.Start();
        }

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                var boundary = new Rectangle(0, 0, width, height);
                var qtree = new QuadTree(boundary, capacity);

                foreach (var p in particles)
                {
                    var point = new myPoint(p.x, p.y, p);
                    qtree.Insert(point);

                    p.Move();
                    p.Show(dc);
                    p.SetHighlight(false);
                }

                foreach (var p in particles)
                {
                    var range = new Circle(p.x, p.y, p.r * 2);
                    var points = qtree.Query(range);
                    foreach (var point in points)
                    {
                        var other = point.userData;
                        // for (let other of particles) {
                        if (p != other && p.Intersects(other))
                        {
                            p.SetHighlight(true);
                        }
                    }
                }

                qtree.Show(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }
    }
}

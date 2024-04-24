using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawingVisualApp
{
    /// <summary>
    /// Based on: Coding Challenge #98.1 Quadtree https://thecodingtrain.com/challenges/98-quadtree
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        public static Random rnd = new Random();
        public static int width, height;

        DrawingVisual visual;
        DrawingContext dc;

        QuadTree qTree;
        Point mouse = new Point();

        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            Rectangle boundary = new Rectangle(0, 0, width, height);
            qTree = new QuadTree(boundary, 4);
            for (var i = 0; i < 300; i++)
            {
                var x = RandomGaussian(width / 2, width / 8, rnd);
                var y = RandomGaussian(height / 2, height / 8, rnd);
                Point p = new Point(x, y);
                qTree.Insert(p);
            }

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 50);

            timer.Start();
        }

        private void timerTick(object sender, EventArgs e) => Drawing();

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                qTree.Show(dc);

                // Range 
                int size = 40;
                Rectangle range = new Rectangle(mouse.X - size, mouse.Y - size, size, size);
                if (mouse.X < width && mouse.Y < height)
                {
                    Rect rect = new Rect(new Point(range.x, range.y),
                                new Point(range.x + range.w, range.y + range.h));
                    dc.DrawRectangle(null, new Pen(Brushes.White, 1), rect);

                    List<Point> points = qTree.Query(range);
                    foreach (var p in points)
                    {
                        dc.DrawEllipse(Brushes.Red, null, p, 2, 2);
                    }
                }

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void g_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var x = e.GetPosition(g).X;
            var y = e.GetPosition(g).Y;

            for (int i = 0; i < 5; i++)
            {
                qTree.Insert(new Point(x + rnd.Next(-5, 5), y + rnd.Next(-5, 5)));
            }

        }

        private void g_MouseMove(object sender, MouseEventArgs e)
        {
            mouse.X = e.GetPosition(g).X;
            mouse.Y = e.GetPosition(g).Y;
        }

        private double RandomGaussian(double mean, double stdDev, Random rand)
        {
            double d = 1.0 - rand.NextDouble();
            double num = 1.0 - rand.NextDouble();
            double num2 = Math.Sqrt(-2.0 * Math.Log(d)) * Math.Sin(Math.PI * 2.0 * num);
            return mean + stdDev * num2;
        }
    }
}

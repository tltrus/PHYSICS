using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawingVisualApp
{

    // Based on Challenges #160 — Spring Forces https://thecodingtrain.com/challenges/160-spring-forces
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timerDraw;
        public static Random rnd = new Random();
        public static int width, height;
        DrawingVisual visual;
        DrawingContext dc;
        Vector2D mouse;

        List<Chain> chains = new List<Chain>();

        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            timerDraw = new System.Windows.Threading.DispatcherTimer();
            timerDraw.Tick += new EventHandler(TimerDrawTick);
            timerDraw.Interval = new TimeSpan(0, 0, 0, 0, 10);

            int chaincount = width / 40;

            for (int i = 1; i < chaincount; ++i)
            {
                chains.Add(new Chain(i * 40, rnd.NextDouble(0.1, 0.4)));
            }

            timerDraw.Start();
        }

        private void TimerDrawTick(object sender, EventArgs e) => Drawing();

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                foreach(var c in chains)
                {
                    c.Draw(dc);
                }

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void g_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mouse = e.GetPosition(g).ToVector2D();

                foreach(var c in chains)
                {
                    c.tail.position.Set(mouse.X, mouse.Y);
                    c.tail.velocity.Set(0, 0);
                }
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

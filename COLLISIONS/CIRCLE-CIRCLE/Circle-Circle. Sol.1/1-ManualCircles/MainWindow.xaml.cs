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

        Vector mouse = new Vector();
        List<Ball> manualcircles = new List<Ball>();


        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            timerDraw = new System.Windows.Threading.DispatcherTimer();
            timerDraw.Tick += new EventHandler(timerDrawTick);
            timerDraw.Interval = new TimeSpan(0, 0, 0, 0, 10);

            Init();

            //timerDraw.Start();
            Draw();
        }

        private void Init()
        {
            // Manual circles
            var ball1 = new Ball()
            {
                pos = new Vector(50, 60),
                vel = new Vector(0, 0),
                radius = 20,
                brush = Brushes.White
            };
            var ball2 = new Ball()
            {
                pos = new Vector(250, 80),
                vel = new Vector(0, 0),
                radius = 20,
                brush = Brushes.White
            };

            manualcircles.Add(ball1);
            manualcircles.Add(ball2);
        }

        private void Draw()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                foreach (var c in manualcircles)
                    c.Draw(dc);

                manualcircles[0].Update(mouse);
                for (int i = 1; i < manualcircles.Count; i++)
                    manualcircles[i].Update();

                foreach (var c in manualcircles)
                    c.DetectCollision(manualcircles);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void timerDrawTick(object sender, EventArgs e) => Draw();

        private void g_MouseMove(object sender, MouseEventArgs e)
        {
            mouse = e.GetPosition(g).ToVector();

            Draw();
        }
    }

    public static class PointExtention
    {
        public static Vector ToVector(this Point p) => new Vector(p.X, p.Y);
    }
}

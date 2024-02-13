using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace AutoMovableCircles
{
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timerDraw;
        public static Random rnd = new Random();
        public static int width, height;

        DrawingVisual visual;
        DrawingContext dc;

        List<Ball> movablecircles = new List<Ball>();


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

            timerDraw.Start();
        }

        private void Init()
        {
            for (int i = 0; i < 50; i++)
            {
                var circle = new Ball();
                movablecircles.Add(circle);
            }
        }

        private void Loop()
        {
            foreach (var c in movablecircles)
            {
                c.DetectEdgeCollisions();
                c.DetectCollision(movablecircles);
                c.Update();
            }

            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                foreach (var c in movablecircles)
                    c.Draw(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void timerDrawTick(object sender, EventArgs e) => Loop();
    }
}

using System;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
    // Based on Coding Challenge #93. Double pendulum
    // https://github.com/CodingTrain/website/blob/main/CodingChallenges/CC_093_DoublePendulum/P5/sketch.js

    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timerDraw;
        public static Random rnd = new Random();
        public static int width, height;
        DrawingVisual visual;
        DrawingContext dc;
        Arm armL, armR;


        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            timerDraw = new System.Windows.Threading.DispatcherTimer();
            timerDraw.Tick += new EventHandler(timerDrawTick);
            timerDraw.Interval = new TimeSpan(0, 0, 0, 0, 30);

            armL = new Arm(200, 120, 4);
            armR = new Arm(300, 120, 7);

            timerDraw.Start();
        }

        private void timerDrawTick(object sender, EventArgs e) => Drawing();

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                // Body
                dc.DrawEllipse(Brushes.Gray, null, new Point(250, 90), 20, 30);
                Rect rect = new Rect()
                {
                    X = 220,
                    Y = 120,
                    Width = 60,
                    Height = 150
                };
                dc.DrawRectangle(Brushes.Gray, null, rect);
                rect = new Rect()
                {
                    X = 200,
                    Y = 120,
                    Width = 100,
                    Height = 10
                };
                dc.DrawRectangle(Brushes.Gray, null, rect);

                armL.Draw(dc);
                armR.Draw(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }
    }
}

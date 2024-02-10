using System;
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

        Water Water;


        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            timerDraw = new System.Windows.Threading.DispatcherTimer();
            timerDraw.Tick += new EventHandler(timerDrawTick);
            timerDraw.Interval = new TimeSpan(0, 0, 0, 0, 20);

            Water = new Water(rnd, width, height);

            timerDraw.Start();
        }

        private void timerDrawTick(object sender, EventArgs e) => Drawing();


        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                Water.Update();
                Water.Draw(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void g_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Water.x = e.GetPosition(g).X;
            Water.y = e.GetPosition(g).Y;

            Water.Splash(100);
        }

    }

    public static class MiscExtention
    {
        public static Vector2D ToVector2D(this Point p)
        {
            return new Vector2D(p.X, p.Y);
        }

        public static Point ToPoint(this Vector2D v)
        {
            return new Point(v.X, v.Y);
        }

        /// <summary>
        /// Случайный выбор числа в диапазоне (мин, макс)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="minNumber">Минимальная граница</param>
        /// <param name="maxNumber">Максимальная граница</param>
        /// <returns></returns>
        public static double NextDoubleRange(this System.Random random, double minNumber, double maxNumber)
        {
            return random.NextDouble() * (maxNumber - minNumber) + minNumber;
        }
    }
}

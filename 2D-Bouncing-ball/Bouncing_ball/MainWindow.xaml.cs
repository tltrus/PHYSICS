using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BouncingBall
{
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        static Random rnd = new Random();
        WriteableBitmap wb2;
        static int imgWidth, imgHeight;
        List<Ball> balls;
        Vector gravity;

        public MainWindow() => InitializeComponent();

        private void Window_Initialized(object sender, EventArgs e)
        {
            imgWidth = (int)image.Width; imgHeight = (int)image.Height;

            wb2 = new WriteableBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Bgra32, null); 
            image.Source = wb2;

            gravity = new Vector(0, 1);

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            init();

            Control();
        }

        void init()
        {
            balls = new List<Ball>();
            for (int i = 0; i < 1; i++)
                balls.Add(new Ball(imgWidth, imgHeight));

            //balls[0].ApplyForce(new Vector(0.5, 1));
        }

        void Control()
        {
            wb2.Clear(Colors.Black);
            foreach (var ball in balls)
            {
                ball.AddForce(gravity);

                if (ball.isFloorContact())
                {
                    var c = 0.1;
                    var force = new Vector(ball.velocity.X, ball.velocity.Y);
                    force *= -1;

                    //set 
                    force.Normalize();
                    force *= c;

                    ball.AddForce(force);
                }

                ball.Update();
                wb2.FillEllipseCentered((int)ball.position.X, (int)ball.position.Y, ball.R, ball.R, Colors.White);
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            init();
            
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e) => Control();

    }
}

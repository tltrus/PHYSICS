using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawingVisualApp
{
    // Based on
    // https://code.tutsplus.com/tutorials/quick-tip-collision-detection-between-a-circle-and-a-line--active-10546

    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timerDraw;
        public static Random rnd = new Random();
        public static int width, height;

        DrawingVisual visual;
        DrawingContext dc;

        Circle circle;
        Line line, leftNormal;

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
            circle = new Circle()
            {
                pos = new Vector(60, 60),
                vel = new Vector(0, 1),
                radius = 20,
                brush = Brushes.White
            };
            line = new Line(Brushes.Blue)
            {
                x1 = new Vector(20, 140),
                x2 = new Vector(100, 180)
            };
            leftNormal = new Line(Brushes.Red)
            {
                x1 = new Vector(20, 140),
                x2 = new Vector(100, 180)
            };
            leftNormal.Rotate(Math.PI * 0.5);
        }

        private void Loop()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                circle.Update();
                circle.DetectCollision(line, leftNormal);
                circle.Draw(dc);

                line.Draw(dc);
                leftNormal.Draw(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void timerDrawTick(object sender, EventArgs e) => Loop();
    }
}

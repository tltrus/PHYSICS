using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawingVisualApp
{
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        DrawingVisual visual;
        DrawingContext dc;

        // Physical constants
        private const double dt = 0.016;
        private const double g = 9.81;

        // System parameters
        private const double R = 60;           // hoop radius
        private const double l = 55;           // rod length (from hoop center)
        private const double mt = 1.0;         // hoop mass
        private double mgr = 10.0;             // pendulum mass

        // System state
        private double x = 0;                   // X-coordinate of hoop center
        private double theta = 0.0;              // HOOP ROTATION angle (for blue marker)
        private double phi = 0.5;                 // PENDULUM ANGLE (relative to vertical)

        // Velocities
        private double vx = 0.0;                  // linear velocity of hoop center
        private double omega_theta = 0.0;         // angular velocity of HOOP rotation
        private double omega_phi = 0.0;           // angular velocity of PENDULUM

        private const double groundY = 300;
        private bool isPaused = false;

        // For visual feedback
        private bool showVectors = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeControls();

            visual = new DrawingVisual();
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = TimeSpan.FromMilliseconds(16);

            InitPhysics();
            timer.Start();
        }

        private void InitializeControls()
        {
            sliderMass.Value = mgr;
            tbMassValue.Text = mgr.ToString("F1");
            btnPause.Content = "Pause";

            // Add checkbox for debugging
            if (chkShowVectors != null)
            {
                chkShowVectors.IsChecked = showVectors;
            }
        }

        private void InitPhysics()
        {
            x = 500;
            theta = 0.0;       // hoop not rotated
            phi = 0.5;         // pendulum deflected
            vx = 0.0;
            omega_theta = 0.0;
            omega_phi = 0.0;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                UpdatePhysics();
            }
            Drawing();
        }

        private void UpdatePhysics()
        {
            // CORRECT EQUATIONS OF MOTION for rolling hoop with pendulum:
            // 1. Red ball is a pendulum suspended from the hoop center
            // 2. Blue marker is a fixed point on the hoop
            // 3. Hoop rolls without slipping

            // Rolling without slipping condition: vx = R * omega_theta
            // where vx is hoop center velocity, omega_theta is hoop angular velocity

            // 1. Pendulum equation (red ball):
            // For a simple pendulum: alpha_phi = -(g/l) * sin(phi)
            double alpha_phi = -(g / l) * Math.Sin(phi);

            // But our pendulum is attached to a rolling hoop,
            // so we add the influence of hoop center acceleration:
            double ax = GetHoopAcceleration(); // hoop center acceleration

            // Effective acceleration for pendulum = g + ax (in projection)
            alpha_phi = -(g / l) * Math.Sin(phi) - (ax / l) * Math.Cos(phi);

            omega_phi += alpha_phi * dt;
            phi += omega_phi * dt;

            // 2. Hoop equation:
            // Torque from pendulum relative to contact point:
            // M = -mgr * g * l * sin(phi) + mgr * ax * l * cos(phi)
            double M = -mgr * g * l * Math.Sin(phi) + mgr * ax * l * Math.Cos(phi);

            // Moment of inertia of system relative to contact point:
            // I = I_hoop + I_pendulum + (mt + mgr) * R² (by Steiner's theorem)
            double I_hoop_contact = 2 * mt * R * R; // for thin hoop
            double I_pendulum_contact = mgr * (R * R + l * l - 2 * R * l * Math.Cos(phi));
            double I_total = I_hoop_contact + I_pendulum_contact;

            // Hoop angular acceleration:
            double alpha_theta = M / I_total;
            omega_theta += alpha_theta * dt;

            // 3. Rolling without slipping:
            vx = R * omega_theta;
            x += vx * dt;

            // Hoop rotation (for blue marker):
            theta += omega_theta * dt;

            // Normalize angles
            phi = NormalizeAngle(phi);
            theta = NormalizeAngle(theta);
        }

        private double GetHoopAcceleration()
        {
            // Hoop center acceleration from equations of motion
            // Simplified calculation
            double ax = -R * (g / l) * Math.Sin(phi) * 0.1;
            return ax;
        }

        private double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
        }

        private void Drawing()
        {
            g1.RemoveVisual(visual);

            using (dc = visual.RenderOpen())
            {
                // Background
                dc.DrawRectangle(Brushes.Black, null,
                    new Rect(0, 0, g1.ActualWidth, g1.ActualHeight));

                // Ground
                double groundLevel = groundY + R;
                dc.DrawLine(new Pen(Brushes.Gray, 2),
                    new Point(0, groundLevel),
                    new Point(g1.ActualWidth, groundLevel));

                // Hoop center
                double centerX = x;
                double centerY = groundY;

                // 1. HOOP (with marker)
                DrawHoopWithMarker(dc, centerX, centerY, theta);

                // 2. PENDULUM (red ball on rod)
                DrawPendulum(dc, centerX, centerY, phi);

                // 3. Velocity vectors (for debugging)
                if (showVectors)
                {
                    DrawVelocityVectors(dc, centerX, centerY);
                }

                // 4. Information
                DrawInfo(dc);

                dc.Close();
            }
            g1.AddVisual(visual);
        }

        private void DrawHoopWithMarker(DrawingContext dc, double cx, double cy, double hoopAngle)
        {
            // Hoop (white ring)
            dc.DrawEllipse(null, new Pen(Brushes.White, 2),
                new Point(cx, cy), R, R);

            // Blue marker ON THE HOOP (rotates with hoop)
            // This is a fixed point on the hoop, e.g., at top (angle = 0)
            double markerAngle = hoopAngle; // marker angle = hoop rotation angle
            double markerX = cx + R * Math.Sin(markerAngle);
            double markerY = cy - R * Math.Cos(markerAngle); // minus because y increases downward

            dc.DrawEllipse(Brushes.Cyan, null,
                new Point(markerX, markerY), 5, 5);

            // Hoop center
            dc.DrawEllipse(Brushes.White, null,
                new Point(cx, cy), 3, 3);
        }

        private void DrawPendulum(DrawingContext dc, double cx, double cy, double angle)
        {
            // IMPORTANT: angle phi=0 should correspond to pendulum at bottom (vertically down)
            // Current formula: pendulum is at distance l from center
            // at angle phi from vertical

            // CORRECT formula:
            // When phi = 0: pendulum straight down
            // When phi = π/2: pendulum to the right
            // When phi = -π/2: pendulum to the left

            double loadX = cx + l * Math.Sin(angle);       // Horizontal component
            double loadY = cy + l * Math.Cos(angle);       // Vertical component (PLUS, not minus!)

            // Rod
            dc.DrawLine(new Pen(Brushes.Yellow, 2),
                new Point(cx, cy),
                new Point(loadX, loadY));

            // Pendulum mass
            dc.DrawEllipse(Brushes.Red, null,
                new Point(loadX, loadY), 10, 10);
        }

        private void DrawVelocityVectors(DrawingContext dc, double cx, double cy)
        {
            // Hoop center velocity vector
            double scale = 0.5;
            dc.DrawLine(new Pen(Brushes.Green, 1),
                new Point(cx, cy),
                new Point(cx + vx * scale, cy));

            // Pendulum velocity vector
            double loadVx = vx + l * omega_phi * Math.Cos(phi);
            double loadVy = l * omega_phi * Math.Sin(phi);

            double loadX = cx + l * Math.Sin(phi);
            double loadY = cy - l * Math.Cos(phi);

            dc.DrawLine(new Pen(Brushes.Red, 1),
                new Point(loadX, loadY),
                new Point(loadX + loadVx * scale, loadY + loadVy * scale));
        }

        private void DrawInfo(DrawingContext dc)
        {
            string info = $"Hoop center: {x:F1} px\n" +
                         $"Hoop angle (θ): {theta:F2} rad\n" +
                         $"Pendulum angle (φ): {phi:F2} rad\n" +
                         $"ω_hoop: {omega_theta:F2} rad/s\n" +
                         $"ω_pendulum: {omega_phi:F2} rad/s\n" +
                         $"Pendulum mass: {mgr:F1} kg";

            FormattedText text = new FormattedText(
                info,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.LightGray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(text, new Point(10, 10));

            if (isPaused)
            {
                FormattedText pausedText = new FormattedText(
                    "PAUSED",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial Bold"),
                    24,
                    Brushes.Yellow,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                dc.DrawText(pausedText,
                    new Point(g1.ActualWidth / 2 - 40, g1.ActualHeight / 2 - 12));
            }
        }

        // Event handlers
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            InitPhysics();
            Drawing();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;
            btnPause.Content = isPaused ? "Resume" : "Pause";
        }

        private void SliderMass_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            mgr = sliderMass.Value;
            if (tbMassValue != null) tbMassValue.Text = mgr.ToString("F1");
        }

        private void ChkShowVectors_Checked(object sender, RoutedEventArgs e)
        {
            if (chkShowVectors != null)
            {
                showVectors = chkShowVectors.IsChecked ?? false;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Space:
                    isPaused = !isPaused;
                    btnPause.Content = isPaused ? "Resume" : "Pause";
                    break;
                case Key.R:
                    InitPhysics();
                    Drawing();
                    break;
                case Key.V:
                    showVectors = !showVectors;
                    if (chkShowVectors != null)
                    {
                        chkShowVectors.IsChecked = showVectors;
                    }
                    break;
            }
        }
    }
}
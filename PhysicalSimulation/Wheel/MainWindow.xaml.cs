using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DrawingVisualApp
{
    public partial class MainWindow : Window
    {
        private class Segment
        {
            public Point A;
            public Point B;
            public Segment(Point a, Point b) { A = a; B = b; }
        }

        private struct Edge
        {
            public int A, B;
            public double Rest;
            public Edge(int a, int b, double r) { A = a; B = b; Rest = r; }
        }

        private const int PointCount = 19;
        private const double OuterRadius = 55;

        private const double TimeStep = 1.0 / 60.0;
        private const double SpringStiffness = 750;
        private const double GravityForce = 200;
        private const double Damping = 0.97;

        private const double VertexRadius = 4.0;
        private const double Skin = 0.5;

        private const double MouseSpring = 3000;
        private const double MouseDamping = 25;
        private const double GrabRadius = 12;

        private const double RollingFriction = 0.05;

        private double angularVelocity = 0;
        private const double AngularDamping = 0.995;
        private const double Inertia = 2500;

        private readonly Point[] positions = new Point[PointCount];
        private readonly Vector[] velocities = new Vector[PointCount];
        private readonly Vector[] forces = new Vector[PointCount];

        private readonly List<Edge> edges = new List<Edge>();
        private readonly List<Segment> segments = new List<Segment>();

        private DrawingVisual visual;
        private DispatcherTimer timer;

        private double width;
        private double height;

        private bool dragging;
        private Point mouseWorld;

        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            timer.Tick += Tick;

            Loaded += OnLoaded;

            canvas.MouseLeftButtonDown += MouseDown;
            canvas.MouseLeftButtonUp += MouseUp;
            canvas.MouseMove += MouseMove;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            width = canvas.ActualWidth;
            height = canvas.ActualHeight;
            if (width < 100) width = 900;
            if (height < 100) height = 700;

            InitSoftBody();
            InitSegments();
            timer.Start();
        }

        private void InitSoftBody()
        {
            double cx = width / 2;
            double cy = height / 4;

            positions[0] = new Point(cx, cy);
            velocities[0] = new Vector();

            for (int i = 1; i < PointCount; i++)
            {
                double a = 2 * Math.PI * (i - 1) / (PointCount - 1);
                positions[i] = new Point(
                    cx + OuterRadius * Math.Cos(a),
                    cy + OuterRadius * Math.Sin(a));
                velocities[i] = new Vector();
            }

            edges.Clear();
            double side = 2 * OuterRadius * Math.Sin(Math.PI / (PointCount - 1));

            for (int i = 1; i < PointCount; i++)
                edges.Add(new Edge(0, i, OuterRadius));

            for (int i = 1; i < PointCount; i++)
            {
                int next = (i == PointCount - 1) ? 1 : i + 1;
                edges.Add(new Edge(i, next, side));
            }
        }

        private void InitSegments()
        {
            segments.Clear();

            segments.Add(new Segment(
                new Point(50, height * 0.55),
                new Point(width * 0.45, height * 0.7)));

            segments.Add(new Segment(
                new Point(0, height - 20),
                new Point(width, height - 20)));
        }

        private void Tick(object sender, EventArgs e)
        {
            UpdatePhysics();
            Draw();
        }

        private void UpdatePhysics()
        {
            for (int i = 0; i < PointCount; i++)
                forces[i] = new Vector();

            foreach (Edge e in edges)
            {
                Vector d = positions[e.B] - positions[e.A];
                double len = d.Length;
                if (len < 1e-6) continue;

                Vector dir = d / len;
                double stretch = len - e.Rest;
                Vector f = dir * (SpringStiffness * stretch);

                forces[e.A] += f;
                forces[e.B] -= f;
            }

            for (int i = 0; i < PointCount; i++)
                forces[i] += new Vector(0, GravityForce);

            ApplyMouseForce();

            for (int i = 0; i < PointCount; i++)
            {
                velocities[i] = (velocities[i] + forces[i] * TimeStep) * Damping;
                positions[i] += velocities[i] * TimeStep;
            }

            ResolveVertexSegmentContacts();

            // angular velocity damping
            angularVelocity *= AngularDamping;

            // add rotational velocity to vertices
            Point center = ComputeCenter();
            for (int i = 1; i < PointCount; i++)
            {
                Vector r = positions[i] - center;
                Vector rotVel = new Vector(-r.Y, r.X) * angularVelocity;
                velocities[i] += rotVel * TimeStep;
            }
        }

        private void ResolveVertexSegmentContacts()
        {
            Point center = ComputeCenter();

            for (int i = 1; i < PointCount; i++)
            {
                for (int s = 0; s < segments.Count; s++)
                {
                    Point p = positions[i];
                    Vector v = velocities[i];

                    Segment seg = segments[s];
                    Vector ab = seg.B - seg.A;
                    Vector ap = p - seg.A;

                    double t = Vector.Multiply(ap, ab) / ab.LengthSquared;
                    t = Math.Max(0, Math.Min(1, t));

                    Point closest = seg.A + ab * t;
                    Vector diff = p - closest;
                    double dist = diff.Length;

                    if (dist >= VertexRadius)
                        continue;

                    Vector normal = diff / Math.Max(dist, 1e-6);
                    p += normal * (VertexRadius - dist + Skin);

                    double vn = Vector.Multiply(v, normal);
                    if (vn < 0)
                        v -= normal * vn;

                    // ===== REAL ROLLING =====
                    Vector tangent = new Vector(-normal.Y, normal.X);
                    Vector r = p - center;

                    Vector rotVel = new Vector(-r.Y, r.X) * angularVelocity;
                    double relSpeed = Vector.Multiply(v + rotVel, tangent);

                    double impulse = relSpeed * RollingFriction;

                    v -= tangent * impulse;
                    angularVelocity -= Vector.Multiply(r, tangent) * impulse / Inertia;

                    positions[i] = p;
                    velocities[i] = v;
                }
            }
        }

        private Point ComputeCenter()
        {
            double x = 0, y = 0;
            for (int i = 0; i < PointCount; i++)
            {
                x += positions[i].X;
                y += positions[i].Y;
            }
            return new Point(x / PointCount, y / PointCount);
        }

        private void ApplyMouseForce()
        {
            if (!dragging) return;
            Vector d = mouseWorld - positions[0];
            forces[0] += d * MouseSpring - velocities[0] * MouseDamping;
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.GetPosition(canvas) - positions[0]).Length < GrabRadius)
            {
                dragging = true;
                mouseWorld = e.GetPosition(canvas);
                canvas.CaptureMouse();
            }
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
                mouseWorld = e.GetPosition(canvas);
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
            canvas.ReleaseMouseCapture();
        }

        private void Draw()
        {
            DrawingContext dc = visual.RenderOpen();

            dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));

            foreach (var s in segments)
                dc.DrawLine(new Pen(Brushes.Orange, 2), s.A, s.B);

            foreach (var e in edges)
                dc.DrawLine(new Pen(Brushes.Gray, 1.3), positions[e.A], positions[e.B]);

            for (int i = 0; i < PointCount; i++)
            {
                Brush b = (i == 0) ? Brushes.Red : Brushes.White;
                double r = (i == 0) ? 6 : VertexRadius;
                dc.DrawEllipse(b, null, positions[i], r, r);
            }

            dc.Close();
            canvas.RemoveVisual(visual);
            canvas.AddVisual(visual);
        }
    }
}
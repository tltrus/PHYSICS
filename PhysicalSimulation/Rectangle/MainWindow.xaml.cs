using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DrawingVisualApp
{
    public partial class MainWindow : Window
    {
        // ============================ APPLICATION PARAMETERS ============================

        // General simulation parameters (now fields instead of constants)
        private int PARTICLE_COUNT = 100;                 // Number of particles
        private const double SPRING_CONSTANT = 700;           // Spring stiffness coefficient
        private const double TIME_STEP = 0.02;                // Time step for simulation
        private double INTERACTION_RADIUS = 50;         // Maximum interaction radius
        private const double GRAVITY = 9.8 * 8;               // Gravity acceleration (INCREASED for faster falling)

        // Particle parameters
        private const double PARTICLE_RADIUS = 5;             // Particle rendering radius
        private const double INITIAL_SPACING = 30;           // Initial distance between particles
        private const double INITIAL_VELOCITY_RANGE = 8;     // Initial velocity range

        // Wall parameters
        private const double WALL_THICKNESS = 10;             // Wall rendering thickness
        private Point wallStart = new Point(0, 0);           // Wall start point
        private Point wallEnd = new Point(0, 0);             // Wall end point

        // Boundary parameters
        private const double BOUNDARY_DAMPING = 0.8;         // Damping on boundary collision
        private const double GROUND_FRICTION = 0.9;          // Ground friction
        private const double GROUND_HEIGHT = 100;            // Ground height from bottom
        private double VELOCITY_DAMPING = 0.995;       // General velocity damping

        // Physics parameters
        private const double FORCE_LIMIT = 1000;             // Maximum force value
        private const double DAMPING_COEFFICIENT = 0.1;      // Damping coefficient
        private const double COLLISION_DAMPING = 0.8;        // Damping on wall collision

        // Optimization parameters (Spatial Hashing)
        private const double CELL_SIZE = 60;                 // Spatial grid cell size
        private const int HASH_MULTIPLIER = 1000;            // Coordinate hash multiplier

        // Rendering parameters
        private const int CONNECTION_LINE_WIDTH = 1;         // Connection line thickness
        private const int GROUND_LINE_WIDTH = 4;             // Ground line thickness

        // Colors
        private readonly Color PARTICLE_COLOR = Colors.White;
        private readonly Color CONNECTION_COLOR = Colors.Green;
        private readonly Color BACKGROUND_COLOR = Colors.Black;
        private readonly Color GROUND_COLOR = Colors.DarkGreen;
        private readonly Color WALL_COLOR = Colors.Orange;
        private readonly Color TEXT_COLOR = Colors.White;

        // FPS counter parameters
        private const double FPS_UPDATE_INTERVAL = 1.0;      // FPS update interval (seconds)

        // ============================ SIMULATION VARIABLES ============================

        private DispatcherTimer timer;
        private Random rnd = new Random();
        private double width, height;

        // WriteableBitmap for rendering
        private WriteableBitmap writeableBitmap;

        // Data arrays
        private double[] forcesX, forcesY;
        private double[] positionsX, positionsY;
        private double[] velocitiesX, velocitiesY;
        private double[,] initialDistances;

        // Performance
        private int frameCount = 0;
        private DateTime lastFpsUpdate = DateTime.Now;
        private double currentFps = 0;
        private Stopwatch stopwatch = new Stopwatch();

        // Spatial hashing for optimization
        private Dictionary<int, List<int>> spatialGrid = new Dictionary<int, List<int>>();

        // Pause flag
        private bool isPaused = false;

        public MainWindow()
        {
            InitializeComponent();

            // Load initial values from TextBox
            LoadParametersFromUI();

            InitializeSimulation();
        }

        private void LoadParametersFromUI()
        {
            // Load values from UI
            if (int.TryParse(txtParticleCount.Text, out int particleCount))
                PARTICLE_COUNT = Math.Max(1, Math.Min(1000, particleCount));

            if (double.TryParse(txtInteractionRadius.Text, out double interactionRadius))
                INTERACTION_RADIUS = Math.Max(10, Math.Min(200, interactionRadius));

            if (double.TryParse(txtVelocityDamping.Text, out double velocityDamping))
                VELOCITY_DAMPING = Math.Max(0.9, Math.Min(1.0, velocityDamping));
        }

        private void InitializeSimulation()
        {
            width = (int)g.Width;
            height = (int)g.Height;

            // Initialize WriteableBitmap
            writeableBitmap = new WriteableBitmap(
                (int)width,
                (int)height,
                96, 96,
                PixelFormats.Bgra32,
                null);

            // Set WriteableBitmap as image source
            image.Source = writeableBitmap;

            // Initialize wall
            wallStart = new Point(0, height / 2);
            wallEnd = new Point(250, height / 2 + 40);

            // Setup timer
            timer = new DispatcherTimer();
            timer.Tick += OnTimerTick;
            timer.Interval = TimeSpan.FromMilliseconds(10);

            InitializeParticles();
            timer.Start();

            UpdateStatus();
        }

        private void InitializeParticles()
        {
            // Initialize arrays
            positionsX = new double[PARTICLE_COUNT];
            positionsY = new double[PARTICLE_COUNT];
            velocitiesX = new double[PARTICLE_COUNT];
            velocitiesY = new double[PARTICLE_COUNT];
            forcesX = new double[PARTICLE_COUNT];
            forcesY = new double[PARTICLE_COUNT];
            initialDistances = new double[PARTICLE_COUNT, PARTICLE_COUNT];

            // Grid parameters
            int gridCols = (int)Math.Ceiling(Math.Sqrt(PARTICLE_COUNT));
            int gridRows = (int)Math.Ceiling((double)PARTICLE_COUNT / gridCols);

            // Distribute particles in grid
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                int row = i / gridCols;
                int col = i % gridCols;

                positionsX[i] = width * 0.25 + (col - gridCols * 0.5) * INITIAL_SPACING;
                positionsY[i] = 50 + row * INITIAL_SPACING;

                // Random velocities
                velocitiesX[i] = (rnd.NextDouble() - 0.5) * INITIAL_VELOCITY_RANGE;
                velocitiesY[i] = (rnd.NextDouble() - 0.5) * INITIAL_VELOCITY_RANGE;
            }

            // Calculate initial distances
            CalculateInitialDistances();
        }

        private void CalculateInitialDistances()
        {
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                for (int j = 0; j < PARTICLE_COUNT; j++)
                {
                    if (i != j)
                    {
                        double dx = positionsX[i] - positionsX[j];
                        double dy = positionsY[i] - positionsY[j];
                        initialDistances[i, j] = Math.Sqrt(dx * dx + dy * dy);
                    }
                }
            }
        }

        private void UpdateSpatialGrid()
        {
            spatialGrid.Clear();

            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                int cellX = (int)(positionsX[i] / CELL_SIZE);
                int cellY = (int)(positionsY[i] / CELL_SIZE);
                int cellHash = cellX * HASH_MULTIPLIER + cellY;

                if (!spatialGrid.ContainsKey(cellHash))
                    spatialGrid[cellHash] = new List<int>();

                spatialGrid[cellHash].Add(i);
            }
        }

        private List<int> GetNearbyParticles(int particleIndex)
        {
            List<int> nearby = new List<int>();
            int cellX = (int)(positionsX[particleIndex] / CELL_SIZE);
            int cellY = (int)(positionsY[particleIndex] / CELL_SIZE);

            // Check neighboring cells
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int hash = (cellX + dx) * HASH_MULTIPLIER + (cellY + dy);
                    if (spatialGrid.TryGetValue(hash, out var particles))
                    {
                        nearby.AddRange(particles);
                    }
                }
            }

            return nearby;
        }

        private void CalculateForces()
        {
            // Reset forces
            Array.Clear(forcesX, 0, forcesX.Length);
            Array.Clear(forcesY, 0, forcesY.Length);

            // Update spatial grid
            UpdateSpatialGrid();

            // Add gravity
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                forcesY[i] += GRAVITY;
            }

            // Calculate interaction forces
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                var nearby = GetNearbyParticles(i);

                foreach (int j in nearby)
                {
                    if (j <= i) continue; // Avoid double calculations

                    double dx = positionsX[i] - positionsX[j];
                    double dy = positionsY[i] - positionsY[j];
                    double distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared > INTERACTION_RADIUS * INTERACTION_RADIUS || distanceSquared < 0.01)
                        continue;

                    double distance = Math.Sqrt(distanceSquared);

                    // Spring force
                    double initialDist = initialDistances[i, j];
                    double displacement = distance - initialDist;
                    double springForce = -SPRING_CONSTANT * displacement;

                    // Damping
                    double relativeVelX = velocitiesX[i] - velocitiesX[j];
                    double relativeVelY = velocitiesY[i] - velocitiesY[j];
                    double damping = -DAMPING_COEFFICIENT * (dx * relativeVelX + dy * relativeVelY) / (distance + 0.001);

                    double totalForce = springForce + damping;

                    // Limit force
                    totalForce = Math.Max(-FORCE_LIMIT, Math.Min(FORCE_LIMIT, totalForce));

                    // Apply forces
                    if (distance > 0.1)
                    {
                        double forceX = totalForce * dx / distance;
                        double forceY = totalForce * dy / distance;

                        forcesX[i] += forceX;
                        forcesY[i] += forceY;
                        forcesX[j] -= forceX;
                        forcesY[j] -= forceY;
                    }
                }
            }
        }

        private bool CheckWallCollision(int particleIndex, out Point collisionPoint, out Vector wallNormal)
        {
            collisionPoint = new Point();
            wallNormal = new Vector();

            double px = positionsX[particleIndex];
            double py = positionsY[particleIndex];

            // Wall vector
            Vector wallVec = new Vector(wallEnd.X - wallStart.X, wallEnd.Y - wallStart.Y);
            double wallLength = wallVec.Length;

            if (wallLength < 0.001) return false;

            // Normals
            Vector wallDir = new Vector(wallVec.X / wallLength, wallVec.Y / wallLength);
            wallNormal = new Vector(-wallDir.Y, wallDir.X);

            // Projection on wall
            Vector toParticle = new Vector(px - wallStart.X, py - wallStart.Y);
            double projection = Vector.Multiply(toParticle, wallDir);

            // Closest point on wall
            double closestX, closestY;

            if (projection <= 0)
            {
                closestX = wallStart.X;
                closestY = wallStart.Y;
            }
            else if (projection >= wallLength)
            {
                closestX = wallEnd.X;
                closestY = wallEnd.Y;
            }
            else
            {
                closestX = wallStart.X + projection * wallDir.X;
                closestY = wallStart.Y + projection * wallDir.Y;
            }

            // Distance to wall
            double distance = Math.Sqrt(Math.Pow(px - closestX, 2) + Math.Pow(py - closestY, 2));

            if (distance < PARTICLE_RADIUS + WALL_THICKNESS * 0.5)
            {
                // Check side
                Vector toClosest = new Vector(closestX - px, closestY - py);
                double dot = Vector.Multiply(toClosest, wallNormal);

                if (dot < 0)
                {
                    collisionPoint = new Point(closestX, closestY);
                    return true;
                }
            }

            return false;
        }

        private void HandleBoundaryCollisions()
        {
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                // Left boundary
                if (positionsX[i] < PARTICLE_RADIUS)
                {
                    positionsX[i] = PARTICLE_RADIUS;
                    velocitiesX[i] = -velocitiesX[i] * BOUNDARY_DAMPING;
                }
                // Right boundary
                else if (positionsX[i] > width - PARTICLE_RADIUS)
                {
                    positionsX[i] = width - PARTICLE_RADIUS;
                    velocitiesX[i] = -velocitiesX[i] * BOUNDARY_DAMPING;
                }

                // Top boundary
                if (positionsY[i] < PARTICLE_RADIUS)
                {
                    positionsY[i] = PARTICLE_RADIUS;
                    velocitiesY[i] = -velocitiesY[i] * BOUNDARY_DAMPING;
                }
                // Bottom boundary (ground)
                else if (positionsY[i] > height - GROUND_HEIGHT)
                {
                    positionsY[i] = height - GROUND_HEIGHT;
                    velocitiesY[i] = -velocitiesY[i] * BOUNDARY_DAMPING;
                    velocitiesX[i] *= GROUND_FRICTION;
                }

                // Wall collision
                if (CheckWallCollision(i, out Point collisionPoint, out Vector wallNormal))
                {
                    double pushDistance = PARTICLE_RADIUS + WALL_THICKNESS * 0.5;
                    positionsX[i] = collisionPoint.X - wallNormal.X * pushDistance;
                    positionsY[i] = collisionPoint.Y - wallNormal.Y * pushDistance;

                    double dot = velocitiesX[i] * wallNormal.X + velocitiesY[i] * wallNormal.Y;
                    velocitiesX[i] -= 2 * dot * wallNormal.X;
                    velocitiesY[i] -= 2 * dot * wallNormal.Y;

                    velocitiesX[i] *= COLLISION_DAMPING;
                    velocitiesY[i] *= COLLISION_DAMPING;
                }
            }
        }

        private void UpdateFPS()
        {
            frameCount++;
            var now = DateTime.Now;
            if ((now - lastFpsUpdate).TotalSeconds >= FPS_UPDATE_INTERVAL)
            {
                currentFps = frameCount / (now - lastFpsUpdate).TotalSeconds;
                frameCount = 0;
                lastFpsUpdate = now;

                // Update information in UI
                UpdateInfoText();
            }
        }

        private void UpdateInfoText()
        {
            infoText.Text = $"Particles: {PARTICLE_COUNT}\n" +
                           $"FPS: {currentFps:F1}\n" +
                           $"Frame: {stopwatch.Elapsed.TotalMilliseconds:F2} ms\n" +
                           $"Interactions: {INTERACTION_RADIUS}\n" +
                           $"Damping: {VELOCITY_DAMPING:F3}";
        }

        private void UpdateStatus()
        {
            txtStatus.Text = isPaused ? "Simulation Paused" : "Simulation Running";
            btnPause.Content = isPaused ? "Resume" : "Pause";
            btnPause.Background = isPaused ? new SolidColorBrush(Color.FromRgb(46, 125, 50)) :
                                             new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (isPaused) return;

            stopwatch.Restart();
            UpdateFPS();
            CalculateForces();

            // Motion integration
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                velocitiesX[i] += forcesX[i] * TIME_STEP;
                velocitiesY[i] += forcesY[i] * TIME_STEP;

                // Damping
                velocitiesX[i] *= VELOCITY_DAMPING;
                velocitiesY[i] *= VELOCITY_DAMPING;

                positionsX[i] += velocitiesX[i] * TIME_STEP;
                positionsY[i] += velocitiesY[i] * TIME_STEP;
            }

            HandleBoundaryCollisions();
            Render();
            stopwatch.Stop();
        }

        private void Render()
        {
            try
            {
                writeableBitmap.Lock();

                // Use WriteableBitmapEx methods for drawing
                writeableBitmap.Clear(BACKGROUND_COLOR);

                // Draw ground line
                writeableBitmap.DrawLineAa(
                    (int)0,
                    (int)(height - (GROUND_HEIGHT - 2)),
                    (int)width,
                    (int)(height - (GROUND_HEIGHT - 2)),
                    GROUND_COLOR,
                    GROUND_LINE_WIDTH);

                // Draw wall
                writeableBitmap.DrawLineAa(
                               (int)wallStart.X,
                               (int)wallStart.Y,
                               (int)wallEnd.X,
                               (int)wallEnd.Y,
                               WALL_COLOR,
                               (int)(WALL_THICKNESS * 0.5));

                // Draw connections
                DrawConnections();

                // Draw particles
                for (int i = 0; i < PARTICLE_COUNT; i++)
                {
                    writeableBitmap.FillEllipseCentered(
                        (int)positionsX[i],
                        (int)positionsY[i],
                        (int)PARTICLE_RADIUS,
                        (int)PARTICLE_RADIUS,
                        PARTICLE_COLOR);
                }

                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight));
            }
            finally
            {
                writeableBitmap.Unlock();
            }
        }

        private void DrawConnections()
        {
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                var nearby = GetNearbyParticles(i);

                foreach (int j in nearby)
                {
                    if (j <= i) continue;

                    double dx = positionsX[i] - positionsX[j];
                    double dy = positionsY[i] - positionsY[j];
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance < INTERACTION_RADIUS)
                    {
                        writeableBitmap.DrawLineAa(
                            (int)positionsX[i],
                            (int)positionsY[i],
                            (int)positionsX[j],
                            (int)positionsY[j],
                            CONNECTION_COLOR,
                            CONNECTION_LINE_WIDTH);
                    }
                }
            }
        }

        // Button handlers
        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            // Stop timer
            timer?.Stop();

            // Load new parameters from UI
            LoadParametersFromUI();

            // Update TextBox with applied values
            txtParticleCount.Text = PARTICLE_COUNT.ToString();
            txtInteractionRadius.Text = INTERACTION_RADIUS.ToString("F1");
            txtVelocityDamping.Text = VELOCITY_DAMPING.ToString("F3");

            // Restart simulation
            InitializeParticles();

            // Start timer
            timer?.Start();
            isPaused = false;
            UpdateStatus();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;
            UpdateStatus();
        }

        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();
            base.OnClosed(e);
        }
    }
}
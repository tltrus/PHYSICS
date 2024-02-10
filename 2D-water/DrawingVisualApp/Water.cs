using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
    class Water
    {
        class Particle
		{
			public Vector2D Position;
			public Vector2D Velocity;
			public float Orientation;
			public int r;

			public Particle(Random rnd, Vector2D position, Vector2D velocity, float orientation)
			{
				Position = position;
				Velocity = velocity;
				Orientation = orientation;
				r = rnd.Next(5);
			}
		}

		struct WaterColumn
		{
            public float TargetHeight;
			public float Height;
			public float X;
			public float Speed;

			public void Update(float dampening, float tension)
			{
				float y = TargetHeight - Height;
				Speed += tension * y - Speed * dampening;
				Height += Speed;
			}
		}

		WaterColumn[] columns;
		public float Tension = 0.025f;
		public float Dampening = 0.025f;
		public float Spread = 0.25f;

		List<Particle> particles = new List<Particle>();

		public double x, y;

		private int screenWidth, screenHeight;
		public Random rnd;
		int scale = 6;

		public Water(Random rnd, int screenWidth, int screenHeight)
        {
			this.rnd = rnd;
			this.screenWidth = screenWidth;
			this.screenHeight = screenHeight;

			columns = new WaterColumn[screenWidth / scale];

			for (int i = 0; i < columns.Length; i++)
			{
				columns[i] = new WaterColumn()
				{
					Height = 240,
					TargetHeight = 240,
					Speed = 0,
					X = i * scale
				};
			}
		}

		void UpdateParticle(Particle particle)
		{
			const float Gravity = 0.3f;
			particle.Velocity.Y += Gravity;
			particle.Position += particle.Velocity;
		}

		public void Update()
		{
			for (int i = 0; i < columns.Length; i++)
				columns[i].Update(Dampening, Tension);

			float[] lDeltas = new float[columns.Length];
			float[] rDeltas = new float[columns.Length];

			// do some passes where columns pull on their neighbours
			for (int j = 0; j < 8; j++)
			{
				for (int i = 0; i < columns.Length; i++)
				{
					if (i > 0)
					{
						lDeltas[i] = Spread * (columns[i].Height - columns[i - 1].Height);
						columns[i - 1].Speed += lDeltas[i];
					}
					if (i < columns.Length - 1)
					{
						rDeltas[i] = Spread * (columns[i].Height - columns[i + 1].Height);
						columns[i + 1].Speed += rDeltas[i];
					}
				}

				for (int i = 0; i < columns.Length; i++)
				{
					if (i > 0)
						columns[i - 1].Height += lDeltas[i];
					if (i < columns.Length - 1)
						columns[i + 1].Height += rDeltas[i];
				}
			}

			// PARTICLE
			foreach (var particle in particles)
				UpdateParticle(particle);

			// delete particles that are off-screen or under water
			particles = particles.Where(x => x.Position.X >= 0 && x.Position.X <= 800 && x.Position.Y - 5 <= screenHeight).ToList();
		}

		public int GetIndex(double x) => (int)x / scale;
		private int GetWaveHeight()
        {
			var i = GetIndex(x);
			var y = columns[i].Height;

			return (int)y;
        }

		public void Splash(float speed)
		{
			var index = GetIndex(x);

			if (index >= 0 && index < columns.Length)
				columns[index].Speed = speed;

			CreateSplashParticles(x, speed);
		}

		private void CreateSplashParticles(double xPosition, float speed)
		{
			int y = GetWaveHeight();
			if (speed > 60)
			{
				for (int i = 0; i < speed / 8; i++)
				{
					Vector2D pos = new Vector2D(x, y);
					Vector2D vel = Vector2D.Random2D(rnd, 0, -3.14);
					vel.Mult(rnd.Next(5));
					particles.Add(new Particle(rnd, pos, vel, 0));
				}
			}
		}


		public void Draw(DrawingContext dc)
		{
			PointCollection points = new PointCollection();

			for (int i = 0; i < columns.Length; i++)
            {
				var x = columns[i].X;
				var y = columns[i].Height;
				var p = new Point(x, y);
				points.Add(p);
			}
			points.Add(new Point(screenWidth, screenHeight));

			StreamGeometry streamGeometry = new StreamGeometry();
			using (StreamGeometryContext geometryContext = streamGeometry.Open())
			{
				Point p0 = new Point(0, screenHeight);
				geometryContext.BeginFigure(p0, true, true);
				geometryContext.PolyLineTo(points, true, true);
			}
			dc.DrawGeometry(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1), streamGeometry);

			foreach (var particle in particles)
            {
				var p = new Point(particle.Position.X, particle.Position.Y);
				dc.DrawEllipse(Brushes.LightBlue, null, p, particle.r, particle.r);
			}
				
		}
	}
}

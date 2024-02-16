using System.Windows;

namespace BouncingBall
{
    class Ball
    {
        public Vector position;
        public Vector velocity, acc;
        public int R = 10;

        int width, height;


        public Ball(int width, int height)
        {
            position = new Vector(R + 5, height / 2);
            velocity = new Vector(10, 0);
            acc = new Vector(0, 0);

            this.width = width;
            this.height = height;
        }

        public void AddForce(Vector force) => acc += force;

        public bool isFloorContact() => position.Y > height - R;

        public void Update()
        {
            var с = -0.9;

            if (position.X > width - R)
            {
                position.X = width - R;
                velocity.X *= с;
            }
            else if (position.X < R)
            {
                position.X = R;
                velocity.X *= с;
            }
            if (position.Y > height - R)
            {
                position.Y = height - R;
                velocity.Y *= с;
            }

            velocity += acc;
            position += velocity;

            acc *= 0;
        }
    }
}

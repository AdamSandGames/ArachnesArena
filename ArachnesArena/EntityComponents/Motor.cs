using Microsoft.Xna.Framework;
using System;

namespace ArachnesArena
{
    public class Motor : ECSComponent
    {
        public float Speed = 1f;
        public Vector3 Direction = Vector3.Zero;
        private SimpleMap gameMap;
        public Motor(ECSEntity attachee, SimpleMap map)
            : base(attachee)
        {
            gameMap = map;
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 velocity = Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (gameMap.GetHeightAt(parent.transform.Position + velocity) >= 0)
            {
                if (Math.Abs((parent.transform.Position + velocity).Y - gameMap.GetHeightAt(parent.transform.Position + velocity)) > 0.08f)
                {
                    parent.transform.Translate(Vector3.Up * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds *
                       (((parent.transform.Position + velocity).Y - gameMap.GetHeightAt(parent.transform.Position + velocity) > 0) ? -1 : 1));
                }
                else
                {
                    parent.transform.Translate(velocity);
                    parent.transform.Position = new Vector3(
                        parent.transform.Position.X,
                        gameMap.GetHeightAt(parent.transform.Position),
                        parent.transform.Position.Z);
                }
            }
            else
            {
                StopCommand stop = new StopCommand();
                stop.execute(parent);
            }
        }

        public void Impulse(Vector3 dir, float spd = 0f)
        {
            // TEMP - locks vertical movement TODO
            dir = new Vector3(dir.X, parent.transform.Position.Y, dir.Z);
            // ~TEMP
            if (dir.Length() > 0)
            {
                dir.Normalize();
                Direction = dir;
            }
            else
            {
                Direction = Vector3.Zero;
            }
            Speed = spd;
        }

        public void Stop()
        {
            Direction = Vector3.Zero;
            Speed = 1f;
        }



    }
}

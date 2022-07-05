using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ArachnesArena
{
    public class Camera : GameComponent
    {
        private Game1 myGame;
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Direction { get; set; } = Vector3.Forward;
        public Vector3 Up { get; } = Vector3.Up;

        private float FieldOfView { get; set; } = MathHelper.PiOver4;
        //REM private float FocalLength { get; set; } = 1f;
        private float AspectRatio { get; set; }
        private float nearPlane = 0.5f;
        private float farPlane = 200f;

        public Camera(Game1 game, Vector3 pos, Vector3 target, Vector3 up)
                : base(game)
        {
            myGame = game;
            Position = pos;
            Up = up;
            Direction = target - pos;
            AspectRatio = (float)Game.Window.ClientBounds.Width / Game.Window.ClientBounds.Height;
            FieldOfView = MathHelper.PiOver2 * 0.5f;
            //REM FocalLength = 1f;
            View = Matrix.CreateLookAt(Position, Position + Direction, Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, nearPlane, farPlane);
        }
        public override void Initialize()
        {

            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            Vector3 moveDir = Vector3.Zero;

            if (myGame.mouseState_.X < myGame.screenWidth_ * 1 / 50 || myGame.mouseState_.X > myGame.screenWidth_ * 49 / 50 ||
                myGame.mouseState_.Y < myGame.screenHeight_ * 1 / 50 || myGame.mouseState_.Y > myGame.screenHeight_ * 49 / 50)
            {
                if (myGame.mouseState_.X < myGame.screenWidth_ * 1 / 50)
                {
                    moveDir += new Vector3(Direction.Z, 0, -Direction.X);
                }
                else if (myGame.mouseState_.X > myGame.screenWidth_ * 49 / 50)
                {
                    moveDir -= new Vector3(Direction.Z, 0, -Direction.X);
                }
                if (myGame.mouseState_.Y < myGame.screenHeight_ * 1 / 50)
                {
                    moveDir += Direction * new Vector3(1, 0, 1);
                }
                else if (myGame.mouseState_.Y > myGame.screenHeight_ * 49 / 50)
                {
                    moveDir -= Direction * new Vector3(1, 0, 1);
                }
            }
            if (myGame.keyboardState_.IsKeyDown(Keys.U))
                moveDir += new Vector3(0, 1, 0);
            if (myGame.keyboardState_.IsKeyDown(Keys.J))
                moveDir -= new Vector3(0, 1, 0);
            if (moveDir.Length() > 0f)
                moveDir.Normalize();
            Position += 5f * moveDir * (float)gameTime.ElapsedGameTime.TotalSeconds;

            View = Matrix.CreateLookAt(Position, Position + Direction, Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, nearPlane, farPlane);
            base.Update(gameTime);
        }

        public Vector4 GetCamPos4()
        {
            return new Vector4(Position, 1f);
        }
        public Vector4 GetCamDir4()
        {
            return new Vector4(Direction, 1f);
        }
        public Vector4 GetViewVector4()
        {
            Vector3 v = Vector3.Transform(Direction - Position, Matrix.CreateRotationY(0));
            return new Vector4(v, 1f);
        }
        public void SetView()
        {
            View = Matrix.CreateLookAt(Position, Position + Direction, Up);
        }
    }
}
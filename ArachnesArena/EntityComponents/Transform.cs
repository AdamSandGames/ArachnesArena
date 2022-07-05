using Microsoft.Xna.Framework;

namespace ArachnesArena
{
    public class Transform : ECSComponent
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public float Scale { get; set; } = 0;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Matrix TransMat { get; set; } = Matrix.Identity;
        public Matrix RotatMat { get; set; } = Matrix.Identity;
        public Matrix WorldMat { get; set; } = Matrix.Identity;

        public Transform(ECSEntity attachee, Vector3 pos, float scal, Quaternion rot)
            : base(attachee)
        {
            this.Position = pos;
            this.Scale = scal;
            this.Rotation = rot;
            UpdateMatrix();
        }
        public void Translate(Vector3 distance)
        {
            Position += distance;
            UpdateMatrix();
        }
        public void Rotate(Quaternion spin)
        {
            Rotation = Quaternion.Multiply(Rotation, spin);
            UpdateMatrix();
        }
        public void Rescale(float scale)
        {
            this.Scale = scale;
            UpdateMatrix();
        }
        private void UpdateMatrix()
        {
            TransMat = Matrix.CreateTranslation(Position);
            RotatMat = Matrix.CreateFromQuaternion(Rotation);

            WorldMat = RotatMat * TransMat;
        }

        public override void Update(GameTime gameTime)
        {
            // TODO MUST MATCH MAP SIZE
            // TODO Move this
            if (Position.X < 0)
            {
                Position = new Vector3(0, Position.Y, Position.Z);
            }
            if (Position.X > 20)
            {
                Position = new Vector3(20, Position.Y, Position.Z);
            }
            if (Position.Z < 0)
            {
                Position = new Vector3(Position.X, Position.Y, 0);
            }
            if (Position.Z > 20)
            {
                Position = new Vector3(Position.X, Position.Y, 20);
            }
            // Skip
        }

        public float DistanceTo(ECSEntity target)
        {
            var diff = Position - target.transform.Position;
            return diff.Length();
        }
    }
}

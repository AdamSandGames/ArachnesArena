
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArachnesArena
{
    public class SpriteRenderer : ECSComponent
    {
        public Texture2D texture;
        public Texture2D floorTex;
        private Color color = Color.Transparent;
        private bool _hasTxt = false;
        public bool showCircle = false;

        private Camera povCam;

        private VertexPositionTexture[] quadVerts = new VertexPositionTexture[4];
        private VertexPositionTexture[] floorVerts = new VertexPositionTexture[4];

        public SpriteRenderer(ECSEntity attachee, Texture2D mainTexture, Texture2D floorTexture, Camera cam)
            : base(attachee)
        {
            this.texture = mainTexture;
            this.floorTex = floorTexture;
            _hasTxt = true;
            povCam = cam;

            quadVerts[0] = new VertexPositionTexture(new Vector3(-0.5f, 1f, 0f), new Vector2(1, 1));
            quadVerts[1] = new VertexPositionTexture(new Vector3(0.5f, 1f, 0f), new Vector2(1, 0));
            quadVerts[2] = new VertexPositionTexture(new Vector3(-0.5f, 0f, 0f), new Vector2(0, 1));
            quadVerts[3] = new VertexPositionTexture(new Vector3(0.5f, 0f, 0f), new Vector2(0, 0));

            floorVerts[0] = new VertexPositionTexture(new Vector3(-0.5f, 0.05f, -0.5f), new Vector2(0, 0));
            floorVerts[1] = new VertexPositionTexture(new Vector3(0.5f, 0.05f, -0.5f), new Vector2(0, 1));
            floorVerts[2] = new VertexPositionTexture(new Vector3(-0.5f, 0.05f, 0.5f), new Vector2(1, 0));
            floorVerts[3] = new VertexPositionTexture(new Vector3(0.5f, 0.05f, 0.5f), new Vector2(1, 1));

            Game1.sprites.Add(this);
        }

        public void Draw(BasicEffect effect, GraphicsDevice gd)
        {
            if (!_hasTxt)
            {
                return;
            }
            Transform tfm = parent.FindComponent<Transform>();
            Matrix scaleMat = Matrix.CreateScale(tfm.Scale);
            Matrix billWorldMat = Matrix.CreateConstrainedBillboard(tfm.Position, povCam.Position, Vector3.Up, povCam.Direction, null);
            Matrix finalMat = Matrix.Identity * scaleMat;
            finalMat.Right = -finalMat.Right;
            finalMat *= billWorldMat;

            effect.World = finalMat;
            effect.TextureEnabled = true;
            effect.Texture = texture;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, quadVerts, 0, 2);
            }


            // IF SELECTED
            if (showCircle)
            {
                effect.Texture = floorTex;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, floorVerts, 0, 2);
                }
            }
        }
        public override void Update(GameTime gameTime)
        {
            // Skip
        }
    }
}

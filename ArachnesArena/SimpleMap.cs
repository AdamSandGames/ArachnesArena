using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArachnesArena
{
    public class SimpleMap : DrawableGameComponent
    {
        private VertexPositionColor[] vpc;
        private Color mapCol = new Color(100, 150, 100, 255); //255
        private Color mapCol2 = new Color(120, 120, 100, 255); //255
        private BasicEffect effect;

        public int[,] elevationField;
        public float levelStep;
        public SimpleMap(Game game, BasicEffect beff, float levStep = 0.5f, int[,] elev = null)
            : base(game)
        {
            levelStep = levStep;
            elevationField = elev;
            if (elev == null)
            {
                vpc = ElevationToVertexPositionColor(RandomElevations(20)); // Default Random Map
            }
            else
            {
                vpc = ElevationToVertexPositionColor(elev);
            }
            effect = beff;
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            VertexBuffer vb = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), vpc.Length, BufferUsage.None);
            vb.SetData(vpc.ToArray());
            GraphicsDevice.SetVertexBuffer(vb);
            effect.VertexColorEnabled = true;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>
                    (PrimitiveType.TriangleList, vpc, 0, vpc.Length / 3);

            }
        }

        private VertexPositionColor[] ElevationToVertexPositionColor(int[,] heights)
        {
            List<VertexPositionColor> verList = new List<VertexPositionColor>();
            VertexPositionColor[] verts;

            for (int i = 0; i <= heights.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= heights.GetUpperBound(1); j++)
                {

                    if (heights[i, j] >= 0)
                    {
                        // Create square's vertices
                        // Top Face
                        verList.Add(new VertexPositionColor(new Vector3(j + 0, heights[i, j] * levelStep, i + 0), mapCol));
                        verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j] * levelStep, i + 0), mapCol));
                        verList.Add(new VertexPositionColor(new Vector3(j + 0, heights[i, j] * levelStep, i + 1), mapCol));

                        verList.Add(new VertexPositionColor(new Vector3(j + 0, heights[i, j] * levelStep, i + 1), mapCol));
                        verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j] * levelStep, i + 0), mapCol));
                        verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j] * levelStep, i + 1), mapCol));

                    }

                    // Right Face
                    if (j < heights.GetUpperBound(1))
                    {
                        if (heights[i, j] != heights[i, j + 1])
                        {
                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j] * levelStep, i + 1), mapCol2));
                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j] * levelStep, i + 0), mapCol2));
                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j + 1] * levelStep, i + 1), mapCol2));

                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j + 1] * levelStep, i + 1), mapCol2));
                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j] * levelStep, i + 0), mapCol2));
                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j + 1] * levelStep, i + 0), mapCol2));
                        }
                    }

                    // Front Face
                    if (i < heights.GetUpperBound(0))
                    {
                        if (heights[i, j] != heights[i + 1, j])
                        {
                            verList.Add(new VertexPositionColor(new Vector3(j + 0, heights[i, j] * levelStep, i + 1), mapCol2));
                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j] * levelStep, i + 1), mapCol2));
                            verList.Add(new VertexPositionColor(new Vector3(j + 0, heights[i + 1, j] * levelStep, i + 1), mapCol2));

                            verList.Add(new VertexPositionColor(new Vector3(j + 0, heights[i + 1, j] * levelStep, i + 1), mapCol2));
                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i, j] * levelStep, i + 1), mapCol2));
                            verList.Add(new VertexPositionColor(new Vector3(j + 1, heights[i + 1, j] * levelStep, i + 1), mapCol2));
                        }
                    }

                }
            }

            verts = new VertexPositionColor[verList.Count];
            for (int p = 0; p < verList.Count; p++)
            {
                verts[p] = verList[p];
            }
            return verts;
        }

        private int[,] RandomElevations(int dimensions = 20)
        {
            elevationField = new int[dimensions, dimensions];
            Random rand = new Random();
            for (int i = 0; i < dimensions; i++)
            {
                for (int j = 0; j < dimensions; j++)
                {
                    int val = rand.Next(0, 3); // TODO fix
                    elevationField[i, j] = val;
                }
            }

            return elevationField;
        }
        public float GetHeightAt(Vector3 position)
        {
            int x = (int)Math.Floor(position.X);
            int z = (int)Math.Floor(position.Z);
            if (x >= 0 && x < elevationField.GetUpperBound(1) && z >= 0 && z < elevationField.GetUpperBound(0))
                return elevationField[z, x] * levelStep;
            else
                return -1;
        }
    }
}

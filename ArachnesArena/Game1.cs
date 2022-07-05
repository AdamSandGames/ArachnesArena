using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;


namespace ArachnesArena
{
    public enum MenuView
    {
        // “Start Screen”, “Start/Join Match”, “Quit”, "Playing"
        Start,
        HostJoin,
        Quit,
        Play
    }
    public class Game1 : Game
    {
        #region display
        private GraphicsDeviceManager graphics_;
        private SpriteBatch spriteBatch_;

        Camera povCamera;

        public int screenWidth_;
        public int screenHeight_;

        public static List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        public Effect WorldShader { get; set; }
        private BasicEffect _basicEffect;
        private BasicEffect _spriteEffect;
        //private Effect _myEffect;

        private SpriteFont testFont;
        private Texture2D PH_SpiderTex;
        private Texture2D UnitFootCircle;

        public SpriteFont buttonSpriteFont;
        public Texture2D buttonSprite;

        #endregion

        #region input

        public KeyboardState keyboardState_;
        public KeyboardState prevKeyboardState_;
        public MouseState mouseState_;
        public MouseState prevMouseState_;
        private int mouseDownX;
        private int mouseDownY;
        private bool draggingMouse = false;
        private Color dragColor = new Color(0f, 0.2f, 0f, 0.1f);
        private Texture2D rect;

        #endregion

        #region game

        private MenuView gameState_;
        // private ECSEntity testSpider;

        private List<ECSEntity> MyTeam;
        private List<ECSEntity> playerSelectedUnits;
        public static List<ECSEntity> allGameUnits;
        private Faction myPlayerFaction;

        public SimpleMap SimpleGameMap;

        #endregion

        #region network

        private bool isHost = true;
        private MPServer server_;
        private MPClient client_;

        private int PORT = 1337;
        private string SERVER_IP = "127.0.0.1"; // Local host
        private string SERVER_NAME = "ArachneArenaServer";

        #endregion

        public Game1()
        {
            graphics_ = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            graphics_.IsFullScreen = false;
            Window.IsBorderless = false; // true
            screenWidth_ = 1600;// GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight_ = 900;// GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics_.PreferredBackBufferWidth = screenWidth_;
            graphics_.PreferredBackBufferHeight = screenHeight_;
            Window.Position = new Point(0, 0);

            graphics_.ApplyChanges();
        }

        protected override void Initialize()
        {
            // Component Loading
            MyTeam = new List<ECSEntity>();
            playerSelectedUnits = new List<ECSEntity>();
            allGameUnits = new List<ECSEntity>();

            using (povCamera = new Camera(this, new Vector3(4, 7, 16), new Vector3(10, 0, 10), Vector3.Up))
                Components.Add(povCamera);

            if (!PortInUse(PORT))
            {
                myPlayerFaction = Faction.Blue;
                isHost = true;
                server_ = new MPServer(this, PORT, SERVER_NAME);
            }
            else
            {
                myPlayerFaction = Faction.Red;
                isHost = false;
                client_ = new MPClient(this, PORT, SERVER_IP, SERVER_NAME);

            }
            gameState_ = MenuView.Play;
            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch_ = new SpriteBatch(GraphicsDevice);
            RasterizerState rs = new RasterizerState
            {
                CullMode = CullMode.None
            };
            GraphicsDevice.RasterizerState = rs;



            // Menu stuff
            buttonSpriteFont = Content.Load<SpriteFont>("MenuButtonFont");
            buttonSprite = Content.Load<Texture2D>("simpleButton");

            // Placeholder Assets
            testFont = Content.Load<SpriteFont>("testFont");
            PH_SpiderTex = Content.Load<Texture2D>(@"TexturesSprites\PH_WeaverSprite");
            UnitFootCircle = Content.Load<Texture2D>(@"TexturesSprites\UnitCircleSprite");
            _basicEffect = new BasicEffect(GraphicsDevice);
            _spriteEffect = new BasicEffect(GraphicsDevice);
            //_myEffect = new Effect(GraphicsDevice);
            rect = new Texture2D(GraphicsDevice, 1, 1);

            if (isHost)
            {
                // Test Maps
                if (false) // true = random
                {
                    using SimpleMap testMap = new SimpleMap(this, _basicEffect);
                    SimpleGameMap = testMap;
                    Components.Add(testMap);
                }
                else // false = designed TODO Load map from image
                {
                    int[,] mapdata = new int[20, 20];
                    Random rand = new Random();
                    for (int i = 0; i < 20; i++)
                    {
                        for (int j = 0; j < 20; j++)
                        {
                            int val = rand.Next(0, 2);
                            mapdata[i, j] = val;
                        }
                    }
                    using SimpleMap imageMap = new SimpleMap(this, _basicEffect, 0.3f, mapdata);
                    SimpleGameMap = imageMap;
                    Components.Add(imageMap);
                }
            }
            else
            {
                while (SimpleGameMap == null)
                    client_.Listen();
            }


            if (isHost)
            {
                // Test spiders
                for (int i = 0; i < 3; i++)
                {
                    CreateSpider("blueSpider " + i, i, new Vector3(2.5f, 0f, 4.5f + i * 2), 0.3f, Faction.Blue);

                    CreateSpider("redSpider " + i, i + 3, new Vector3(8.5f, 0f, 4.5f + i * 2), 0.3f, Faction.Red);
                }
            }
            else
            {
                while (allGameUnits.Count < 2)
                    client_.Listen();
            }


            base.LoadContent();
        }
        protected override void UnloadContent()
        {

            base.UnloadContent();
        }
        protected override void Update(GameTime gameTime)
        {

            #region input state
            if (this.IsActive)
            {
                if (keyboardState_.IsKeyDown(Keys.Escape))
                {
                    if (isHost)
                    {
                        server_.ServerShutdown();
                    }
                    else
                    {
                        client_.SendDisconnect();
                    }
                    Exit();
                }
                prevMouseState_ = mouseState_;
                mouseState_ = Mouse.GetState(); // Mouse confined to window.
                if (mouseState_.X < screenWidth_ * 0)
                    Mouse.SetPosition(0, mouseState_.Y);
                else if (mouseState_.X > screenWidth_ * 1)
                    Mouse.SetPosition(screenWidth_, mouseState_.Y);
                if (mouseState_.Y < screenHeight_ * 0)
                    Mouse.SetPosition(mouseState_.X, 0);
                else if (mouseState_.Y > screenHeight_ * 1)
                    Mouse.SetPosition(mouseState_.X, screenHeight_);

                mouseState_ = Mouse.GetState();
                if (mouseState_.LeftButton == ButtonState.Pressed &&
                        prevMouseState_.LeftButton != ButtonState.Pressed) // On LMB down
                {
                    mouseDownX = mouseState_.X;
                    mouseDownY = mouseState_.Y;
                    draggingMouse = true;
                }

                prevKeyboardState_ = keyboardState_;
                keyboardState_ = Keyboard.GetState();
            }
            #endregion

            if (gameState_ == MenuView.Play)
            {
                if (isHost)
                {
                    server_.Listen();
                }
                else
                {
                    client_.Listen();
                }

                List<ECSEntity> newSelection = new List<ECSEntity>();
                if (mouseState_.LeftButton != ButtonState.Pressed &&
                        prevMouseState_.LeftButton == ButtonState.Pressed) // On LMB Up
                {
                    draggingMouse = false;
                    playerSelectedUnits.Clear();


                    if (Math.Abs(mouseDownX - mouseState_.X) < 10) // Left-click (no drag)
                    {
                        newSelection.Add(GetClickedUnit(mouseState_.X, mouseState_.Y));

                    }
                    else // Left-click Drag
                    {
                        newSelection = GetUnitsInRect(new Rectangle(mouseDownX, mouseDownY, mouseState_.X, mouseState_.Y));

                    }

                }
                else if (keyboardState_.IsKeyDown(Keys.F1))
                {
                    playerSelectedUnits.Clear();
                    newSelection.Add(MyTeam[0]);
                }
                else if (keyboardState_.IsKeyDown(Keys.F2))
                {
                    playerSelectedUnits.Clear();
                    newSelection.Add(MyTeam[1]);
                }
                else if (keyboardState_.IsKeyDown(Keys.F3))
                {
                    playerSelectedUnits.Clear();
                    newSelection.Add(MyTeam[2]);
                }
                if (keyboardState_.IsKeyDown(Keys.P) && isHost)
                {
                    isHost = false;
                    client_ = new MPClient(this, PORT, SERVER_IP, SERVER_NAME);
                }
                if (newSelection != null && newSelection.Count != 0)
                {
                    foreach (ECSEntity unit in allGameUnits)
                    {
                        unit.FindComponent<SpriteRenderer>().showCircle = false;
                    }
                    foreach (ECSEntity unit in newSelection)
                    {
                        if (unit != null) //&& unit.Alive)
                        {
                            unit.FindComponent<SpriteRenderer>().showCircle = true;
                            playerSelectedUnits.Add(unit);
                        }
                    }
                }

                Command newCommand = InputCommand();

                //for each selected unit: execute(unit)
                if (playerSelectedUnits.Count != 0)
                {
                    foreach (ECSEntity unit in playerSelectedUnits)
                    {
                        if (unit.FindComponent<FactionFlag>().IsFaction(myPlayerFaction))
                        {
                            if (newCommand != null)
                            {
                                IssueCommand(newCommand, unit);
                            }
                        }
                    }
                }
            }



            //int dmx = _mouseState.X - _prevMouseState.X;
            //int dmy = _mouseState.Y - _prevMouseState.Y;

            base.Update(gameTime);
        }

        public void IssueCommand(Command comm, ECSEntity unit)
        {
            if (isHost)
            {
                comm.execute(unit);
                if (server_.running)
                    server_.SendCommand(comm, unit.tag);
            }
            else
            {
                if (client_.running)
                    client_.SendCommand(comm, unit.tag);
            }
        }

        private Command InputCommand()
        {
            if (!keyboardState_.IsKeyDown(Keys.S)) // if not stopping units, find a valid command input
            {
                if (mouseState_.RightButton == ButtonState.Pressed)// && _prevMouseState.RightButton != ButtonState.Pressed)
                {
                    ECSEntity tarUnit = GetClickedUnit(mouseState_.X, mouseState_.Y);
                    if (tarUnit == null)
                    {
                        Vector2 target = GetMouseClickXZ(mouseState_.Position.ToVector2());
                        Vector3 targetPosition = new Vector3(target.X, 0f, target.Y);
                        return new MoveToPoint(targetPosition);
                    }
                    else
                    {
                        if (tarUnit.FindComponent<FactionFlag>().IsFaction(myPlayerFaction))
                        {
                            return new MoveToTarget(tarUnit);
                        }
                        else
                        {
                            return new AttackTarget(tarUnit);
                        }
                    }
                }
            }
            else
            {
                return new StopCommand();
            }
            return null;
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics_.GraphicsDevice.Clear(new Color(30, 30, 30, 255));

            graphics_.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphics_.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            graphics_.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics_.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            graphics_.GraphicsDevice.VertexSamplerStates[0] = SamplerState.AnisotropicWrap;

            base.Draw(gameTime);


            Vector2 uiAnchor = new Vector2(screenWidth_ * 1 / 10, screenHeight_ * 1 / 10);
            Vector2 newLine = new Vector2(0, 30);

            SetCamera(_basicEffect);
            SetCamera(_spriteEffect);
            //SetShadersBasic(_myEffect);

            spriteBatch_.Begin();

            foreach (SpriteRenderer sprite in sprites)
            {
                if (sprite.parent.Alive)
                {
                    sprite.Draw(_spriteEffect, GraphicsDevice);
                }
            }

            spriteBatch_.DrawString(testFont, $"Host: {isHost}", uiAnchor + newLine * 0f, Color.White);
            if (isHost)
                spriteBatch_.DrawString(testFont, $"Client Connections: {server_.players.Count}", uiAnchor + newLine * 1f, Color.White);
            else
                spriteBatch_.DrawString(testFont, $"Player ID: {client_.ID}", uiAnchor + newLine * 1f, Color.White);
            //spriteBatch_.DrawString(testFont, "X: " + povCamera.GetCamPos4().X.ToString(), uiAnchor + newLine * 0f, Color.White);
            //spriteBatch_.DrawString(testFont, "Y: " + povCamera.GetCamPos4().Y.ToString(), uiAnchor + newLine * 1f, Color.White);
            //spriteBatch_.DrawString(testFont, "Z: " + povCamera.GetCamPos4().Z.ToString(), uiAnchor + newLine * 2f, Color.White);

            //Vector2 mP = GetMouseClickXZ(mouseState_.Position.ToVector2());
            ////Vector2 mP = ZeroPlaneIntersect( CalculateRay(mouseState_.Position.ToVector2(), povCamera.View, povCamera.Projection, GraphicsDevice.Viewport));
            //spriteBatch_.DrawString(testFont, "mX: " + mP.X, uiAnchor + newLine * 4f, Color.White);
            //spriteBatch_.DrawString(testFont, "mZ: " + mP.Y, uiAnchor + newLine * 5f, Color.White);

            //Vector3 mV3 = RayYPlaneIntersect(  CalculateRay(mouseState_.Position.ToVector2(), povCamera.View, povCamera.Projection, GraphicsDevice.Viewport), 1f);

            
            //if (draggingMouse)
            //{
            //    graphics_.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //    int flipX = 0, flipY = 0;
            //    if (mouseState_.X > mouseDownX)
            //        flipX = 1;
            //    if (mouseState_.Y > mouseDownY)
            //        flipY = 1;
            //    int rectX = Math.Max(Math.Abs(mouseState_.X - mouseDownX), 1);
            //    int rectY = Math.Max(Math.Abs(mouseState_.Y - mouseDownY), 1);
            //    Texture2D rect = new Texture2D(GraphicsDevice, rectX, rectY);
            //    Color[] data = new Color[rectX * rectY];
            //    for (int i = 0; i < data.Length; i++) data[i] = dragColor;
            //    rect.SetData(data);
            //    Vector2 pos = mouseState_.Position.ToVector2() - new Vector2(rectX * flipX, rectY * flipY);
            //    spriteBatch_.Draw(rect, pos, Color.LimeGreen);//, layerDepth:1f);
            //}

            if (draggingMouse)
            {
                graphics_.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                int flipX = 0, flipY = 0;
                if (mouseState_.X > mouseDownX)
                    flipX = 1;
                if (mouseState_.Y > mouseDownY)
                    flipY = 1;
                int rectX = Math.Max(Math.Abs(mouseState_.X - mouseDownX), 1);
                int rectY = Math.Max(Math.Abs(mouseState_.Y - mouseDownY), 1);
                rect = new Texture2D(GraphicsDevice, rectX, rectY);
                Color[] data = new Color[rectX * rectY];
                for (int i = 0; i < data.Length; i++) data[i] = dragColor;
                rect.SetData(data);

                Vector2 pos = mouseState_.Position.ToVector2() - new Vector2(rectX * flipX, rectY * flipY);
                spriteBatch_.Draw(rect, pos, Color.LimeGreen);//, layerDepth:1f);

                

            }
            spriteBatch_.End();


        }

        public void CreateSpider(string name, int tag, Vector3 position, float scale, Faction faction)
        {
            using (ECSEntity newSpider = new ECSEntity(this, spriteBatch_, name, tag))
            {
                newSpider.AddComponent(new Transform(newSpider, position, scale, Quaternion.Identity));
                newSpider.AddComponent(new SpriteRenderer(newSpider, PH_SpiderTex, UnitFootCircle, povCamera));
                newSpider.AddComponent(new Motor(newSpider, SimpleGameMap));
                newSpider.AddComponent(new PlayerController(newSpider));
                newSpider.AddComponent(new FactionFlag(newSpider, faction));
                newSpider.AddComponent(new Combat(newSpider));
                Components.Add(newSpider);
                allGameUnits.Add(newSpider);
                if (faction == myPlayerFaction)
                    MyTeam.Add(newSpider);
            }
        }

        public void ReplaceMap(float step, int[,] elevs)
        {
            using SimpleMap newMap = new SimpleMap(this, _basicEffect, step, elevs);
            SimpleGameMap = newMap;
            Components.Add(newMap);
        }

        public static bool PortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] UPDipEndPoints = ipProperties.GetActiveUdpListeners();

            foreach (IPEndPoint endPoint in UPDipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            IPEndPoint[] TCPipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in TCPipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }

            return inUse;
        }

        public void NetCommandIssueByTag(Command comm, int tag)
        {
            ECSEntity unit = ECSEntity.GetEntityWithTagFromList(tag, allGameUnits);
            comm.execute(unit);
        }
        void SetCamera(BasicEffect effect)
        {
            effect.View = povCamera.View;
            effect.Projection = povCamera.Projection;
        }

        void SetShadersBasic(Effect myEffect)
        {
            // Camera
            myEffect.Parameters["View"].SetValue(povCamera.View);
            myEffect.Parameters["Projection"].SetValue(povCamera.Projection);
            myEffect.Parameters["CameraPosition"].SetValue(povCamera.GetCamPos4());
            myEffect.Parameters["CameraView"].SetValue(povCamera.GetViewVector4());
            myEffect.Parameters["CameraDir"].SetValue(povCamera.GetCamDir4());

            //// Lights
            //myEffect.Parameters["LightPos"].SetValue(light1.getLightPos4());
            //myEffect.Parameters["LightDirection"].SetValue(light1.getLightDir4());
            //myEffect.Parameters["LightColor"].SetValue(light1.getLightColor4());
            //// Ambient
            //myEffect.Parameters["AmbientColor"].SetValue(light1.Ambient.Color4);
            //myEffect.Parameters["AmbientIntensity"].SetValue(light1.Ambient.Intensity);
            //// Diffuse
            //myEffect.Parameters["DiffuseColor"].SetValue(light1.Diffuse.Color4);
            //myEffect.Parameters["DiffuseIntensity"].SetValue(light1.Diffuse.Intensity);
            //// Specular
            //myEffect.Parameters["Shininess"].SetValue(light1.Specular.Shine);
            //myEffect.Parameters["SpecularColor"].SetValue(light1.Specular.Color4);
            //myEffect.Parameters["SpecularIntensity"].SetValue(light1.Specular.Intensity);
            //// Fog
            //myEffect.Parameters["FogDistance"].SetValue(light1.Fog.Distance);
            //myEffect.Parameters["FogDensity"].SetValue(light1.Fog.Density);
            //myEffect.Parameters["FogColor"].SetValue(light1.Fog.Color4);
        }

        private Ray CalculateRay(Vector2 mousePosition, Matrix view, Matrix projection, Viewport viewport)
        {
            Vector3 nearPoint = viewport.Unproject(
                new Vector3(mousePosition.X, mousePosition.Y, 0.0f),
                projection,
                view,
                Matrix.Identity);

            Vector3 farPoint = viewport.Unproject(
                new Vector3(mousePosition.X, mousePosition.Y, 1.0f),
                projection,
                view,
                Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }
        private float? IntersectDistance(BoundingSphere sphere, Vector2 mousePosition,
            Matrix view, Matrix projection, Viewport viewport)
        {
            Ray mouseRay = CalculateRay(mousePosition, view, projection, viewport);
            return mouseRay.Intersects(sphere);
        }
        private Vector2 ZeroPlaneIntersect(Ray ray)
        {
            float denom = Vector3.Dot(Vector3.Up, ray.Direction);
            if (Math.Abs(denom) > 0.0001f) // your favorite epsilon
            {
                float t = -Vector3.Dot(Vector3.Up, ray.Position) / denom;

                Vector3 intersect = ray.Position + ray.Direction * t;
                return new Vector2(intersect.X, intersect.Z);
            }
            return Vector2.Zero;
        }
        private Vector3 RayYPlaneIntersect(Ray ray, float elevation)
        {
            float denom = Vector3.Dot(new Vector3(0, elevation, 0), ray.Direction);
            if (Math.Abs(denom) > 0.0001f) // your favorite epsilon
            {
                float t = -Vector3.Dot(new Vector3(0, elevation, 0), ray.Position) / denom;

                Vector3 intersect = ray.Position + ray.Direction * t;
                return new Vector3(intersect.X, elevation, intersect.Z);
            }
            return Vector3.Zero;
        }
        private Vector2 GetMouseClickXZ(Vector2 mxy)
        {
            return ZeroPlaneIntersect(CalculateRay(mxy, povCamera.View, povCamera.Projection, GraphicsDevice.Viewport));
        }
        private ECSEntity GetClickedUnit(int x, int y)
        {
            Vector2 cPos = GetMouseClickXZ(new Vector2(x, y));
            ECSEntity nearest = null;
            Vector2 nearPos = Vector2.UnitY;
            foreach (ECSEntity unit in allGameUnits)
            {
                Vector2 tPos = new Vector2(unit.transform.Position.X, unit.transform.Position.Z);
                if ((nearPos == Vector2.UnitY || Vector2.Distance(tPos, cPos) < Vector2.Distance(nearPos, cPos)) && Vector2.Distance(tPos, cPos) < 0.2f)
                {
                    nearPos = tPos;
                    nearest = unit;
                }
            }

            return nearest;
        }
        private List<ECSEntity> GetUnitsInRect(Rectangle rect)
        {
            Rectangle worldRect = new Rectangle(
                GetMouseClickXZ(new Vector2(rect.X, rect.Y)).ToPoint(),
                GetMouseClickXZ(new Vector2(rect.Width, rect.Height)).ToPoint());
            List<ECSEntity> result = new List<ECSEntity>();
            Rectangle fixRect = new Rectangle(
                Math.Min(worldRect.X, worldRect.Width),
                Math.Min(worldRect.Y, worldRect.Height),
                Math.Max(worldRect.X, worldRect.Width),
                Math.Max(worldRect.Y, worldRect.Height));
            foreach (ECSEntity unit in allGameUnits)
            {
                if (fixRect.Contains((int)unit.transform.Position.X, (int)unit.transform.Position.Z))
                {
                    result.Add(unit);
                }
            }
            return result;
        }
    }
}


/*

One Red

Two Orange
? Navigation Mesh

Three Violet
•	“Start Screen”, “Start/Join Match”, “Quit” Menus/Options implemented
•	Players can connect to each other by joining hosted matches.
•	Units can Use Abilities included in their skillset
•	Command “Use Ability” implemented
•	Ability: Sticky Throw implemented

Alpha Blue
•	Ability: Spin Web implemented
•	Webs implemented
•	*//**Players can play functional 1v1 deathmatches with each other.**//*
•	Spiders Level up upgrades and Spider ability upgrades implemented. *Spider types and abilities fully implemented
•	UI: Unit Health bars
•	Node Pathfinding

Beta Green
•	Interface Behavior: [Left-click-and-drag] group select implemented
•	Audio and Sound Effects for Spiders
•	In-match music plays
•	UI: Selected Unit Health, Stats, and Abilities
•	Crickets implemented with wandering movement
•	Crickets heals their killers for a small amount of lost health
•	Crickets grant upgrade currency to spiders for their abilities

Final Gold
•	Fog of War fully implemented
•	User interface finalized
•	Controllable units will clump together when given the same commands and will attempt to execute those commands
•	Units will attack any non-ally that enters their attack range but will only follow things to attack when given a command. *Game World Features fully implemented
•	Return to menus from matches before the match ends

 */

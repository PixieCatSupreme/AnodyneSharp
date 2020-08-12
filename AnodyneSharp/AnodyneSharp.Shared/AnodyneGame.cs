#region Using Statements
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Resources.Loading;
using AnodyneSharp.Resources.Writing;
using AnodyneSharp.States;
using AnodyneSharp.UI;
using AnodyneSharp.UI.Font;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace AnodyneSharp
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AnodyneGame : Game
    {
        GraphicsDeviceManager graphics;

        State _currentState;
        Camera _camera;

        private static Effect fadeout;
        private static float static_timer = 0;
        private static int static_step = 0;
        private static Effect static_shader;
        private static Effect blackwhite;

        private UILabel _fpsLabel;

        private string _baseFolder;

        public AnodyneGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _currentState = null;

            _fpsLabel = new UILabel(new Vector2(0, GameConstants.HEADER_HEIGHT), Color.LightBlue, false);

            GlobalState.START_TIME = DateTime.Now;

            //_baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _baseFolder = "";

            GlobalState.ActivatedNexusPortals[MapUtilities.GetMapID("STREET")] = true;


#if WINDOWS
            InitGraphics();
#endif
        }

        private void InitGraphics()
        {
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 540;

            graphics.ApplyChanges();
        }

        /// <summary>
        /// 
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if OPENGL
            InitGraphics();
#endif

            _camera = new Camera();
            _camera.Initialize(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            SpriteDrawer.Initialize(graphics.GraphicsDevice);

            InventoryManager.ResetValues();
            AchievementManager.ResetValues();

            EntityManager.Initialize();

            base.Initialize();

            _currentState = new PlayState(_camera);
            _currentState.Create();

            _fpsLabel.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            GlobalState.NEXT_MAP_NAME = "BLANK";
            GlobalState.PLAYER_WARP_TARGET = new Vector2(23, 130 - 20); //original reports 23,130 but ends up subtracting HEADER_HEIGHT(20) from Y value

            ResourceManager.LoadResources(Content);
            CardDataManager.ReadCardData();

            if (File.Exists("InputConfig.dat"))
            {
                using InputConfigLoader inputConfigWriter = new InputConfigLoader($"{_baseFolder}InputConfig.dat");
                inputConfigWriter.LoadInputConfig();
            }
            else
            {
                SetDefaultKeys();
                using InputConfigWriter inputConfigWriter = new InputConfigWriter($"{_baseFolder}InputConfig.dat");
                inputConfigWriter.WriteInputConfig();
            }

            DialogueManager.LoadDialogue( Language.EN);

            fadeout = Content.Load<Effect>("effects/screenfade");

            fadeout.CurrentTechnique = fadeout.Techniques["Fade"];
            fadeout.Parameters["FadeColor"].SetValue(new Vector4(0,0,0,1));
            fadeout.Parameters["ScreenSize"].SetValue(new Vector2(GameConstants.SCREEN_WIDTH_IN_PIXELS, GameConstants.SCREEN_HEIGHT_IN_PIXELS));

            static_shader = Content.Load<Effect>("effects/static");
            static_shader.CurrentTechnique = static_shader.Techniques["AddStatic"];

            blackwhite = Content.Load<Effect>("effects/blackwhite");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            GameTimes.UpdateTimes(gameTime);
            KeyInput.Update();

            _camera.Update();

            _currentState.Update();

            static_timer += GameTimes.DeltaTime;
            if (static_timer > 1.0f / 8.0f)
            {
                static_timer = 0;
                static_step = (static_step + 1) % 4;
                static_shader.Parameters["step"].SetValue(static_step);
            }

            if (KeyInput.JustPressedKey(Keys.F12))
            {
                GlobalState.ShowFPS = !GlobalState.ShowFPS;
            }

            fadeout.Parameters["Fade"].SetValue(GlobalState.transition_fadeout_progress);
            fadeout.Parameters["StrideSize"].SetValue((int)GlobalState.PIXELATION);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GameTimes.UpdateFPS(gameTime);
            if (GlobalState.ShowFPS)
            {
                _fpsLabel.SetText($"FPS: {GameTimes.FPS:0}");
            }

            SpriteDrawer.DrawBackground(_currentState.Background);

            SpriteDrawer.BeginDraw(_camera);
            _currentState.Draw();
            SpriteDrawer.EndDraw();

            SpriteDrawer.BeginGUIDraw(gameEffect:(GlobalState.CURRENT_MAP_NAME == "SUBURB" ? static_shader : null));
            _currentState.DrawUI();

            if (GlobalState.ShowFPS)
            {
                _fpsLabel.Draw();
            }

            SpriteDrawer.EndGUIDraw();

            //TODO: Actually implement explicit render stages
            SpriteDrawer.Render((GlobalState.CURRENT_MAP_NAME == "SUBURB" && GlobalState.transition_fadeout_progress == 0 ? blackwhite : fadeout));
        }

        private void SetDefaultKeys()
        {
            KeyInput.RebindableKeys = new Dictionary<KeyFunctions, RebindableKey>()
            {
                [KeyFunctions.Up] = new RebindableKey(
                    new List<Keys> { Keys.Up },
                    new List<Buttons> { Buttons.DPadUp, Buttons.LeftThumbstickUp }
                    ),
                [KeyFunctions.Right] = new RebindableKey(
                    new List<Keys> { Keys.Right },
                    new List<Buttons> { Buttons.DPadRight, Buttons.LeftThumbstickRight }
                    ),
                [KeyFunctions.Down] = new RebindableKey(
                    new List<Keys> { Keys.Down },
                    new List<Buttons> { Buttons.DPadDown, Buttons.LeftThumbstickDown }
                    ),
                [KeyFunctions.Left] = new RebindableKey(
                    new List<Keys> { Keys.Left },
                    new List<Buttons> { Buttons.DPadLeft, Buttons.LeftThumbstickLeft }
                    ),
                [KeyFunctions.Accept] = new RebindableKey(
                    new List<Keys> { Keys.C },
                    new List<Buttons> { Buttons.A }
                    ),
                [KeyFunctions.Cancel] = new RebindableKey(
                    new List<Keys> { Keys.X },
                    new List<Buttons> { Buttons.B }
                    ),
                [KeyFunctions.Pause] = new RebindableKey(
                    new List<Keys> { Keys.Enter, Keys.Escape },
                    new List<Buttons> { Buttons.Start, Buttons.Back }
                    ),
                [KeyFunctions.PreviousPage] = new RebindableKey(
                    new List<Keys> { Keys.PageDown },
                    new List<Buttons> { Buttons.LeftShoulder, Buttons.LeftTrigger }
                    ),
                [KeyFunctions.NextPage] = new RebindableKey(
                    new List<Keys> { Keys.PageUp },
                    new List<Buttons> { Buttons.RightShoulder, Buttons.RightTrigger }
                    )
            };
        }
    }
}

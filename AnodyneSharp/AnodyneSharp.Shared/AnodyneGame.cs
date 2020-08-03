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

        private static Effect bwEffect;

        private UILabel _fpsLabel;

        private string _baseFolder;

        public AnodyneGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.PreferredBackBufferWidth = 1920;
            //graphics.PreferredBackBufferHeight = 1080;
            //graphics.PreferredBackBufferWidth = 960;
            //graphics.PreferredBackBufferHeight = 1080;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 540;
            //graphics.ToggleFullScreen();
            graphics.ApplyChanges();

            _camera = new Camera();
            _camera.Initialize(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            _currentState = null;

            _fpsLabel = new UILabel(new Vector2(0, GameConstants.HEADER_HEIGHT), Color.LightBlue, false);

            GlobalState.START_TIME = DateTime.Now;

            //_baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _baseFolder = "";

            GlobalState.ActivatedNexusPortals[MapUtilities.GetMapID("STREET")] = true;

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

            bwEffect = Content.Load<Effect>("effects/blackwhite");

            bwEffect.CurrentTechnique = bwEffect.Techniques["BasicColorDrawing"];
            //bwEffect.Parameters["Cutoff"].SetValue(DrawingUtilities.GetDrawingZ(DrawOrder.FG_SPRITES, 0));
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

            if (KeyInput.JustPressedKey(Keys.F12))
            {
                GlobalState.ShowFPS = !GlobalState.ShowFPS;
            }

            SpriteDrawer.FullScreenFade = Color.Lerp(Color.White, Color.Black, GlobalState.transition_fadeout_progress);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GameTimes.UpdateFPS(gameTime);
            _fpsLabel.SetText($"FPS: {GameTimes.FPS:0}");

            SpriteDrawer.BeginDraw(_camera);
            _currentState.Draw();
            SpriteDrawer.EndDraw();

            SpriteDrawer.BeginGUIDraw();
            _currentState.DrawUI();

            if (GlobalState.ShowFPS)
            {
                _fpsLabel.Draw();
            }

            SpriteDrawer.EndGUIDraw();

            SpriteDrawer.Render();
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

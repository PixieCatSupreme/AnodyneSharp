#region Using Statements
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Effects;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Resources.Loading;
using AnodyneSharp.Resources.Writing;
using AnodyneSharp.States;
using AnodyneSharp.States.MainMenu;
using AnodyneSharp.UI;
using AnodyneSharp.UI.Font;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

#endregion

namespace AnodyneSharp
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AnodyneGame : Game
    {
        public enum GameState
        {
            TitleScreen,
            MainMenu,
            Intro,
            Game
        }

        GraphicsDeviceManager graphics;

        State _currentState;
        Camera _camera;

        private UILabel _fpsLabel;

        private string _baseFolder;

        public AnodyneGame()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _currentState = null;

            //_baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _baseFolder = "";

#if WINDOWS
            InitGraphics();
#endif
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

            SpriteDrawer.Initialize(graphics.GraphicsDevice);

            GlobalState.ResetValues();

            EntityManager.Initialize();

            base.Initialize();

            SetState(GameState.TitleScreen);

            _fpsLabel = new UILabel(new Vector2(0, GameConstants.HEADER_HEIGHT), false, "", Color.LightBlue);

            GlobalState.darkness.SetCamera(_camera);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
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

            DialogueManager.Reload();

            FG_Blend.Load(Content);

            foreach (var effect in GlobalState.AllEffects)
            {
                effect.Load(Content, graphics.GraphicsDevice);
            }

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

            _currentState.Update();

            _camera.Update();

            FG_Blend.Update(_camera);

            foreach (var effect in GlobalState.AllEffects.Where(e => e.Active()))
            {
                effect.Update();
            }

            if (KeyInput.JustPressedKey(Keys.F12))
            {
                GlobalState.ShowFPS = !GlobalState.ShowFPS;
            }

            if (GlobalState.ClosingGame)
            {
                Exit();
            }
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

            SpriteDrawer.BeginDraw(_camera, FG_Blend.GetEffect());
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

        private void SetState(GameState state)
        {
            foreach (var effect in GlobalState.AllEffects)
            {
                effect.Deactivate();
            }

            _currentState = state switch
            {
                GameState.TitleScreen => new TitleState(),
                GameState.MainMenu => new MainMenuState(),
                GameState.Intro => new IntroState(),
                GameState.Game => new PlayState(_camera),
                _ => null
            };

            if (_currentState != null)
            {
                _currentState.Create();
                _currentState.ChangeStateEvent = SetState;
            }
        }

        private void InitGraphics()
        {
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 540;

            graphics.ApplyChanges();
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
                    ),
                [KeyFunctions.Broom1] = new RebindableKey(
                    new List<Keys> { Keys.D1 }
                    ),
                [KeyFunctions.Broom2] = new RebindableKey(
                    new List<Keys> { Keys.D2 }
                    ),
                [KeyFunctions.Broom3] = new RebindableKey(
                    new List<Keys> { Keys.D3 }
                    ),
                [KeyFunctions.Broom4] = new RebindableKey(
                    new List<Keys> { Keys.D4 }
                    )
            };
        }
    }
}

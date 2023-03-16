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
            Game,
            Credits
        }

        GraphicsDeviceManager graphics;

        State _currentState;
        Camera _camera;

        private UILabel _fpsLabel;

        public AnodyneGame()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _currentState = null;


            if (!Directory.Exists(GameConstants.SavePath))
            {
                Directory.CreateDirectory(GameConstants.SavePath);
            }

            if (!Directory.Exists(GameConstants.SavePath + "Saves/"))
            {
                Directory.CreateDirectory(GameConstants.SavePath + "Saves/");
            }

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
#if OPENGL || ANDROID
            InitGraphics();
#endif

            _camera = new Camera();

            SpriteDrawer.Initialize(graphics.GraphicsDevice);

            GlobalState.ResetValues();

            EntityManager.Initialize();

            base.Initialize();

            _fpsLabel = new UILabel(new Vector2(0, GameConstants.HEADER_HEIGHT), false, "", Color.LightBlue);

            GlobalState.darkness.SetCamera(_camera);

            Window.Title = "Anodyne Fan Remake";

            SetState(GameState.TitleScreen);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            if (File.Exists($"{GameConstants.SavePath}InputConfig.dat"))
            {
                using InputConfigLoader inputConfigWriter = new InputConfigLoader($"{GameConstants.SavePath}InputConfig.dat");
                inputConfigWriter.LoadInputConfig();
            }
            else
            {
                SetDefaultKeys();
                using InputConfigWriter inputConfigWriter = new InputConfigWriter($"{GameConstants.SavePath}InputConfig.dat");
                inputConfigWriter.WriteInputConfig();
            }

            ResourceManager.LoadResources(Content);
            CardDataManager.ReadCardData();
            DialogueManager.Reload();
            SpriteDrawer.Load(Content);

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
            GameTimes.TimeScale = 1;

            base.Update(gameTime);

            GameTimes.UpdateTimes(gameTime);
            KeyInput.Update();

            if (GlobalState.settings.pause_on_unfocus && !IsActive)
            {
                return;
            }

            _currentState.Update();

            _camera.Update();

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

            if (GlobalState.ResolutionDirty)
            {
                InitGraphics();
                GlobalState.ResolutionDirty = false;
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

#if DEBUG
            if (KeyInput.JustPressedKey(Keys.F11))
            {
                Cheatz.Cheatz.ToggleFuckItMode();
            }

            if (KeyInput.JustPressedKey(Keys.P))
            {
                SetState(GameState.Credits);
            }
#endif

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
                GameState.Credits => new CreditsState(),
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
#if ANDROID
            GlobalState.settings.resolution = Resolution.Scaled;
            GlobalState.settings.scale = SpriteDrawer.MaxScale;
#else
            GlobalState.settings.scale = Math.Min(GlobalState.settings.scale, SpriteDrawer.MaxScale);
#endif

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.IsFullScreen = false;
            switch (GlobalState.settings.resolution)
            {
                case Resolution.Windowed:
                    graphics.PreferredBackBufferWidth = 160 * GlobalState.settings.scale;
                    graphics.PreferredBackBufferHeight = 180 * GlobalState.settings.scale;
                    break;
                case Resolution.Scaled:
                case Resolution.Stretch:
                    graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    graphics.IsFullScreen = true;
                    break;
            }

            switch (GlobalState.settings.fps)
            {
                case FPS.Fixed:
                    IsFixedTimeStep = true;
                    graphics.SynchronizeWithVerticalRetrace = true;
                    break;
                case FPS.VSync:
                    IsFixedTimeStep = false;
                    graphics.SynchronizeWithVerticalRetrace = true;
                    break;
                case FPS.Unlocked:
                    IsFixedTimeStep = false;
                    graphics.SynchronizeWithVerticalRetrace = false;
                    break;
            }

            graphics.ApplyChanges();
        }

        private void SetDefaultKeys()
        {
            KeyInput.RebindableKeys = new Dictionary<KeyFunctions, RebindableKey>()
            {
                [KeyFunctions.Up] = new RebindableKey(
                    new List<Keys> { Keys.Up, Keys.W },
                    new List<Buttons> { Buttons.DPadUp, Buttons.LeftThumbstickUp }
                    ),
                [KeyFunctions.Right] = new RebindableKey(
                    new List<Keys> { Keys.Right, Keys.D },
                    new List<Buttons> { Buttons.DPadRight, Buttons.LeftThumbstickRight }
                    ),
                [KeyFunctions.Down] = new RebindableKey(
                    new List<Keys> { Keys.Down, Keys.S },
                    new List<Buttons> { Buttons.DPadDown, Buttons.LeftThumbstickDown }
                    ),
                [KeyFunctions.Left] = new RebindableKey(
                    new List<Keys> { Keys.Left, Keys.A },
                    new List<Buttons> { Buttons.DPadLeft, Buttons.LeftThumbstickLeft }
                    ),
                [KeyFunctions.Accept] = new RebindableKey(
                    new List<Keys> { Keys.C, Keys.RightShift },
                    new List<Buttons> { Buttons.A }
                    ),
                [KeyFunctions.Cancel] = new RebindableKey(
                    new List<Keys> { Keys.X, Keys.Space },
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
                    new List<Keys> { Keys.D1 },
                    new List<Buttons> { Buttons.RightThumbstickUp }
                    ),
                [KeyFunctions.Broom2] = new RebindableKey(
                    new List<Keys> { Keys.D2 },
                    new List<Buttons> { Buttons.RightThumbstickRight }
                    ),
                [KeyFunctions.Broom3] = new RebindableKey(
                    new List<Keys> { Keys.D3 },
                    new List<Buttons> { Buttons.RightThumbstickDown }
                    ),
                [KeyFunctions.Broom4] = new RebindableKey(
                    new List<Keys> { Keys.D4 },
                    new List<Buttons> { Buttons.RightThumbstickLeft }
                    ),
                [KeyFunctions.QuickSave] = new RebindableKey(
                    new List<Keys> { Keys.F4 }
                    ),
                [KeyFunctions.QuickLoad] = new RebindableKey(
                    new List<Keys> { Keys.F5 }
                    )
            };
        }
    }
}

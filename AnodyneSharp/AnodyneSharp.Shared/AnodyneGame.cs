#region Using Statements
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.States;
using AnodyneSharp.UI;
using AnodyneSharp.UI.Font;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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

        private UILabel _fpsLabel;

        public AnodyneGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.PreferredBackBufferWidth = 1920;
            //graphics.PreferredBackBufferHeight = 1080;
            //graphics.PreferredBackBufferWidth = 960;
            //graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 540;
            //graphics.ToggleFullScreen();
            graphics.ApplyChanges();

            _camera = new Camera();
            _camera.Initialize(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            _camera.Zoom = 3;

            _currentState = null;

            _fpsLabel = new UILabel(new Vector2(0, GameConstants.HEADER_HEIGHT), Color.LightBlue, false);
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

            GlobalState.CUR_HEALTH = GlobalState.MAX_HEALTH = 16;

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
            GlobalState.CURRENT_MAP_NAME = "STREET";

            ResourceManager.LoadResources(Content);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // TODO: Add your update logic here			
            base.Update(gameTime);
            GameTimes.UpdateTimes(gameTime);

            _camera.Update();

            _currentState.Update();

            if (KeyInput.CanPressKey(Keys.F12))
            {
                GlobalState.ShowFPS = !GlobalState.ShowFPS;
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
            _fpsLabel.SetText($"FPS: {GameTimes.FPS:0}");

            SpriteDrawer.BeginDraw(_camera);
            _currentState.Draw();
            SpriteDrawer.EndDraw();

            SpriteDrawer.BeginGUIDraw(graphics.PreferredBackBufferWidth / GameConstants.SCREEN_WIDTH_IN_PIXELS);
            _currentState.DrawUI();

            if (GlobalState.ShowFPS)
            {
                _fpsLabel.Draw();
            }

            SpriteDrawer.EndGUIDraw();
        }
    }
}

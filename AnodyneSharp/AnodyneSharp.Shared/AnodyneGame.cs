#region Using Statements
using System;
using AnodyneSharp.Drawing;
using AnodyneSharp.Map;
using AnodyneSharp.Map.Tiles;
using AnodyneSharp.Registry;
using AnodyneSharp.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AnodyneSharp.Input;
using AnodyneSharp.Entities.Player;
using AnodyneSharp.Entities;
using System.Collections.Generic;

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
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            SpriteDrawer.Initialize(graphics.GraphicsDevice);

            _currentState = new PlayState(_camera);

            EntityManager.Initialize();

            EntityManager.GetGridEntities("STREET",new Vector2(0, 4) , out List<EntityPreset> t);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        { 
            GlobalState.CURRENT_MAP_NAME = "STREET";
            GlobalState.FreeRoamCamera = true;

            TileData.LoadTileMaps(Content);

            Player.SetSprites(Content);
            _currentState.Create();
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
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GameTimes.UpdateFPS(gameTime);

            SpriteDrawer.BeginDraw(_camera);
            _currentState.Draw();
            SpriteDrawer.EndDraw();


        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PongGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentManager contentManager;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Color bgcolor;
        bool paused;
        Rectangle screenRectangle;

        // screen resolution
        const int X_RESOLUTION = 1280;
        const int Y_RESOLUTION = 720;

        // game objects
        Paddle paddle;
        Ball ball;

        // score support
        int player1_Score;
        int player2_Score;
        Vector2 scoreLocation;

        public PongGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Setup screen
            Window.Title = "Breakout";
            graphics.PreferredBackBufferWidth = X_RESOLUTION;
            graphics.PreferredBackBufferHeight = Y_RESOLUTION;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            // screen rectangle used for bounds
            screenRectangle = new Rectangle(
                0,
                0,
                graphics.PreferredBackBufferWidth,
                graphics.PreferredBackBufferHeight);
            bgcolor = Color.DarkSlateGray;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            contentManager = Content;

            Texture2D tempTexture = Content.Load<Texture2D>("ball");
            ball = new Ball(tempTexture, screenRectangle);
            tempTexture = Content.Load<Texture2D>("paddle");
            paddle = new Paddle(tempTexture, screenRectangle);
            font = Content.Load<SpriteFont>("font");
            scoreLocation = new Vector2(20, 20);
            bgcolor = Color.DarkSlateGray;

            startGame();
        }

        /// <summary>
        /// startGame() reloads level to beginning state
        /// </summary>
        protected void startGame()
        {
            ball.setStart();
            paddle.StartPosition();
            ball.StartPosition(paddle.GetBounds());
            player1_Score = 0;
            player2_Score = 0;
            paused = false;

        }

        
        
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            if (!paused)
            {
                //update ball and paddle
                paddle.Update();
                ball.Update(paddle.GetBounds());
                ball.PaddleCollision(paddle.GetBounds());
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(bgcolor);

            spriteBatch.Begin();
            
            paddle.Draw(spriteBatch);
            ball.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

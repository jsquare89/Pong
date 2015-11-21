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
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Storage;
using System.IO;

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
        Rectangle screenRectangle;
        bool paused;

        // screen resolution
        const int X_RESOLUTION = 1280;
        const int Y_RESOLUTION = 720;

        // game objects
        Paddle player1Paddle;
        Paddle player2Paddle;
        Ball ball;

        // Controls support
        GamePadState p1GamePad;
        GamePadState p2GamePad;
        KeyboardState keyboard;

        // score support
        int player1_Score;
        int player2_Score;
        Vector2 player1ScoreLocation;
        Vector2 player2ScoreLocation;

        // Save data handling
        StorageDevice device; // HDD saving to
        StorageContainer container; // STFS container to save to
        string containerName = "PongGameStorage";
        string filename = "savegame.sav";
        //[Serializable]
        public struct SaveGameData
        {
            public int highScore;
        }
        SaveGameData saveGameData;

        // gamestates
        enum GameState
        {
            MainMenu,
            ScoreScreen,
            Playing,
        }
        GameState currentGameState = GameState.MainMenu;
        //Menu

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
            // set position for player score to be displayed
            player1ScoreLocation.X = 20;
            player1ScoreLocation.Y = 20;
            player2ScoreLocation.X = screenRectangle.Width - 50;
            player2ScoreLocation.Y = 20;
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

            //Add component for accessing save state on xbox
#if (XBOX)
            Components.Add(new GamerServicesComponent(this));
#endif

            Texture2D tempTexture = Content.Load<Texture2D>("ball");
            ball = new Ball(tempTexture, screenRectangle);
            tempTexture = Content.Load<Texture2D>("paddle");
            player1Paddle = new Paddle(tempTexture, screenRectangle, 1);
            player2Paddle = new Paddle(tempTexture, screenRectangle, 2);
            font = Content.Load<SpriteFont>("font");
            bgcolor = Color.DarkSlateGray;
            paused = false;

            startGame();
        }

        /// <summary>
        /// startGame() reloads level to beginning state
        /// </summary>
        protected void startGame()
        {
            ball.setStart();
            player1Paddle.StartPosition();
            ball.StartPosition();
            player1_Score = 0;
            player2_Score = 0;

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
                player1Paddle.Update();
                player2Paddle.Update();
                ball.Update();
                //ball.PaddleCollision(player1Paddle.GetBounds());
                ball.PaddleCollision(player2Paddle.GetBounds());
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// gets strorage device to save to
        /// </summary>
        public void GetDevice()
        {
            //Starts the selection processes.
            IAsyncResult result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            //Sets the global variable.
            device = StorageDevice.EndShowSelector(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();
        }

        public void GetContainer()
        {
            //Starts the selection processes.
            IAsyncResult result = device.BeginOpenContainer(containerName, null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            //Sets the global variable.
            container = device.EndOpenContainer(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();
        }

        public void Save()
        {
            // Check to see whether the save exists.
            if (container.FileExists(filename))
                // Delete it so that we can create one fresh.
                container.DeleteFile(filename);

            using (Stream stream = container.CreateFile(filename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData)); // create XML serializer object
                serializer.Serialize(stream, saveGameData); // pass saveGameData struct to xml stream
            }
            container.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool true if loaded from file. false if not file to load</returns>
        public bool Load()
        {
            // Check to see whether the save exists.
            if (!container.FileExists(filename))
            {
                // If not, dispose of the container and return.
                container.Dispose();
                return false;
            }

            // Open the file.
            Stream stream = container.OpenFile(filename, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData)); // create XML serializer object
            saveGameData = (SaveGameData)serializer.Deserialize(stream); // get saved data from stream(file)
            stream.Close();
            container.Dispose();
            return true;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(bgcolor);

            spriteBatch.Begin();
            
            player1Paddle.Draw(spriteBatch);
            player2Paddle.Draw(spriteBatch);
            ball.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

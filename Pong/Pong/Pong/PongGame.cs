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

        // score support
        public static int player1_Lives;
        public static int player2_Lives;
        public static int pongScore;
        int highScoreCount;
        public static bool scoreSaved;
        Vector2 player1LifePosition;
        Vector2 player2LifePosition;
        Vector2 scorePosition;

        // Save data handling
        StorageDevice device; // HDD saving to
        StorageContainer container; // STFS container to save to
        string containerName = "PongGameStorage";
        string filename = "savegame.sav";
        //[Serializable]
        public struct SaveGameData
        {
            public int[] highScore;
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
            player1LifePosition.X = 100;
            player1LifePosition.Y = 20;
            player2LifePosition.X = screenRectangle.Width - 120;
            player2LifePosition.Y = 20;
            scorePosition.X = screenRectangle.Width / 2 - 70;
            scorePosition.Y = 20;

            currentGameState = GameState.Playing;
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

            // initialize save game data
            pongScore = 0;
            highScoreCount = 10;
            scoreSaved = false;
            saveGameData.highScore = new int[highScoreCount];
            for (int i = 0; i < highScoreCount; i++)
            {
                saveGameData.highScore[i] = 0;
            }
            // Load save game high score into saveGameData struct
            GetDevice();

            startGame();
            
        }

        /// <summary>
        /// startGame() reloads level to beginning state
        /// </summary>
        protected void startGame()
        {

            Load();

            // initialise paddles and ball
            ball.setStart();
            player1Paddle.StartPosition();
            player2Paddle.StartPosition();
            ball.StartPosition();

            // set player lives
            player1_Lives = 5;
            player2_Lives = 5;

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

            if (currentGameState == GameState.Playing && player1_Lives > 0 && player2_Lives > 0)
            {
                //update ball and paddle
                player1Paddle.Update();
                player2Paddle.Update();
                ball.Update();
                ball.PaddleCollision(player1Paddle.GetBounds(), player2Paddle.GetBounds());
                LoadHighScore();
            }else if(player1_Lives == 0 || player2_Lives  == 0)
            {
                if (!scoreSaved)
                    UpdateSavedHighScore();
            }

            base.Update(gameTime);
        }

        private void UpdateSavedHighScore()
        {
            
            // get score index
            int scoreIndex = -1;
            for(int i=0; i<highScoreCount; i++)
            {
                if (pongScore > saveGameData.highScore[i])
                {
                    scoreIndex = i;
                    break;
                }
            }
               
            // move scores down and add score at index
            if(scoreIndex> -1)
            {
                for(int i = highScoreCount -1; i > scoreIndex; i--)
                {
                    saveGameData.highScore[i] = saveGameData.highScore[i - 1];
                }

                saveGameData.highScore[scoreIndex] = pongScore;
                Save();
                scoreSaved = true;
               
            } 
            
        }

        private void LoadHighScore()
        {
            Load();
        }

        /// <summary>
        /// gets strorage device to save to
        /// </summary>
        private void GetDevice()
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

        private void GetContainer()
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

        private void Save()
        {
            GetContainer();
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
        private bool Load()
        {
            GetContainer();
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
            spriteBatch.DrawString(font, player1_Lives.ToString(), player1LifePosition, Color.White);
            spriteBatch.DrawString(font, player2_Lives.ToString(), player2LifePosition, Color.White);
            spriteBatch.DrawString(font, "Score: " + pongScore, scorePosition, Color.White);
            player1Paddle.Draw(spriteBatch);
            player2Paddle.Draw(spriteBatch);
            ball.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

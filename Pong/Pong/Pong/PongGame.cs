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


        // screen resolution
        const int X_RESOLUTION = 1280;
        const int Y_RESOLUTION = 720;

        // pause menu
        Texture2D overlay;
        Texture2D pauseMenu;
        Vector2 pauseMenuPosition;

        // game objects
        Paddle player1Paddle;
        Paddle player2Paddle;
        Ball ball;

        // sound effects
        public SoundEffectInstance Music;
        SoundEffect beep;

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
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
        }
        public static GameState currentGameState = GameState.MainMenu;
        
        // input handling
        KeyboardState prevKeyboardState;

        /// <summary>
        /// PongGame constructor. Setup screen size and background.
        /// </summary>
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

            InitPauseMenu();
            base.Initialize();
        }

        // Helper method for creating the pause menu overlay
        private void InitPauseMenu()
        {
            overlay = new Texture2D(GraphicsDevice, X_RESOLUTION, Y_RESOLUTION, false, SurfaceFormat.Color);
            // set the color to the amount of pixels
            Color[] overlayData = new Color[X_RESOLUTION * Y_RESOLUTION];

            // loop through all the colors setting them to whatever values we want
            for (int i = 0; i < overlayData.Length; i++)
            {
                overlayData[i] = new Color(255, 255, 255, 120);
            }

            // set the color data on the texture
            overlay.SetData(overlayData);


            pauseMenuPosition = new Vector2(200, 100);
            pauseMenu = new Texture2D(GraphicsDevice, X_RESOLUTION - 400, Y_RESOLUTION - 200, false, SurfaceFormat.Color);
            // set the color to the amount of pixels
            Color[] pauseMenuData = new Color[(X_RESOLUTION - 400) * (Y_RESOLUTION - 200)];

            // loop through all the colors setting them to whatever values we want
            for (int i = 0; i < pauseMenuData.Length; i++)
            {
                pauseMenuData[i] = new Color(255, 255, 255, 255);
            }

            // set the color data on the texture
            pauseMenu.SetData(pauseMenuData);
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
            ball = new Ball(this, tempTexture, screenRectangle);
            tempTexture = Content.Load<Texture2D>("paddle");
            player1Paddle = new Paddle(tempTexture, screenRectangle, 1);
            player2Paddle = new Paddle(tempTexture, screenRectangle, 2);
            font = Content.Load<SpriteFont>("font");
            bgcolor = Color.DarkSlateGray;

            SoundEffect music = Content.Load<SoundEffect>("music");
            Music = music.CreateInstance();
            Music.Volume = 0.05f;
            Music.IsLooped = true;
            beep = Content.Load<SoundEffect>("beep");

            // initialize save game data
            highScoreCount = 10;
            saveGameData.highScore = new int[highScoreCount];
            for (int i = 0; i < highScoreCount; i++)
            {
                saveGameData.highScore[i] = 0;
            }
            // Load save game high score into saveGameData struct
            GetDevice();

            StartGame();
            
        }

        /// <summary>
        /// StartGame() reloads level to beginning state
        /// </summary>
        protected void StartGame()
        {
            Load();
            
            pongScore = 0;
            scoreSaved = false;

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
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePad1 = GamePad.GetState(PlayerIndex.One);
            GamePadState currentGamePad2 = GamePad.GetState(PlayerIndex.Two);
            // Allows the game to exit
            if (currentGamePad1.Buttons.Back == ButtonState.Pressed ||
                currentGamePad2.Buttons.Back == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            HandlePauseGameInput(currentKeyboardState, currentGamePad1, currentGamePad2);
            HandleRestartGameInput(currentKeyboardState, currentGamePad1, currentGamePad2);

            // play game is playing state and players both have life
            if (currentGameState == GameState.Playing && player1_Lives > 0 && player2_Lives > 0)
            {
                //update ball and paddle
                player1Paddle.Update();
                player2Paddle.Update();
                ball.Update(beep);
                ball.PaddleCollision(player1Paddle.GetBounds(), player2Paddle.GetBounds(), beep);
                LoadHighScore();
            } else if(player1_Lives == 0 || player2_Lives  == 0) // save game if a player has 0 life
            {
                if (!scoreSaved)
                    UpdateSavedHighScore();

                Music.Stop();
            }

            
            prevKeyboardState = currentKeyboardState;

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

        // help handles pause game input and pause the music when paused
        private void HandlePauseGameInput(KeyboardState currentKeyboardState, GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            if ((currentKeyboardState.IsKeyUp(Keys.P) && prevKeyboardState.IsKeyDown(Keys.P)) ||
                currentGamePad1.Buttons.Start == ButtonState.Pressed ||
                currentGamePad2.Buttons.Start == ButtonState.Pressed)
                if (currentGameState == GameState.Paused)
                {
                    currentGameState = GameState.Playing;
                    Music.Play();
                }
                else
                {
                    currentGameState = GameState.Paused;
                    Music.Pause();
                }

        }
        
        // helps handles the restart game request
        private void HandleRestartGameInput(KeyboardState currentKeyboardState, GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            if (currentGameState == GameState.Paused)
            {
                if ((currentKeyboardState.IsKeyUp(Keys.R) && prevKeyboardState.IsKeyDown(Keys.R)) ||
                    currentGamePad1.Buttons.RightShoulder == ButtonState.Pressed ||
                    currentGamePad2.Buttons.RightShoulder == ButtonState.Pressed)
                {
                    StartGame();
                    currentGameState = GameState.Playing;
                }
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

            // print player life and score to screen
            spriteBatch.DrawString(font, player1_Lives.ToString(), player1LifePosition, Color.White);
            spriteBatch.DrawString(font, player2_Lives.ToString(), player2LifePosition, Color.White);
            spriteBatch.DrawString(font, "Score: " + pongScore, scorePosition, Color.White);
            player1Paddle.Draw(spriteBatch);
            player2Paddle.Draw(spriteBatch);
            ball.Draw(spriteBatch);

            if (currentGameState == GameState.Paused)
            {
                spriteBatch.Draw(overlay, new Vector2(0, 0), Color.Black);
                spriteBatch.Draw(pauseMenu, pauseMenuPosition, Color.DodgerBlue);
                
                Vector2 textPosition = new Vector2(0, 0);
                RenderTarget2D renderTarget = new RenderTarget2D(GraphicsDevice,
                    (int)font.MeasureString("High Scores").X + 10,
                    (int)font.MeasureString("High Scores").Y + 10);
                textPosition.X = (X_RESOLUTION / 2) - (renderTarget.Width / 2);
                textPosition.Y = pauseMenuPosition.Y + 10;
                
                spriteBatch.DrawString(font, "High Scores", textPosition, Color.White);
                for ( int i = 0 ; i < saveGameData.highScore.Count() ; i++ )
                {
                    string scoreText =  i + 1 + " ----------------- " + saveGameData.highScore[ i ];
                    renderTarget = new RenderTarget2D(GraphicsDevice,
                    (int)font.MeasureString(scoreText).X + 10,
                    (int)font.MeasureString(scoreText).Y + 10);
                    Vector2 position = textPosition + new Vector2( 0, 40 * (i + 1) );
                    position.X = (X_RESOLUTION / 2) - (renderTarget.Width / 2);
                    spriteBatch.DrawString( font, scoreText, position, Color.White );
                }

                string infoText = "Press 'P/Start' to resume or 'R/Right Shoulder' to restart";
                renderTarget = new RenderTarget2D(GraphicsDevice,
                (int)font.MeasureString(infoText).X + 10,
                (int)font.MeasureString(infoText).Y + 10);
                textPosition.X = (X_RESOLUTION / 2) - (renderTarget.Width / 2);
                textPosition.Y = pauseMenuPosition.Y + pauseMenu.Height - 40;
                spriteBatch.DrawString(font, infoText, textPosition, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

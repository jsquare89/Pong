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
    class Paddle
    {
        Texture2D texture;
        Rectangle screenBounds;

        Vector2 motion;
        Vector2 position;
        float paddleSpeed = 15f;

        KeyboardState keyboard;
        GamePadState playerGamePad;

        int playerIndex;


        public Paddle(Texture2D texture, Rectangle screenBounds, int playerIndex)
        {
            this.playerIndex = playerIndex;
            this.texture = texture;
            this.screenBounds = screenBounds;
            
            StartPosition();
        }

        public void Update()
        {
            motion = Vector2.Zero;
            keyboard = Keyboard.GetState();
            if (playerIndex == 1)
            {
                playerGamePad = GamePad.GetState(PlayerIndex.One);
                p1HandleInput();
            }
            if(playerIndex ==2)
            {
                playerGamePad = GamePad.GetState(PlayerIndex.Two);
                p2HandleInput();
            }
            motion.Y *= paddleSpeed;
            position += motion;
            LockPaddle();            
        }

        private void p1HandleInput()
        {
            if (keyboard.IsKeyDown(Keys.W) ||
                playerGamePad.IsButtonDown(Buttons.LeftThumbstickUp) ||
                playerGamePad.IsButtonDown(Buttons.DPadUp))
                motion.Y = -1;
            if (keyboard.IsKeyDown(Keys.S) ||
                playerGamePad.IsButtonDown(Buttons.LeftThumbstickDown) ||
                playerGamePad.IsButtonDown(Buttons.DPadDown))
                motion.Y = 1;
        }


        private void p2HandleInput()
        {
            if (keyboard.IsKeyDown(Keys.Up) ||
                playerGamePad.IsButtonDown(Buttons.LeftThumbstickUp) ||
                playerGamePad.IsButtonDown(Buttons.DPadUp))
                motion.Y = -1;
            if (keyboard.IsKeyDown(Keys.Down) ||
                playerGamePad.IsButtonDown(Buttons.LeftThumbstickDown) ||
                playerGamePad.IsButtonDown(Buttons.DPadDown))
                motion.Y = 1;
        }

        private void LockPaddle()
        {
            if (position.Y < 0)
                position.Y = 0;
            if (position.Y + texture.Height > screenBounds.Height)
                position.Y = screenBounds.Height - texture.Height;
        }

        public void StartPosition()
        {
            if (playerIndex == 1)
            {
                position.X = 0;
                position.Y = (screenBounds.Height - texture.Height) / 2;
            }
            else if(playerIndex ==2)
            {
                position.X = screenBounds.Width - texture.Width;
                position.Y = (screenBounds.Height - texture.Height) / 2;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
        }
    }
}

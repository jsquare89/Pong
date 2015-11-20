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
        float rotationAngle;

        KeyboardState keyboard;
        GamePadState gamepad;

        public Paddle(Texture2D texture, Rectangle screenBounds)
        {
            this.texture = texture;
            rotationAngle = MathHelper.PiOver2;
            this.screenBounds = screenBounds;
            
            StartPosition();
        }

        public void Update()
        {

            motion = Vector2.Zero;

            keyboard = Keyboard.GetState();
            gamepad = GamePad.GetState(PlayerIndex.One);

            if (keyboard.IsKeyDown(Keys.Up) ||
                gamepad.IsButtonDown(Buttons.LeftThumbstickUp) ||
                gamepad.IsButtonDown(Buttons.DPadUp))
                motion.Y = -1;
            if (keyboard.IsKeyDown(Keys.Down) ||
                gamepad.IsButtonDown(Buttons.LeftThumbstickDown) ||
                gamepad.IsButtonDown(Buttons.DPadDown))
                motion.Y = 1;

            motion.Y *= paddleSpeed;
            position += motion;
            LockPaddle();            
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
            position.X = screenBounds.Width - texture.Width;
            position.Y = (screenBounds.Height - texture.Height) /2;
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

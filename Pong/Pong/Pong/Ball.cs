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
    class Ball
    {
        Texture2D texture;
        Rectangle screenBounds;

        Vector2 motion;
        Vector2 position;
        Rectangle bounds;
        bool collided;
        bool gameStart;

        float ballSpeed = 4;


        float ballStartSpeed = 10f;

        public void ModifyBallSpeed(float speed)
        {
            this.ballSpeed += speed;
        }

        // return the bounding rectangle of the ball
        public Rectangle Bounds
        {
            get
            {
                bounds.X = (int)position.X;
                bounds.Y = (int)position.Y;
                return bounds;
            }
        }

        public Ball(Texture2D texture, Rectangle screenBounds)
        {
            bounds = new Rectangle(0, 0, texture.Width, texture.Height);
            this.texture = texture;
            this.screenBounds = screenBounds;
            gameStart = false;
        }

        public void setStart()
        {
            gameStart = false;
        }

        public void Update()
        {
            collided = false;

            // start the ball
            if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
                gameStart = true;

            // set the ball motion and check for collisions
            if (gameStart)
            {
                position += motion * ballSpeed;
                ballSpeed += 0.001f;

                CheckWallCollision();
            }
            else
            {
                //reset ball to starting paddle
                StartPosition();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void CheckWallCollision()
        {
            // check against left bounding
            if (position.X < 0)
            {
                //position.X = 0;
                //motion.X *= -1;

                // reset ball and increase player 2 score
                StartPosition();
                PongGame.player1_Lives -= 1;

            }
            // check against right bounding
            if (position.X + texture.Width > screenBounds.Width)
            {
                //position.X = screenBounds.Width - texture.Width;
                //motion.X *= -1;

                // reset ball and increase player 2 score
                StartPosition();
                PongGame.player2_Lives -= 1;
            }
            // check against top bounding
            if (position.Y < 0)
            {
                position.Y = 0;
                motion.Y *= -1;

                
            }
            // check against bottom bounding
            if(position.Y > screenBounds.Height - texture.Height)
            {
                position.Y = screenBounds.Height - texture.Height;
                motion.Y *= -1;
            }
        }


        public void StartPosition()
        {
            Random rand = new Random();

            // use rand to get a random direction for ball to start with
            motion = new Vector2((float)rand.NextDouble() - 0.5f, (float)(rand.NextDouble() - 0.5f) / 1.5f);
            motion.Normalize();

            ballSpeed = ballStartSpeed;
            
            // set ball position
            position.X = screenBounds.Width /2 ;
            position.Y = screenBounds.Height /2;
        }

        public void PaddleCollision(Rectangle paddle1, Rectangle paddle2)
        {
            // rectangle to check for collision
            Rectangle ballLocation = new Rectangle(
                (int)position.X,
                (int)position.Y,
                texture.Width,
                texture.Height);

            // check collision with paddle and change direction of the ball
            if (paddle1.Intersects(ballLocation))
            {
                position.X = paddle1.X + texture.Width;
                motion.X *= -1;
                PongGame.pongScore++;
            }
            if (paddle2.Intersects(ballLocation))
            {
                position.X = paddle2.X - texture.Height;
                motion.X *= -1;
                PongGame.pongScore++;
            }
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.LightBlue);
        }
    }
}

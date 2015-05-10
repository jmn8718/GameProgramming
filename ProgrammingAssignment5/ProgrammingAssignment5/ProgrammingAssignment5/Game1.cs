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

namespace ProgrammingAssignment5
{
    using TeddyMineExplosion;
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WINDOW_WIDTH = 800;
        const int WINDOW_HEIGHT = 600;

        //mine support
        Texture2D mineSprite;
        List<Mine> mines = new List<Mine>();

        // random teddy bear support
        Random rand = new Random();
        Texture2D teddyBearSprite;
        List<TeddyBear> bears = new List<TeddyBear>();

        // spawning support
        int spawnTimer = 0;
        int elapsedSpawnDelayMilliseconds = 0;
        
        // explosion support
        Texture2D explosionSprite;
        List<Explosion> explosions = new List<Explosion>();

        // click processing
        bool leftClickStarted = false;
        bool leftButtonReleased = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //set resolution and make mouse visible
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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

            // load sprites
            mineSprite = Content.Load<Texture2D>("mine");
            teddyBearSprite = Content.Load<Texture2D>("teddybear");
            explosionSprite = Content.Load<Texture2D>("explosion");

            spawnTimer = rand.Next(1, 4) * 1000;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            elapsedSpawnDelayMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
            if (elapsedSpawnDelayMilliseconds >= spawnTimer)
            {
                elapsedSpawnDelayMilliseconds = 0;
                spawnTimer = rand.Next(1, 4)*1000;
                float xSpeed = (float) (rand.NextDouble()-0.5);
                float ySpeed = (float) (rand.NextDouble()-0.5);
                bears.Add(new TeddyBear(teddyBearSprite,new Vector2(xSpeed,ySpeed),WINDOW_WIDTH,WINDOW_HEIGHT));
            }

            foreach (TeddyBear bear in bears)
            {
                bear.Update(gameTime);
                int i=0;
                while (bear.Active && i<mines.Count)
                {
                    if (mines[i].Active && bear.CollisionRectangle.Intersects(mines[i].CollisionRectangle))
                    {
                        bear.Active = false;
                        mines[i].Active = false;
                        explosions.Add(new Explosion(explosionSprite, mines[i].CollisionRectangle.Center.X, mines[i].CollisionRectangle.Center.Y));
                    }
                    i++;
                }
            }

            // update explosions
            foreach (Explosion explosion in explosions)
            {
                explosion.Update(gameTime);
            }

            // remove dead teddies
            for (int i = bears.Count - 1; i >= 0; i--)
            {
                if (!bears[i].Active)
                {
                    bears.RemoveAt(i);
                }
            }
            // remove exploded mines
            for (int i = mines.Count - 1; i >= 0; i--)
            {
                if (!mines[i].Active)
                {
                    mines.RemoveAt(i);
                }
            }

            // remove dead explosions
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                if (!explosions[i].Playing)
                {
                    explosions.RemoveAt(i);
                }
            }

            MouseState mouse = Mouse.GetState();

            // check for left click started
            if (mouse.RightButton == ButtonState.Pressed &&
                leftButtonReleased)
            {
                leftClickStarted = true;
                leftButtonReleased = false;
            }
            else if (mouse.LeftButton == ButtonState.Released)
            {
                leftButtonReleased = true;

                // if left click finished, add new mine to list
                if (leftClickStarted)
                {
                    leftClickStarted = false;

                    // STUDENTS: add a new pickup to the end of the list of pickups
                    mines.Add(new Mine(mineSprite,mouse.X,mouse.Y));

                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            foreach(Mine mine in mines){
                mine.Draw(spriteBatch);
            }

            foreach (TeddyBear bear in bears)
            {
                bear.Draw(spriteBatch);
            }

            foreach (Explosion explosion in explosions)
            {
                explosion.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

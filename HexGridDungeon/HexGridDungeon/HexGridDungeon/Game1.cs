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

using HexGridDungeon.WorldGeneration;

namespace HexGridDungeon
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D SpriteTexture;
		
		DungeonGenerator hexDungeon;
		int spriteHeight, spriteWidth;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
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
            this.IsMouseVisible = true;

			hexDungeon = new DungeonGenerator(7, 5, this);
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			SpriteTexture = this.Content.Load<Texture2D>("BlankHexShape");

			spriteWidth = SpriteTexture.Width;
			spriteHeight = SpriteTexture.Height;
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

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin();

			Tuple<int, int> pos3Tup = XYConverter(0, 0);
			Vector2 pos;

			for (int i = 0; i <= hexDungeon.Width; i++)
			{
				for (int j = 0; j <= hexDungeon.Height; j++)
				{

					pos3Tup = XYConverter(i, j);
					pos = new Vector2(pos3Tup.Item1, pos3Tup.Item2);
                    if (hexDungeon.Stage.GetTile(new Tuple<int, int>(i, j)) != null)
                        SpriteTexture = GetSpriteTexture(hexDungeon.Stage.GetTile(new Tuple<int, int>(i, j)).GetSpriteID);
                    else
                        SpriteTexture = this.Content.Load<Texture2D>("NullHexShape");
                    spriteBatch.Draw(SpriteTexture, pos, Color.White);
				}
			}

			spriteBatch.End();
			// TODO: Add your drawing code here

			base.Draw(gameTime);
		}

		public void DrawState(HexGrid _hexGrid)
		{
			GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin();

			Tuple<int, int> pos3Tup = XYConverter(0, 0);
			Vector2 pos;

			for (int i = 0; i < _hexGrid.Width; i++)
			{
				for (int j = 0; j < _hexGrid.Height; j++)
				{

					pos3Tup = XYConverter(i, j);
					pos = new Vector2(pos3Tup.Item1, pos3Tup.Item2);
					SpriteTexture = GetSpriteTexture(hexDungeon.Stage.GetTile(new Tuple<int, int>(i, j)).GetSpriteID);
					spriteBatch.Draw(SpriteTexture, pos, Color.White);
				}
			}

			spriteBatch.End();
		}

		protected Tuple<int, int> XYConverter(int x, int y)
		{
			int newX = 0;
			int newY = 0;

			if (x % 2 == 0)
			{
				newX = (int)(x * (0.75 * spriteWidth));
				newY = (int)(y * spriteWidth);
			}
			else if (x % 2 == 1)
			{
				newX = (int)(x * (0.75 * spriteWidth));
				newY = (int)(y * spriteWidth + (spriteWidth / 2));
			}

			return new Tuple<int, int>(newX, newY);
		}

		protected Texture2D GetSpriteTexture(string texture)
		{
			if (texture == null)
				return this.Content.Load<Texture2D>("NullHexShape");
			else
				return this.Content.Load<Texture2D>(texture);
		}
	}
}

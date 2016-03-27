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
	public class GameMain : Microsoft.Xna.Framework.Game
	{
		// Data - XNA Default
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D SpriteTexture;
		
		// Data - Map Generation
		DungeonGenerator hexDungeonGenerator;
		HexGrid stage;

		// Data - Drawing
		int spriteHeight, spriteWidth;


		// Constructor
		public GameMain()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		#region XNA Core Code

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
            this.IsMouseVisible = true;

			//RoomGenerator myRoomGenerator = new RoomGenerator(1, 20);
			DungeonGenerator myDungeonGenerator = new DungeonGenerator(61, 27);

			//stage = myRoomGenerator.BuildWaterRoom(9, 9);
			stage = myDungeonGenerator.Stage;
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			SpriteTexture = this.Content.Load<Texture2D>("NullHexShape");

			// Assuming all hex's are the same size as the null hex
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
			bool pressed = false;

			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Enter)) { pressed = true; }
			while (Mouse.GetState().LeftButton == ButtonState.Pressed) { pressed = true; }

			// regenerate the map if pressed = true
			if (pressed)
				this.Initialize();

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

			Tuple<int, int> PixelPosition = CoordinateConverter(0, 0);
			Vector2 VectorPixelPosition;

			// For every cell in the stage
			for (int i = 0; i < stage.Width; i++)
			{
				for (int j = 0; j < stage.Height; j++)
				{
					// Get position
					PixelPosition = CoordinateConverter(i, j);
					VectorPixelPosition = new Vector2(PixelPosition.Item1, PixelPosition.Item2);

					// Load sprite if hex tile is not null
					if (stage.GetTile(new Tuple<int, int>(i, j)) != null)
                        SpriteTexture = GetSpriteTexture(stage.GetTile(new Tuple<int, int>(i, j)).GetSpriteID);
					// Else load a placeholder null hex
					else
                        SpriteTexture = this.Content.Load<Texture2D>("NullHexShape");

					// Draw code
					spriteBatch.Draw(SpriteTexture, VectorPixelPosition, Color.White);
				}
			}

			spriteBatch.End();

			base.Draw(gameTime);
		}

		#endregion


		// Helper functions
		protected Tuple<int, int> CoordinateConverter(int x, int y)
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

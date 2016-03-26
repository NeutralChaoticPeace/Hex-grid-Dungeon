using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexGridDungeon.WorldGeneration
{
    public class DungeonGenerator
    {
        // Data
        private Game1 gameRef;
        private HexGrid stage;


        // Properties
        public int Width
        {
            get { return stage.Width; }
        }

        public int Height
        {
            get { return stage.Height; }
        }

        public HexGrid Stage
        {
            get { return stage; }
        }


        // Constructors
        public DungeonGenerator(Game1 game)
        {
            stage = new HexGrid();
            Initialize(game);
        }

        public DungeonGenerator(int size, Game1 game)
        {
            stage = new HexGrid(size);
            Initialize(game);
        }

        public DungeonGenerator(int width, int height, Game1 game)
        {
            stage = new HexGrid(width, height);
            Initialize(game);
        }


        // Initialize
        private void Initialize(Game1 game)
        {
            gameRef = game;

            GenerateBorderWalls();
            CreateFloor(new Tuple<int, int>(5, 5));

        }


        // Procedural Generation
        private void GenerateBorderWalls()
        {
            for(int x = 0; x < stage.Width; x++)
            {
                CreateWall(new Tuple<int, int>(x, 0));
                CreateWall(new Tuple<int, int>(x, stage.Height - 1));
            }

            for (int y = 0; y < stage.Height; y++)
            {
                CreateWall(new Tuple<int, int>(0, y));
                CreateWall(new Tuple<int, int>(stage.Width - 1, y));
            }
        }

        private void GenerateRooms()
        {

        }

        private void GeneratePaths()
        {

        }




        // Tile Specific Operations
        private void CreateWall(Tuple<int, int> coordinate)
        {
            if (stage.IsValidCoordinate(coordinate))
                stage.SetTile(coordinate, new Tiles.TileTypes.Wall());
        }

        private void CreateFloor(Tuple<int, int> coordinate)
        {
            if (stage.IsValidCoordinate(coordinate))
                stage.SetTile(coordinate, new Tiles.TileTypes.Floor());
        }

    }
}

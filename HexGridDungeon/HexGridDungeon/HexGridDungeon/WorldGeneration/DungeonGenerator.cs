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
        }


        // Procedural Generation
        private void GenerateRooms()
        {

        }

        private void GeneratePaths()
        {

        }

    }
}

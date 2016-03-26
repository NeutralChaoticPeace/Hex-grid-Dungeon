using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexGridDungeon.WorldGeneration
{
    public class DungeonGenerator
    {
        // Data
        private static readonly int MinRoomSize = 5;
        private static readonly int MaxRoomSize = 15;
        private Game1 gameRef;
        private HexGrid stage;


        HashSet<HexGrid> DungeonRooms = new HashSet<HexGrid>();

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

            GenerateRooms();

            GeneratePaths();

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


        // ROOMS
        private void GenerateRooms()
        {
            RoomGenerator roomGenerator = new RoomGenerator(MinRoomSize, MaxRoomSize);

            // Initial room square
            int TrySize = Rand.GetInstance().Next(MinRoomSize, (int)Math.Floor(MaxRoomSize*0.75));
            int TrySizeX = TrySize;
            int TrySizeY = TrySize;

            // rectangle modifier
            if(Rand.GetInstance().Next(0, 100) <= 50)
            {
                if(Rand.GetInstance().Next() % 2 == 0)
                    TrySizeX += Rand.GetInstance().Next(1, (MaxRoomSize - TrySizeX));
                else
                    TrySizeY += Rand.GetInstance().Next(1, (MaxRoomSize - TrySizeY));
            }

            // room coordinates
            int TryPlaceX = Rand.GetInstance().Next(0, stage.Width - 1);
            int TryPlaceY = Rand.GetInstance().Next(0, stage.Height - 1);

            // check new room.
            if(IsValidRoomPlacement(TryPlaceX, TryPlaceY, TrySizeX, TrySizeY))
            {
                HexGrid room = roomGenerator.GenerateNewRoom(TrySizeX, TryPlaceY);
                if(TryPlaceRoom(room))
                    DungeonRooms.Add(room);
            }
        }

        private bool IsValidRoomPlacement(int _x, int _y, int _width, int _height)
        {
            for(int x = _x; x < _width + _x; x ++)
            {
                for(int y = _y; y < _height + _y; y++)
                {
                    if (!stage.IsValidCoordinate(new Tuple<int, int>(x, y)))
                        return false;
                    if (stage.GetTile(new Tuple<int, int>(x, y)) != null)
                        return false;
                }
            }

            return true;
        }

        private bool TryPlaceRoom(HexGrid _room)
        {
            return false;
        }


        // PATHS
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

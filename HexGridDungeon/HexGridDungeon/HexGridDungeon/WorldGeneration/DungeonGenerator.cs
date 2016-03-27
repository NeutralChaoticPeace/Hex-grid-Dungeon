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
        private static readonly int MaxRoomSize = 19;
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
        public DungeonGenerator()
        {
            Initialize(21, 25);
        }

        public DungeonGenerator(int size)
        {
            Initialize(size, size);
        }

        public DungeonGenerator(int width, int height)
        {
            Initialize(width, height);
        }


        // Initialize
        private void Initialize(int width, int height)
        {
            // Make dungeon odd sized
            if (width % 2 == 0)
                width += 1;
            if (height % 2 == 0)
                height += 1;
            stage = new HexGrid(width, height);

            GenerateBorderWalls();

            GenerateRooms();

            GeneratePaths();

        }


        // Procedural Generation
        private void GenerateBorderWalls()
        {
            //for(int x = 0; x < stage.Width; x++)
            //{
            //    CreateWall(new Tuple<int, int>(x, 0));
            //    CreateWall(new Tuple<int, int>(x, stage.Height - 1));
            //}

            //for (int y = 0; y < stage.Height; y++)
            //{
            //    CreateWall(new Tuple<int, int>(0, y));
            //    CreateWall(new Tuple<int, int>(stage.Width - 1, y));
            //}

            stage.SetBorder(new Tiles.TileTypes.Wall());
        }


        // ROOMS
        private void GenerateRooms()
        {         
            int RoomTries = Math.Max(stage.Width, stage.Height);

            for (int i = 0; i < RoomTries; i++)
            {
                GenerateRoom();
            }
        }

        private void GenerateRoom()
        {
            RoomGenerator roomGenerator = new RoomGenerator(MinRoomSize, MaxRoomSize);

            // Initial room square
            int TrySize = Rand.GetInstance().Next(MinRoomSize, (int)Math.Floor(MaxRoomSize * 0.75));
            int TrySizeX = TrySize;
            int TrySizeY = TrySize;

            // rectangle modifier
            if (Rand.GetInstance().Next(0, 100) <= 50)
            {
                if (Rand.GetInstance().Next() % 2 == 0)
                    TrySizeX += Rand.GetInstance().Next(1, (MaxRoomSize - TrySizeX));
                else
                    TrySizeY += Rand.GetInstance().Next(1, (MaxRoomSize - TrySizeY));
            }

            // enforce odd room width / height
            if (TrySizeX % 2 == 0)
                TrySizeX += 1;
            if (TrySizeY % 2 == 0)
                TrySizeY += 1;

            // enforce room size
            if (TrySizeX > MaxRoomSize)
                TrySizeX = MaxRoomSize;

            if (TrySizeY > MaxRoomSize)
                TrySizeY = MaxRoomSize;

            if (TrySizeX < MinRoomSize)
                TrySizeX = MinRoomSize;
    
            if (TrySizeY < MinRoomSize)
                TrySizeY = MinRoomSize;

            // room coordinates
            int TryPlaceX = Rand.GetInstance().Next(2, stage.Width - 3);
            int TryPlaceY = Rand.GetInstance().Next(2, stage.Height - 3);

            // enforce odd room location
            if (TryPlaceX % 2 == 1)
                TryPlaceX += 1;
            if (TryPlaceY % 2 == 1)
                TryPlaceY += 1;

            // check new room.
            if (IsValidRoomPlacement(TryPlaceX, TryPlaceY, TrySizeX, TrySizeY))
            {
                HexGrid room = roomGenerator.GenerateNewRoom(TrySizeX, TrySizeY);
                if (TryPlaceRoom(TryPlaceX, TryPlaceY, room))
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

        private bool TryPlaceRoom(int _x, int _y, HexGrid _room)
        {
            for(int x = 0; x < _room.Width; x++)
            {
                for (int y = 0; y < _room.Height; y++)
                {
                    stage.SetTile(new Tuple<int, int>(x + _x, y + _y), _room.GetTile(new Tuple<int, int>(x, y)));
                }
            }
            
            return true;
        }
		
        // PATHS
        private void GeneratePaths()
        {
			for(int x = 0; x < stage.Width; x++)
			{
				for (int y = 0; y < stage.Height; y++)
				{
					if (stage.GetTile(new Tuple<int, int>(x, y)) == null)
					{
						GenerateMaze(x, y);
						return;
					}
				}
			}
        }

		private void GenerateMaze(int x, int y)
		{
			Stack<Tuple<int, int>> unfinishedNodes = new Stack<Tuple<int, int>>();
			// location x,y is a floor
			stage.SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.Floor());
			// push x,y onto the stack
			unfinishedNodes.Push(new Tuple<int, int>(x, y));


			// while stack is not empty
			while(unfinishedNodes.Count > 0)
			{
				// if possible, get every location pathable from the top of the stack
				List<Tuple<int, int>> pathableLocations = CartesianPathableLocations(x, y);
				Tuple<int, int> nextLocation;
				// pick a random one and go 
				if (pathableLocations.Count > 0)
				{
					nextLocation = pathableLocations[Rand.GetInstance().Next(0, pathableLocations.Count)];

					// that location gets the above logic (turn it into a floor, turn in-between spot into a floor)
					// push that location on stack
					stage.SetTile(nextLocation, new Tiles.TileTypes.Floor());
					unfinishedNodes.Push(nextLocation);
				}
				// else if its not possible to get every location (nowhere to path to)
				else
				{
					// remove element from stack and continue
					unfinishedNodes.Pop();
				}
			}
		}

		private List<Tuple<int, int>> CartesianPathableLocations(int x, int y)
		{
			List<Tuple<int, int>> pathableList = new List<Tuple<int, int>>();

			if (stage.GetTile(new Tuple<int, int>(x + 2, y)) != null)
				pathableList.Add(new Tuple<int, int>(x + 2, y));

			if (stage.GetTile(new Tuple<int, int>(x - 2, y)) != null)
				pathableList.Add(new Tuple<int, int>(x - 2, y));

			if (stage.GetTile(new Tuple<int, int>(x, y + 2)) != null)
				pathableList.Add(new Tuple<int, int>(x, y + 2));

			if (stage.GetTile(new Tuple<int, int>(x, y - 2)) != null)
				pathableList.Add(new Tuple<int, int>(x, y - 2));

			return pathableList;

		}

		private List<Tuple<int, int>> HexPathableLocations(int x, int y)
		{
			List<Tuple<int, int>> pathableList = new List<Tuple<int, int>>();

			// check straight down
			Tuple<int, int> temp = stage.GetNextValidStep(new Tuple<int, int>(x, y), HexGrid.Direction.Down);
			if (temp != null)
				pathableList.Add(temp);

			// check straight up
			temp = stage.GetNextValidStep(new Tuple<int, int>(x, y), HexGrid.Direction.Up);
			if (temp != null)
				pathableList.Add(temp);

			// check straight left down
			temp = stage.GetNextValidStep(new Tuple<int, int>(x, y), HexGrid.Direction.LeftDown);
			if (temp != null)
				pathableList.Add(temp);

			// check straight left up
			temp = stage.GetNextValidStep(new Tuple<int, int>(x, y), HexGrid.Direction.LeftUp);
			if (temp != null)
				pathableList.Add(temp);

			// check straight right down
			temp = stage.GetNextValidStep(new Tuple<int, int>(x, y), HexGrid.Direction.RightDown);
			if (temp != null)
				pathableList.Add(temp);

			// check straight right up
			temp = stage.GetNextValidStep(new Tuple<int, int>(x, y), HexGrid.Direction.RightUp);
			if (temp != null)
				pathableList.Add(temp);

			return pathableList;
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

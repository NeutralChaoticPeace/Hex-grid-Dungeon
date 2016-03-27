using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexGridDungeon.WorldGeneration
{
    public class DungeonGenerator
    {
        // Data
        private static readonly int MinRoomSize = 7;
        private static readonly int MaxRoomSize = 15;
        private HexGrid stage;


        HashSet<HexGrid> DungeonRooms = new HashSet<HexGrid>();
        HashSet<Tuple<int, int>> DungeonPaths = new HashSet<Tuple<int, int>>();
        HashSet<Tuple<int, int>> DungeonEntries = new HashSet<Tuple<int, int>>();


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

			ConvertNullToWall();

            GenerateEntries();
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
            int RoomTries = stage.Width * Stage.Height;

            for (int i = 0; i < RoomTries; i++)
            {
                GenerateRoom();
            }
        }

        private void GenerateRoom()
        {
            RoomGenerator roomGenerator = new RoomGenerator();

            // Initial room square
            int TrySize = Rand.GetInstance().Next(MinRoomSize, (int)Math.Floor(MaxRoomSize * 0.85));
            int TrySizeX = TrySize;
            int TrySizeY = TrySize;

            // rectangle modifier
            if (Rand.GetInstance().Next(0, 100) <= 75)
            {
                if (Rand.GetInstance().Next() % 2 == 0)
                    TrySizeX += Rand.GetInstance().Next(Math.Min((int)(TrySizeX * 0.10), MaxRoomSize), Math.Min((int)(TrySizeX*0.25), MaxRoomSize));
                else
                    TrySizeY += Rand.GetInstance().Next(Math.Min((int)(TrySizeY * 0.10), MaxRoomSize), Math.Min((int)(TrySizeY * 0.25), MaxRoomSize));
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

			// location x,y is a floor and start location
			stage.SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.Floor());
			unfinishedNodes.Push(new Tuple<int, int>(x, y));

			// while stack is not empty
			while(unfinishedNodes.Count > 0)
			{
                // if possible, get every location pathable from the top of the stack
                int currentX = unfinishedNodes.Peek().Item1;
                int currentY = unfinishedNodes.Peek().Item2;
                List<Tuple<int, int>> pathableLocations = HexPathableLocations(currentX, currentY);
				Tuple<int, int> nextLocation;

				// if possible, pick a random one and go 
				if (pathableLocations.Count > 0)
				{
					nextLocation = pathableLocations[Rand.GetInstance().Next(0, pathableLocations.Count)];

					// that location gets the above logic (turn it into a floor, turn in-between spot into a floor)
					// push that location on stack
					stage.SetStep(unfinishedNodes.Peek(), stage.GetStepDirection(unfinishedNodes.Peek(), nextLocation), new Tiles.TileTypes.Floor());
                    //stage.SetTile(nextLocation, new Tiles.TileTypes.Floor());
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

		private List<Tuple<int, int>> HexPathableLocations(int x, int y)
		{
			List<Tuple<int, int>> pathableList = new List<Tuple<int, int>>();

            foreach (HexGrid.Direction value in Enum.GetValues(typeof(HexGrid.Direction)))
            {
                Tuple<int, int> temp = stage.GetNextValidStep(new Tuple<int, int>(x, y), value);

                if (stage.GetTile(temp) == null && stage.IsValidCoordinate(temp))
                    pathableList.Add(temp);
            }

            return pathableList;
		}

        private void ConvertNullToWall()
        {
            for (int x = 0; x < stage.Width; x++)
            {
                for (int y = 0; y < stage.Height; y++)
                {
                    if (stage.GetTile(new Tuple<int, int>(x, y)) == null)
                        stage.SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.Wall());
                }
            }
        }


        // ENTRIES
        private void GenerateEntries()
        {
            foreach(HexGrid room in DungeonRooms)
            {
                GenerateEntry(room);
            }
        }

        private void GenerateEntry(HexGrid room)
        {
            // get list of possible doors
            List<Tuple<int, int>> PossibleEntries = GetPossibleEntries(room);
            Tuple<int, int> Entry = PossibleEntries[Rand.GetInstance().Next(0, PossibleEntries.Count)];

            // place door
            room.SetTile(Entry, new Tiles.TileTypes.Floor());
            

            // 50% chance for N+1 doors
            if (Rand.GetInstance().Next(0, 100) <= 50)
                GenerateEntry(room);
        }

        private List<Tuple<int, int>> GetPossibleEntries(HexGrid _room)
        {
            List<Tuple<int, int>> ReturnList = new List<Tuple<int, int>>();

            for(int x = 0; x < _room.Width; x++)
            {
                for (int y = 0; y < _room.Height; y++)
                {
                    if (_room.GetTile(new Tuple<int, int>(x, y)) is Tiles.TileTypes.Wall)
                        ReturnList.Add(new Tuple<int, int>(x, y));
                }
            }

            return ReturnList;
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

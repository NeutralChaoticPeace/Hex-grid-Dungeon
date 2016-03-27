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


        // DungeonRooms<x, y, room>
        HashSet<Tuple<int, int, HexGrid>> DungeonRooms = new HashSet<Tuple<int, int, HexGrid>>();
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

			while (FindDeadEnds());

        }


        // BORDERS
        private void GenerateBorderWalls()
        {
            stage.SetBorder(new Tiles.TileTypes.WallTypes.TrueWall());
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
                    DungeonRooms.Add(new Tuple<int, int, HexGrid>(TryPlaceX, TryPlaceY, room));
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
			stage.SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.FloorTypes.StoneFloor());
			unfinishedNodes.Push(new Tuple<int, int>(x, y));

            // log location as path
            DungeonPaths.Add(unfinishedNodes.Peek());

            // while stack is not empty
            while (unfinishedNodes.Count > 0)
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
					stage.SetStep(unfinishedNodes.Peek(), stage.GetStepDirection(unfinishedNodes.Peek(), nextLocation), new Tiles.TileTypes.FloorTypes.StoneFloor());

                    // log locations
                    DungeonPaths.Add(nextLocation);
                    DungeonPaths.Add(stage.GetNextValidCoordinate(unfinishedNodes.Peek(), stage.GetStepDirection(unfinishedNodes.Peek(), nextLocation)));

                    // push that location on stack
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
                        stage.SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.WallTypes.StoneWall());
                }
            }
        }


        // ENTRIES
        private void GenerateEntries()
        {
            foreach(Tuple<int, int, HexGrid> roomData in DungeonRooms)
            {
                GenerateEntry(roomData);
            }
        }

        private void GenerateEntry(Tuple<int, int, HexGrid> _roomData)
        {
            // get list of possible doors
            List<Tuple<int, int>> PossibleEntries = GetPossibleEntries(_roomData);

            if(PossibleEntries.Count > 0)
            {
                Tuple<int, int> Entry = PossibleEntries[Rand.GetInstance().Next(0, PossibleEntries.Count)];

                Entry = new Tuple<int, int>(Entry.Item1 + _roomData.Item1, Entry.Item2);
                Entry = new Tuple<int, int>(Entry.Item1, Entry.Item2 + _roomData.Item2);
                // place door
                if (stage.SetTile(Entry, new Tiles.TileTypes.FloorTypes.StoneFloor()))
                    // 50% chance for N+1 doors
                    if (Rand.GetInstance().Next(0, 100) <= 50)
                        GenerateEntry(_roomData);
            }
        }

        private List<Tuple<int, int>> GetPossibleEntries(Tuple<int, int, HexGrid> _roomData)
        {
            List<Tuple<int, int>> ReturnList = new List<Tuple<int, int>>();

            for(int x = 0; x < _roomData.Item3.Width; x++)
            {
                for (int y = 0; y < _roomData.Item3.Height; y++)
                {
                    if (_roomData.Item3.GetTile(new Tuple<int, int>(x, y)) is Tiles.TileTypes.Wall)
                        if(IsPossibleEntry(new Tuple<int, int>(x, y), _roomData))
                            ReturnList.Add(new Tuple<int, int>(x, y));
                }
            }

            return ReturnList;
        }

        private bool IsPossibleEntry(Tuple<int, int> _Location, Tuple<int, int, HexGrid> _roomData)
        {
            _Location = new Tuple<int, int>(_Location.Item1 + _roomData.Item1, _Location.Item2);
            _Location = new Tuple<int, int>(_Location.Item1, _Location.Item2 + _roomData.Item2);

            // UP && DOWN
            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Up)) is Tiles.TileTypes.Wall)
                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Down)) is Tiles.TileTypes.Wall)
                    if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftDown)) is Tiles.TileTypes.Floor)
                        if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftUp)) is Tiles.TileTypes.Floor)
                            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightDown)) is Tiles.TileTypes.Floor)
                                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightUp)) is Tiles.TileTypes.Floor)
                                    return true;
            // LEFT UP && RIGHT DOWN
            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Up)) is Tiles.TileTypes.Floor)
                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Down)) is Tiles.TileTypes.Floor)
                    if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftDown)) is Tiles.TileTypes.Floor)
                        if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftUp)) is Tiles.TileTypes.Wall)
                            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightDown)) is Tiles.TileTypes.Wall)
                                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightUp)) is Tiles.TileTypes.Floor)
                                    return true;
            // LEFT DOWN && RIGHT UP
            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Up)) is Tiles.TileTypes.Floor)
                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Down)) is Tiles.TileTypes.Floor)
                    if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftDown)) is Tiles.TileTypes.Wall)
                        if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftUp)) is Tiles.TileTypes.Floor)
                            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightDown)) is Tiles.TileTypes.Floor)
                                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightUp)) is Tiles.TileTypes.Wall)
                                    return true;
            // Otherwise:
            return false;
        }


        // DEAD ENDS
        public bool HasSingleNeighbor(int x, int y)
		{
			if (stage.GetTile(new Tuple<int, int>(x, y)) is Tiles.TileTypes.Floor)
			{

				int count = 0;

				if (stage.GetTile(stage.GetNextValidCoordinate(new Tuple<int, int>(x, y), HexGrid.Direction.Up)) is Tiles.TileTypes.Wall)
					count++;
				if (stage.GetTile(stage.GetNextValidCoordinate(new Tuple<int, int>(x, y), HexGrid.Direction.Down)) is Tiles.TileTypes.Wall)
					count++;
				if (stage.GetTile(stage.GetNextValidCoordinate(new Tuple<int, int>(x, y), HexGrid.Direction.LeftDown)) is Tiles.TileTypes.Wall)
					count++;
				if (stage.GetTile(stage.GetNextValidCoordinate(new Tuple<int, int>(x, y), HexGrid.Direction.RightUp)) is Tiles.TileTypes.Wall)
					count++;
				if (stage.GetTile(stage.GetNextValidCoordinate(new Tuple<int, int>(x, y), HexGrid.Direction.LeftUp)) is Tiles.TileTypes.Wall)
					count++;
				if (stage.GetTile(stage.GetNextValidCoordinate(new Tuple<int, int>(x, y), HexGrid.Direction.RightDown)) is Tiles.TileTypes.Wall)
					count++;

				if (count >= 5)
					return true;
				else
					return false;
			}

			return false;
		}

		public bool FindDeadEnds()
		{
			bool flag = false;
			for (int x = 0; x < stage.Width; x++)
			{
				for (int y = 0; y < stage.Height; y++)
				{
					if (HasSingleNeighbor(x, y))
					{
						// we've found a dead end
						RemoveDeadEnds(x, y);
						flag = true;
					}
				}
			}
			return flag;
		}

		public void RemoveDeadEnds(int x, int y)
		{
			if (!HasSingleNeighbor(x, y))
				return;

			stage.SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.WallTypes.StoneWall());
			int nextX = 0;
			int nextY = 0;

			if (stage.GetTile(new Tuple<int, int>(x + 1, y)) is Tiles.TileTypes.Floor) { nextX = x + 1; nextY = y; }
			if (stage.GetTile(new Tuple<int, int>(x + 1, y + 1)) is Tiles.TileTypes.Floor) { nextX = x + 1; nextY = y + 1; }
			if (stage.GetTile(new Tuple<int, int>(x, y + 1)) is Tiles.TileTypes.Floor) { nextX = x; nextY = y + 1; }
			if (stage.GetTile(new Tuple<int, int>(x - 1, y)) is Tiles.TileTypes.Floor) { nextX = x - 1; nextY = y; }
			if (stage.GetTile(new Tuple<int, int>(x - 1, y - 1)) is Tiles.TileTypes.Floor) { nextX = x - 1; nextY = y - 1; }
			if (stage.GetTile(new Tuple<int, int>(x, y - 1)) is Tiles.TileTypes.Floor) { nextX = x; nextY = y - 1; }


			RemoveDeadEnds(nextX, nextY);
		}


    }
}

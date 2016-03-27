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
		private int RoomPlaceTries;

		// Dungeon Objects - DungeonRooms<x, y, room>
		HashSet<Tuple<int, int, HexGrid>> DungeonRooms = new HashSet<Tuple<int, int, HexGrid>>();
        HashSet<Tuple<int, int>> DungeonPaths = new HashSet<Tuple<int, int>>();
        HashSet<Tuple<int, int>> DungeonEntries = new HashSet<Tuple<int, int>>();
		// Dead ends with 3 locations (not caught by dead end backfill code)
        HashSet<HashSet<Tuple<int, int>>> TriDeadends = new HashSet<HashSet<Tuple<int, int>>>();


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
			RoomPlaceTries = stage.Width * Stage.Height;

			GenerateBorderWalls();

            GenerateRooms();

            GeneratePaths();

			ConvertNullToWall();

            GenerateEntries();

			GenerateLadders();

			while (FillAllDeadEnds());

            //GenerateDFSTokens();

         //   DetectTriSets();

		}


        // BORDERS
        private void GenerateBorderWalls()
        {
            stage.SetBorder(new Tiles.TileTypes.WallTypes.TrueWall());
        }


		#region Rooms

		private void GenerateRooms()
        {         
            for (int i = 0; i < RoomPlaceTries; i++)
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
                if (PlaceRoom(TryPlaceX, TryPlaceY, room))
                    DungeonRooms.Add(new Tuple<int, int, HexGrid>(TryPlaceX, TryPlaceY, room));
            }
        }

        private bool IsValidRoomPlacement(int _x, int _y, int _width, int _height)
        {
			// for every cell in room
            for(int x = _x; x < _width + _x; x ++)
            {
                for(int y = _y; y < _height + _y; y++)
                {
					// if that cell would only overwrite a null location, and that location is in the grid
                    if (!stage.IsValidCoordinate(new Tuple<int, int>(x, y)))
                        return false;
                    if (stage.GetTile(new Tuple<int, int>(x, y)) != null)
                        return false;
                }
            }

            return true;
        }

        private bool PlaceRoom(int _x, int _y, HexGrid _room)
        {
			// copy each cell of the room into the stage
            for(int x = 0; x < _room.Width; x++)
            {
                for (int y = 0; y < _room.Height; y++)
                {
                    stage.SetTile(new Tuple<int, int>(x + _x, y + _y), _room.GetTile(new Tuple<int, int>(x, y)));
                }
            }
            
            return true;
        }

		#endregion

		#region Maze Paths

		private void GeneratePaths()
        {
			// find the first null cell in the stage,
			for(int x = 0; x < stage.Width; x++)
			{
				for (int y = 0; y < stage.Height; y++)
				{
					if (stage.GetTile(new Tuple<int, int>(x, y)) == null)
					{
						// start generating the maze here
						GenerateMaze(x, y);
						return;
					}
				}
			}
        }

		private void GenerateMaze(int x, int y)
		{
			// These are cells that the maze can still branch off of
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

			// for each cell around the input cell
            foreach (HexGrid.Direction value in Enum.GetValues(typeof(HexGrid.Direction)))
            {
                Tuple<int, int> temp = stage.GetNextValidStep(new Tuple<int, int>(x, y), value);

				// if that cell is null and valid, we can path here
                if (stage.GetTile(temp) == null && stage.IsValidCoordinate(temp))
                    pathableList.Add(temp);
            }

            return pathableList;
		}

        private void ConvertNullToWall()
        {
			// for every null cell remaining in stage, convert to wall
            for (int x = 0; x < stage.Width; x++)
            {
                for (int y = 0; y < stage.Height; y++)
                {
                    if (stage.GetTile(new Tuple<int, int>(x, y)) == null)
                        stage.SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.WallTypes.StoneWall());
                }
            }
        }

		#endregion

		#region Entries

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
				// get a random entry
                Tuple<int, int> Entry = PossibleEntries[Rand.GetInstance().Next(0, PossibleEntries.Count)];

				// adjuct from room to stage coordinate
                Entry = new Tuple<int, int>(Entry.Item1 + _roomData.Item1, Entry.Item2);
                Entry = new Tuple<int, int>(Entry.Item1, Entry.Item2 + _roomData.Item2);
				
				// place entry
				if (stage.SetTile(Entry, new Tiles.TileTypes.FloorTypes.StoneFloor()))
				{
					DungeonEntries.Add(Entry);

					// 50% chance for N+1 doors
					if (Rand.GetInstance().Next(0, 100) <= 50)
						GenerateEntry(_roomData);
				}

            }
        }

        private List<Tuple<int, int>> GetPossibleEntries(Tuple<int, int, HexGrid> _roomData)
        {
            List<Tuple<int, int>> ReturnList = new List<Tuple<int, int>>();
			
			// for every cell in the room
            for(int x = 0; x < _roomData.Item3.Width; x++)
            {
                for (int y = 0; y < _roomData.Item3.Height; y++)
                {
					// if that cell is a wall
                    if (_roomData.Item3.GetTile(new Tuple<int, int>(x, y)) is Tiles.TileTypes.Wall)
						// and if that cell is a possible entry
						if (IsPossibleEntry(new Tuple<int, int>(x, y), _roomData))
                            ReturnList.Add(new Tuple<int, int>(x, y));
                }
            }

            return ReturnList;
        }

        private bool IsPossibleEntry(Tuple<int, int> _Location, Tuple<int, int, HexGrid> _roomData)
        {
			// given location, identify 3 walls with 2 floors on either side
            _Location = new Tuple<int, int>(_Location.Item1 + _roomData.Item1, _Location.Item2);
            _Location = new Tuple<int, int>(_Location.Item1, _Location.Item2 + _roomData.Item2);

            // UP && DOWN are walls, all else are floor
            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Up)) is Tiles.TileTypes.Wall)
                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Down)) is Tiles.TileTypes.Wall)
                    if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftDown)) is Tiles.TileTypes.Floor)
                        if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftUp)) is Tiles.TileTypes.Floor)
                            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightDown)) is Tiles.TileTypes.Floor)
                                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightUp)) is Tiles.TileTypes.Floor)
                                    return true;
            // LEFT UP && RIGHT DOWN are walls, all else are floor
            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Up)) is Tiles.TileTypes.Floor)
                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.Down)) is Tiles.TileTypes.Floor)
                    if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftDown)) is Tiles.TileTypes.Floor)
                        if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.LeftUp)) is Tiles.TileTypes.Wall)
                            if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightDown)) is Tiles.TileTypes.Wall)
                                if (stage.GetTile(stage.GetNextValidCoordinate(_Location, HexGrid.Direction.RightUp)) is Tiles.TileTypes.Floor)
                                    return true;
            // LEFT DOWN && RIGHT UP are walls, all else are floor
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

		#endregion

		#region Dead Ends

		public bool HasSingleNeighbor(int x, int y)
		{
			if (stage.GetTile(new Tuple<int, int>(x, y)) is Tiles.TileTypes.Floor)
			{
				int count = 0;

				// optimize this with foreach on direction. check each direction, count neighbors
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

				// == 5 is a single neighbor
				if (count >= 5)
					return true;
				else
					return false;
			}

			return false;
		}

		public bool FillAllDeadEnds()
		{
			// return if any changes are made
			bool flag = false;

			// for every cell in stage
			for (int x = 0; x < stage.Width; x++)
			{
				for (int y = 0; y < stage.Height; y++)
				{
					if (HasSingleNeighbor(x, y))
					{
						// we've found a dead end
						RemoveDeadEnd(x, y);
						flag = true;
					}
				}
			}

			return flag;
		}

		private void RemoveDeadEnd(int x, int y)
		{
			// if this cell is a dead end
			if (!HasSingleNeighbor(x, y))
				return;

			stage.SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.WallTypes.StoneWall());
			int nextX = 0;
			int nextY = 0;

			// foreach on direction?
			if (stage.GetTile(new Tuple<int, int>(x + 1, y)) is Tiles.TileTypes.Floor) { nextX = x + 1; nextY = y; }
			if (stage.GetTile(new Tuple<int, int>(x + 1, y + 1)) is Tiles.TileTypes.Floor) { nextX = x + 1; nextY = y + 1; }
			if (stage.GetTile(new Tuple<int, int>(x, y + 1)) is Tiles.TileTypes.Floor) { nextX = x; nextY = y + 1; }
			if (stage.GetTile(new Tuple<int, int>(x - 1, y)) is Tiles.TileTypes.Floor) { nextX = x - 1; nextY = y; }
			if (stage.GetTile(new Tuple<int, int>(x - 1, y - 1)) is Tiles.TileTypes.Floor) { nextX = x - 1; nextY = y - 1; }
			if (stage.GetTile(new Tuple<int, int>(x, y - 1)) is Tiles.TileTypes.Floor) { nextX = x; nextY = y - 1; }

			// recursively keep filling the dead end
			RemoveDeadEnd(nextX, nextY);
		}

		#endregion


		// LADDERS
		private void GenerateLadders()
		{
			// get list of possible ladders
			List<Tuple<int, int>> possibleLadders = new List<Tuple<int, int>>();

			// Change this later, for now just detect dead ends as possible locations
			for (int x = 0; x < stage.Width; x++)
			{
				for(int y = 0; y < stage.Height; y++)
				{
					if (HasSingleNeighbor(x, y))
					{
						possibleLadders.Add(new Tuple<int, int>(x, y));
					}
				}
			}

			// minimum 2 ladder positions, 1 up and 1 down. About an even number of each
			for (int i = 0; i < Math.Max(2, 0.2 * possibleLadders.Count); i++)
			{
				if(i % 2 == 0)
					stage.SetTile(possibleLadders[Rand.GetInstance().Next(0, possibleLadders.Count)], new Tiles.TileTypes.PortalTypes.UpLadder());
				else
					stage.SetTile(possibleLadders[Rand.GetInstance().Next(0, possibleLadders.Count)], new Tiles.TileTypes.PortalTypes.DownLadder());
			}
		}


        // TODO:	TRI DEAD ENDS - dead ends with 3 tiles adjacent, not just 1.
        private void DetectTriSets()
        {
            for(int x = 0; x < stage.Width; x++)
            {
                for (int y = 0; y < stage.Height; y++)
                {
                    HashSet<Tuple<int, int>> PossibleTriSet = IsTriSet(new Tuple<int, int>(x, y));
                    if (PossibleTriSet != null)
                    {
                        // tri set detected
                        TriDeadends.Add(PossibleTriSet);
                    }
                        
                }
            }

            // mark dead ends
            foreach(var set in TriDeadends)
            {
                foreach (Tuple<int, int> cell in set)
                {
                    stage.SetTile(cell, new Tiles.TileTypes.LiquidTypes.Blood());
                }
            }

//            MarkPathSideSteps();

        }

        private HashSet<Tuple<int, int>> IsTriSet(Tuple<int,int> _Location)
        {
            Tuple<int, int> AdjacentFloor_1 = null;
            Tuple<int, int> AdjacentFloor_2 = null;

            if (DungeonPaths.Contains(_Location))
            {
                foreach (HexGrid.Direction value in Enum.GetValues(typeof(HexGrid.Direction)))
                {
                    Tuple<int, int> temp = stage.GetNextValidCoordinate(new Tuple<int, int>(_Location.Item1, _Location.Item2), value);

                    if (DungeonPaths.Contains(temp))
                    {
                        if (stage.GetTile(temp) is Tiles.TileTypes.Floor)
                        {

                            if (AdjacentFloor_1 == null)
                                AdjacentFloor_1 = temp;
                            else if (AdjacentFloor_2 == null)
                            {
                                foreach (HexGrid.Direction value2 in Enum.GetValues(typeof(HexGrid.Direction)))
                                {
                                    int checkX = stage.GetNextValidCoordinate(temp, value2).Item1;
                                    int checkY = stage.GetNextValidCoordinate(temp, value2).Item2;

                                    if (checkX == AdjacentFloor_1.Item1 && checkY == AdjacentFloor_1.Item2)
                                        AdjacentFloor_2 = temp;
                                }
                            }
                            else
                                return null;
                        }
                    }
                }
            }
            if (AdjacentFloor_1 != null && AdjacentFloor_2 != null)
            {
                HashSet<Tuple<int, int>> ReturnSet = new HashSet<Tuple<int, int>>();

                ReturnSet.Add(_Location);
                ReturnSet.Add(AdjacentFloor_1);
                ReturnSet.Add(AdjacentFloor_2);
                return ReturnSet;
            }

            return null;
        }

        // mark locations from triset that are a sidestep from a path
        //private void MarkPathSideSteps()
        //{
        //    // for each of 3 locations, find the one that does not touch a floor
        //    // mark that location

        //    foreach (var set in TriDeadends)
        //    {
        //        foreach (Tuple<int, int> location in set)
        //        {
        //            if(stage.GetTile(location) is Tiles.TileTypes.LiquidTypes.Blood)
        //                if()
        //                foreach (HexGrid.Direction value in Enum.GetValues(typeof(HexGrid.Direction)))
        //                {
        //                    Tuple<int, int> temp = stage.GetNextValidCoordinate(new Tuple<int, int>(location.Item1, location.Item2), value);

                        
        //                    if (stage.GetTile(temp) is Tiles.TileTypes.Floor)
        //                        continue;
        //                    else
        //                        stage.SetTile(temp, new Tiles.TileTypes.FloorTypes.StoneFloor());
        //                }
        //        }
        //    }

        //}
    }
}

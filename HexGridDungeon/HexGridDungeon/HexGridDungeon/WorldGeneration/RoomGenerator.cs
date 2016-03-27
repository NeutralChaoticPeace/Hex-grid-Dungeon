using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexGridDungeon.WorldGeneration
{
    public class RoomGenerator
    {
        public RoomGenerator()
        {

        }


        public HexGrid GenerateNewRoom(int width, int height)
        {
            int rand = Rand.GetInstance().Next(0, 100);


            if (rand <= 80)
				return BuildSimpleRoom(width, height);
			else if (80 < rand && rand <= 100)
				return BuildLiquidRoom(width, height);
			else
                return BuildSimpleRoom(width, height);
        } 


        public HexGrid BuildSimpleRoom(int width, int height)
        {
			HexGrid room = new HexGrid(width, height);			

            BuildRoomBorder(room);
			FillRoom(new Tiles.TileTypes.Floor(), room);

            return room;
        }
		
        public HexGrid BuildLiquidRoom(int width, int height)
        {
			HexGrid room = new HexGrid(width, height);
			BuildRoomBorder(room);
			FillRoom(new Tiles.TileTypes.Floor(), room);

			List<Tuple<int, int>> liquidLocations = new List<Tuple<int, int>>();

			// start the pool in the middle of the room
			int poolX = room.Width / 2;
			int poolY = room.Height / 2;
			int liquidCount = Rand.GetInstance().Next(Math.Min(room.Width, room.Height), Math.Max(room.Width, room.Height));

			int tileRand = Rand.GetInstance().Next(0, 100);
			Tiles.Tile _tile = null;

			if (tileRand <= 75)
			{
				_tile = new Tiles.TileTypes.LiquidTypes.Water();
			}
			else if (tileRand > 75 && tileRand < 90)
			{
				_tile = new Tiles.TileTypes.LiquidTypes.Lava();
			}
			else if (tileRand >= 90)
			{
				_tile = new Tiles.TileTypes.LiquidTypes.Blood();
			}

			room.SetTile(new Tuple<int, int>(poolX, poolY), _tile);
			liquidLocations.Add(new Tuple<int, int>(poolX, poolY));

			for (int i = 1; i < liquidCount; i++)
			{
				Tuple<int, int> nextAdjLiquidLocation = liquidLocations[Rand.GetInstance().Next(0, liquidLocations.Count - 1)];
				Tuple<int, int> nextLiquidLocation = null;
				Tiles.Tile nextLocationTile = null;

				while (nextLiquidLocation == null && (nextLocationTile == null || nextLocationTile is Tiles.TileTypes.Floor))
				{
					nextLiquidLocation = room.GetNextValidCoordinate(nextAdjLiquidLocation, (HexGrid.Direction)Rand.GetInstance().Next(0, 5));
					nextLocationTile = room.GetTile(nextLiquidLocation);
				}

				room.SetTile(nextLiquidLocation, _tile);
				liquidLocations.Add(nextLiquidLocation);
			}

			BuildRoomBorder(room);

			return room;
        }


        // Helper functions
        private void BuildRoomBorder(HexGrid _room)
        {
            // initial direction priority
            Dictionary<int, HexGrid.Direction> DirectionPriority = new Dictionary<int, HexGrid.Direction>();
            HashSet<Tuple<int, int>> VisitedLocations = new HashSet<Tuple<int, int>>();

            Tuple<int, int> StartLocation = new Tuple<int, int>(0, 0);
            Tuple<int, int> CurrentLocation = StartLocation;
            Tuple<int, int> NextLocation;

            SetPrioityUpClockwise(DirectionPriority);

            _room.SetTile(StartLocation, new Tiles.TileTypes.Wall());
            VisitedLocations.Add(StartLocation);

            while(true)
            {
                for(int i = 1; i <= 6; i++)
                {
                    NextLocation = _room.GetNextValidStep(CurrentLocation, DirectionPriority[i]);
                    // if next location is a new location that is valid...
                    // base case:
                    if(NextLocation != null && NextLocation.Item1 == StartLocation.Item1 && NextLocation.Item2 == StartLocation.Item2)
                    {
                        _room.SetStep(CurrentLocation, DirectionPriority[i], new Tiles.TileTypes.Wall());
                        return;
                    }
                    if (NextLocation != null && !VisitedLocations.Contains(NextLocation))
                    {

                        // otherwise:
                        _room.SetStep(CurrentLocation, DirectionPriority[i], new Tiles.TileTypes.Wall());
                        VisitedLocations.Add(CurrentLocation);
                        CurrentLocation = NextLocation;
                        

                        // when to switch direction priority
                        if (i >= 5)
                            SetPriorityDownClockwise(DirectionPriority);

                        break;
                    }
                        
                }
            }
        }

		public void FillRoom(Tiles.Tile _tile, HexGrid _room)
		{
			Tuple<int, int> StartLocation = new Tuple<int, int>(0, 0);
			bool superBreak = false;

			for (int x = 0; x < _room.Width && !superBreak; x++)
			{
				for (int y = 0; y < _room.Height && !superBreak; y++)
				{
					if (_room.GetTile(new Tuple<int, int>(x, y)) == null)
					{
						StartLocation = new Tuple<int, int>(x, y);
						superBreak = true;
					}
				}
			}

			FillRoomHelper(StartLocation.Item1, StartLocation.Item2, _tile, _room);
		}

		public void FillRoomHelper(int x, int y, Tiles.Tile _tile, HexGrid _room)
		{
			foreach(HexGrid.Direction value in Enum.GetValues(typeof(HexGrid.Direction)))
			{
				if (_room.GetTile(_room.GetNextValidCoordinate(new Tuple<int, int>(x, y), value)) == null)
				{
					_room.SetTile(_room.GetNextValidCoordinate(new Tuple<int, int>(x, y), value), _tile);
					FillRoomHelper(_room.GetNextValidCoordinate(new Tuple<int, int>(x, y), value).Item1, _room.GetNextValidCoordinate(new Tuple<int, int>(x, y), value).Item2, _tile, _room);
				}
            }
		}

        private void SetPrioityUpClockwise(Dictionary<int, HexGrid.Direction> _Dictionary)
        {
            _Dictionary.Clear();
            _Dictionary.Add(1, HexGrid.Direction.Up);
            _Dictionary.Add(2, HexGrid.Direction.RightUp);
            _Dictionary.Add(3, HexGrid.Direction.RightDown);
            _Dictionary.Add(4, HexGrid.Direction.Down);
            _Dictionary.Add(5, HexGrid.Direction.LeftDown);
            _Dictionary.Add(6, HexGrid.Direction.LeftUp);
        }

        private void SetPriorityDownClockwise(Dictionary<int, HexGrid.Direction> _Dictionary)
        {
            _Dictionary.Clear();

            _Dictionary.Add(1, HexGrid.Direction.Down);
            _Dictionary.Add(2, HexGrid.Direction.LeftDown);
            _Dictionary.Add(3, HexGrid.Direction.LeftUp);
            _Dictionary.Add(4, HexGrid.Direction.Up);
            _Dictionary.Add(5, HexGrid.Direction.RightUp);
            _Dictionary.Add(6, HexGrid.Direction.RightDown);
        }
    }
}

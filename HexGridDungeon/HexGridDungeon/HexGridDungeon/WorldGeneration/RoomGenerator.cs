using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexGridDungeon.WorldGeneration
{
    public class RoomGenerator
    {
        private int MinRoomSize;
        private int MaxroomSize;

        public RoomGenerator(int _MinRoomSize, int _MaxRoomSize)
        {
            MinRoomSize = _MinRoomSize;
            MaxroomSize = _MaxRoomSize;
        }


        public HexGrid GenerateNewRoom(int width, int height)
        {
            int rand = Rand.GetInstance().Next(0, 100);


            if (rand <= 50)
                return BuildSimpleRoom(width, height);
            //else if (50 < rand && rand <= 100)
            //    return BuildWaterRoom(width, height);
            else
                return BuildSimpleRoom(width, height);
        } 


        public HexGrid BuildSimpleRoom(int width, int height)
        {
			HexGrid room = new HexGrid(width, height);
            //room.SetBorder(new Tiles.TileTypes.Wall());
            //room.SetArea("floor", width - 2, height - 2, 1, 1);			

            BuildRoomBorder(room);

            return room;
        }


        public HexGrid BuildWaterRoom(int width, int height)
        {
			HexGrid room = new HexGrid(width, height);
			room.SetBorder(new Tiles.TileTypes.Wall());

			int poolWidth = Rand.GetInstance().Next(1, width - 4);
			int poolHeight = Rand.GetInstance().Next(1, height - 4);

			room.SetArea("floor", width - 2, height - 2, 1, 1);

			int poolX = Rand.GetInstance().Next(2, width - poolWidth - 2);
			int poolY = Rand.GetInstance().Next(2, height - poolHeight - 2);

			room.SetArea("liquid", poolWidth, poolHeight, poolX, poolY);

            return room;
        }


        // Helper functions
        private void BuildRoomBorder(HexGrid _room)
        {
            // initial direction priority
            Dictionary<int, HexGrid.Direction> DirectionPriority = new Dictionary<int, HexGrid.Direction>();
            Tuple<int, int> StartLocation = new Tuple<int, int>(0, 0);
            Tuple<int, int> CurrentLocation = StartLocation;
            SetPrioityUpClockwise(DirectionPriority);

            _room.SetTile(StartLocation, new Tiles.TileTypes.Wall());

            for(int j = 0; j < 100; j++)
            {
                for(int i = 1; i <= 6; i++)
                {
                    if (_room.GetNextValidStep(CurrentLocation, DirectionPriority[i]) != null)
                    {
                        // base case:
                        if(_room.GetNextValidStep(CurrentLocation, DirectionPriority[i]) == StartLocation)
                        {
                            _room.SetStep(CurrentLocation, DirectionPriority[i], new Tiles.TileTypes.Wall());
                            return;
                        }

                        // otherwise:
                        _room.SetStep(CurrentLocation, DirectionPriority[i], new Tiles.TileTypes.Wall());
                        CurrentLocation = _room.GetNextValidStep(CurrentLocation, DirectionPriority[i]);

                        // when to switch direction priority
                        if (i >= 5)
                            SetDownClockwisePriority(DirectionPriority);

                        break;
                    }
                        
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

        private void SetDownClockwisePriority(Dictionary<int, HexGrid.Direction> _Dictionary)
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

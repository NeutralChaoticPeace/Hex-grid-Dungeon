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
            else if (50 < rand && rand <= 100)
                return BuildWaterRoom(width, height);
            else
                return BuildSimpleRoom(width, height);
        } 


        private HexGrid BuildSimpleRoom(int width, int height)
        {
			HexGrid room = new HexGrid(width, height);
			room.GenerateBorderWalls();

            return room;
        }


        private HexGrid BuildWaterRoom(int width, int height)
        {
			HexGrid room = new HexGrid(width, height);
			room.GenerateBorderWalls();

			int poolWidth = Rand.GetInstance().Next(width - 2, height - 2);
			int poolHeight = Rand.GetInstance().Next(width - 2, height - 2);

			for (int i = 2; i < poolWidth; i++)
			{
				for (int j = 2; j < poolHeight; j++)
				{
					if (room.IsValidCoordinate(new Tuple<int, int>(i, j)))
                        room.SetTile(new Tuple<int, int>(i, j), new Tiles.TileTypes.Liquid());
				}
			}

            return room;
        }

    }
}

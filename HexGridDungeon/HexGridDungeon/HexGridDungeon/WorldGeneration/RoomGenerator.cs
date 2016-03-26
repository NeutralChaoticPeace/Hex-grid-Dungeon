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


        public HexGrid BuildSimpleRoom(int width, int height)
        {
			HexGrid room = new HexGrid(width, height);
			room.SetBorder(new Tiles.TileTypes.Wall());
			room.SetArea("floor", width - 2, height - 2, 1, 1);			

            return room;
        }


        public HexGrid BuildWaterRoom(int width, int height)
        {
			HexGrid room = new HexGrid(width, height);
			room.SetBorder(new Tiles.TileTypes.Wall());

			int poolWidth = Rand.GetInstance().Next(1, height - 4);
			int poolHeight = Rand.GetInstance().Next(1, height - 4);

			room.SetArea("floor", width - 2, height - 2, 1, 1);

			int poolX = Rand.GetInstance().Next(2, width - poolWidth - 2);
			int poolY = Rand.GetInstance().Next(2, height - poolHeight - 2);

			room.SetArea("liquid", poolWidth, poolHeight, poolX, poolY);

            return room;
        }

    }
}

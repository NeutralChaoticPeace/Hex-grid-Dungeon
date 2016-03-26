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


        public HexGrid GenerateNewRoom()
        {
            int rand = Rand.GetInstance().Next(0, 100);

            if (rand <= 50)
                return BuildSimpleRoom();
            else if (50 < rand && rand <= 100)
                return BuildWaterRoom();
            else
                return BuildSimpleRoom();
        } 


        private HexGrid BuildSimpleRoom()
        {
            return null;
        }


        private HexGrid BuildWaterRoom()
        {
            return null;
        }

    }
}

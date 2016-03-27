using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexGridDungeon
{
    public class Rand
    {
		// Data
        private static Rand Instance = null;
        private Random rand;
        private int seed = -1;


        // Constructors
        private Rand()
        {
            if(0 <= seed)
                rand = new Random(seed);
            else
                rand = new Random();
        }

        public static Rand GetInstance()
        {
            if (Instance == null)
                Instance = new Rand();

            return Instance;
        }


        // Random class wrapper functions
        public int Next()
        {
            return rand.Next();
        }

        public int Next(int maxValue)
        {
            return rand.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return rand.Next(minValue, maxValue);
        }
    }
}

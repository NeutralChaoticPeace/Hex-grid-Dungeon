using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexGridDungeon.Tiles
{
    public abstract class Tile
    {
        protected string spriteID;

        public string GetSpriteID
        {
            get { return spriteID; }
        }

    }
}

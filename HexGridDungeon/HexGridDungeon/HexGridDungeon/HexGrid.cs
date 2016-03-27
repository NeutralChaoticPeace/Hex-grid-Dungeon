using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HexGridDungeon.Tiles;

namespace HexGridDungeon
{
    public class HexGrid
    {
        public enum Direction { Up, Down, LeftUp, LeftDown, RightUp, RightDown};

        // Data
        private Tile[,] Grid;


        // properties
        public int Width
        {
            get { return Grid.GetLength(0); }
        }
        public int Height
        {
            get { return Grid.GetLength(1); }
        }

        // Contructors
        public HexGrid()
        {
            Grid = new Tile[50, 50];
        }

        public HexGrid(int size)
        {
            Grid = new Tile[size, size];
        }

        public HexGrid(int width, int height)
        {
            Grid = new Tile[width, height];
        }


        // Traversal Logic
		public Tuple<int, int> GetNextValidStep(Tuple<int, int> currentCoordinate, Direction dir)
		{
			return GetNextValidCoordinate(GetNextValidCoordinate(currentCoordinate, dir), dir);
		}

        public Tuple<int, int> GetNextValidCoordinate(Tuple<int, int> currentCoordinate, Direction dir)
        {
			if (currentCoordinate == null)
				return null;

            if (!IsValidCoordinate(currentCoordinate))
                return null;

            Tuple<int, int> NextCoordinate;

            // Up
            if (dir == Direction.Up)
            {
                NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1, currentCoordinate.Item2 - 1);
                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Down
            else if (dir == Direction.Down)
            {
                NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1, currentCoordinate.Item2 + 1);
                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Left and Up
            else if (dir == Direction.LeftUp)
            {
                if(currentCoordinate.Item1 % 2 == 1)
                    NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 - 1, currentCoordinate.Item2);
                else
                    NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 - 1, currentCoordinate.Item2 - 1);

                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Left and Down
            else if (dir == Direction.LeftDown)
            {
                if (currentCoordinate.Item1 % 2 == 0)
                    NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 - 1, currentCoordinate.Item2);
                else
                    NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 - 1, currentCoordinate.Item2 + 1);

                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Right and Up
            else if (dir == Direction.RightUp)
            {
                if(currentCoordinate.Item1 % 2 == 0)
                    NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 + 1, currentCoordinate.Item2 - 1);
                else
                    NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 + 1, currentCoordinate.Item2);

                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Right and Down
            else if (dir == Direction.RightDown)
            {
                if(currentCoordinate.Item1 % 2 == 1)
                    NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 + 1, currentCoordinate.Item2 + 1);
                else
                    NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 + 1, currentCoordinate.Item2);

                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Error
            else
                return null;
        }

        public bool IsValidCoordinate(Tuple<int, int> coordinate)
        {
            if (coordinate == null)
                return false;

            // out of lower bound
            if (coordinate.Item1 < 0 || coordinate.Item2 < 0)
                return false;
            // out of upper bround
            else if (coordinate.Item1 >= Grid.GetLength(0) || coordinate.Item2 >= Grid.GetLength(1))
                return false;
            else
                return true;
        }

        public Direction GetStepDirection(Tuple<int, int> current, Tuple<int, int> next)
        {
            foreach (HexGrid.Direction value in Enum.GetValues(typeof(HexGrid.Direction)))
            {
                Tuple<int, int> temp = this.GetNextValidStep(current, value);

                if(temp != null)
                    if (temp.Item1 == next.Item1 && temp.Item2 == next.Item2)
                        return value;
            }

            // this is an ERROR
            return Direction.Down;
        }

        // Tile Get/Set Logic
        public bool SetTile(Tuple<int, int> coordinate, Tile NewTile)
        {
            if(IsValidCoordinate(coordinate))
            {
                Grid[coordinate.Item1, coordinate.Item2] = NewTile;
                return true;
            }
            else
                return false;
        }

		public void SetBorder(Tile _tile)
		{
			for (int x = 0; x < Width; x++)
			{
				SetTile(new Tuple<int, int>(x, 0), _tile);
				SetTile(new Tuple<int, int>(x, Height - 1), _tile);
			}

			for (int y = 0; y < Height; y++)
			{
				SetTile(new Tuple<int, int>(0, y), _tile);
				SetTile(new Tuple<int, int>(Width - 1, y), _tile);
			}
		}

		public void SetArea(string _tile, int _width, int _height, int _x, int _y)
		{
			for (int x = _x; x < _width + _x; x++)
			{
				for (int y = _y; y < _height + _y; y++)
				{
					switch (_tile.ToLower())
					{
						case "floor": SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.Floor());
							break;
						case "wall": SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.Wall());
							break;
						case "liquid": SetTile(new Tuple<int, int>(x, y), new Tiles.TileTypes.Liquid());
							break;
						default: break;

					}
				}
			}
		}

        public void SetStep(Tuple<int, int> Location, Direction _dir, Tile _tile)
        {
            // set first and second tiles in step to input tile
            SetTile(GetNextValidCoordinate(Location, _dir), _tile);
            SetTile(GetNextValidCoordinate(GetNextValidCoordinate(Location, _dir), _dir), _tile);
        }

		public Tile GetTile(Tuple<int, int> coordinate)
        {
            if (IsValidCoordinate(coordinate))
                return Grid[coordinate.Item1, coordinate.Item2];
            else
                return null;
		}

		private void CreateLiquid(string type, Tuple<int, int> coordinate)
		{
			if (IsValidCoordinate(coordinate))
			{
				switch (type.ToLower())
				{
					case "water": SetTile(coordinate, new Tiles.TileTypes.Liquid());
						break;
					default: break;
				}
			}
		}

		private void CreateFloor(Tuple<int, int> coordinate)
		{
			if (IsValidCoordinate(coordinate))
				SetTile(coordinate, new Tiles.TileTypes.Liquid());
		}

		private void CreateWall(Tuple<int, int> coordinate)
		{
			if (IsValidCoordinate(coordinate))
				SetTile(coordinate, new Tiles.TileTypes.Wall());
		}
	}
}

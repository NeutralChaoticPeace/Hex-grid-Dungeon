using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HexGridDungeon.Tiles;

namespace HexGridDungeon
{
    public class HexGrid
    {
		// Definitions
        public enum Direction { Up, RightUp, RightDown, Down, LeftDown, LeftUp};

        // Data - Data Structure
        private Tile[,] Grid;


        // Properties
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
            Grid = new Tile[21, 21];
        }

        public HexGrid(int size)
        {
            Grid = new Tile[size, size];
        }

        public HexGrid(int width, int height)
        {
            Grid = new Tile[width, height];
        }


        #region Traversal Logic

        // Gets the cell 2 units away in the given direction from the current cell
        public Tuple<int, int> GetNextValidStep(Tuple<int, int> currentCoordinate, Direction dir)
		{
			return GetNextValidCoordinate(GetNextValidCoordinate(currentCoordinate, dir), dir);
		}

        // Gets the cell 1 unit away in the given direction from the current cell
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

        // returns true if coordinate exists in hexgrid, false otherwise
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

        // returns the direction given two cells that are one step away from eachother
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

		#endregion

		#region Tile Accessor Logic

		// sets tile at location in hexgrid to input tile
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

		// sets the border of hexgrid to input tile
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

		// setss tiles at first and second location in a step to input tile
        public void SetStep(Tuple<int, int> Location, Direction _dir, Tile _tile)
        {
            // set first and second tiles in step to input tile
            SetTile(GetNextValidCoordinate(Location, _dir), _tile);
            SetTile(GetNextValidCoordinate(GetNextValidCoordinate(Location, _dir), _dir), _tile);
        }

		// gets the tile at a location
		public Tile GetTile(Tuple<int, int> coordinate)
        {
            if (IsValidCoordinate(coordinate))
                return Grid[coordinate.Item1, coordinate.Item2];
            else
                return null;
		}

		#endregion
	}
}

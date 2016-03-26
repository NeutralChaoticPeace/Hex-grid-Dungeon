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
        public Tuple<int, int> GetNextValidCoordinate(Tuple<int, int> currentCoordinate, Direction dir)
        {
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
                NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 - 1, currentCoordinate.Item2 - 1);
                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Left and Down
            else if (dir == Direction.LeftDown)
            {
                NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 - 1, currentCoordinate.Item2);
                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Right and Up
            else if (dir == Direction.RightUp)
            {
                NextCoordinate = new Tuple<int, int>(currentCoordinate.Item1 + 1, currentCoordinate.Item2 - 1);
                if (IsValidCoordinate(NextCoordinate))
                    return NextCoordinate;
                else
                    return null;
            }

            // Right and Down
            else if (dir == Direction.RightDown)
            {
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
            // out of lower bound
            if (coordinate.Item1 < 0 || coordinate.Item2 < 0)
                return false;
            // out of upper bround
            else if (coordinate.Item1 > Grid.GetLength(0) || coordinate.Item2 > Grid.GetLength(1))
                return false;
            else
                return true;
        }


        // Tile Get/Set
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

        public Tile GetTile(Tuple<int, int> coordinate)
        {
            if (IsValidCoordinate(coordinate))
                return Grid[coordinate.Item1, coordinate.Item2];
            else
                return null;
        }
    }
}

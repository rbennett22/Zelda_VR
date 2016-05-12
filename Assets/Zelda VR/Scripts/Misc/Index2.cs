using UnityEngine;

namespace Immersio.Utility
{
    public class Index2
    {
        public enum Direction
        {
            Up, Down, Right, Left
        }
        public static Direction GetDirectionForVector2(Vector2 vec)
        {
            if (vec.x < 0) { return Direction.Left; }
            if (vec.x > 0) { return Direction.Right; }
            if (vec.y > 0) { return Direction.Down; }
            if (vec.y < 0) { return Direction.Up; }

            return Direction.Up;
        }


        public int x, y;


        public Index2()
        {
            x = 0;
            y = 0;
        }
        public Index2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Index2(Vector2 setIndex)
        {
            x = (int)setIndex.x;
            y = (int)setIndex.y;
        }


        public Index2 GetAdjacentIndex(Direction direction)
        {
            if (direction == Direction.Down) return new Index2(x, y - 1);
            else if (direction == Direction.Up) return new Index2(x, y + 1);
            else if (direction == Direction.Left) return new Index2(x - 1, y);
            else if (direction == Direction.Right) return new Index2(x + 1, y);
            else return null;
        }


        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }


        public override string ToString()
        {
            return ("[" + x.ToString() + "," + y.ToString() + "]");

        }

        public bool IsEqual(Index2 n)
        {
            if (n == null)
            {
                return false;
            }

            return (
                x == n.x &&
                y == n.y);
        }


        public static bool Compare(Index2 a, Index2 b)
        {
            if (b == null)
            {
                return false;
            }

            return (
                a.x == b.x &&
                a.y == b.y);
        }

        public static Index2 FromString(string indexString)
        {
            string[] splitString = indexString.Split(',');

            try
            {
                return new Index2(int.Parse(splitString[0]), int.Parse(splitString[1]));
            }
            catch (System.Exception)
            {
                Debug.LogError("Index2.FromString: Invalid format. String must be in \"x,y\" format.");
                return null;
            }
        }
    }
}
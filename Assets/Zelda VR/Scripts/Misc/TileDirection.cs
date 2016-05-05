using UnityEngine;

namespace Immersio.Utility
{
    public class TileDirection
    {
        /*public enum Direction
        {
            Up, Down, Right, Left
        }
        public Direction direction;*/

        int _x, _y;
        public int X { get { return _x; } set { _x = Mathf.Clamp(value, -1, 1); } }
        public int Y { get { return _y; } set { _y = Mathf.Clamp(value, -1, 1); } }


        public TileDirection()
        {
            X = 0;
            Y = 0;
        }
        public TileDirection(int x, int y)
        {
            X = x;
            Y = y;
        }
        public TileDirection(float x, float y)
        {
            X = (int)x;
            Y = (int)y;
        }
        public TileDirection(Vector2 v)
        {
            X = (int)v.x;
            Y = (int)v.y;
        }
        public TileDirection(TileDirection n)
        {
            X = n.X;
            Y = n.Y;
        }


        public Vector2 ToVector2()
        {
            return new Vector2(_x, _y);
        }

        public override string ToString()
        {
            return ("[" + _x.ToString() + "," + _y.ToString() + "]");

        }

        public bool IsEqual(TileDirection n)
        {
            if (n == null)
            {
                return false;
            }

            return (
                _x == n._x &&
                _y == n._y);
        }

        public bool IsZero() { return this == Zero; }
        public bool IsUp() { return this == Up; }
        public bool IsDown() { return this == Down; }
        public bool IsRight() { return this == Right; }
        public bool IsLeft() { return this == Left; }

        public TileDirection Reversed { get { return new TileDirection(-_x, -_y); } }
        public void Reverse()
        {
            _x *= -1;
            _y *= -1;
        }


        public static bool Compare(TileDirection a, TileDirection b)
        {
            if (b == null)
            {
                return false;
            }

            return (
                a._x == b._x &&
                a._y == b._y);
        }

        public static TileDirection FromString(string indexString)
        {
            string[] splitString = indexString.Split(',');

            try
            {
                return new TileDirection(int.Parse(splitString[0]), int.Parse(splitString[1]));
            }
            catch (System.Exception)
            {
                Debug.LogError("TileDirection.FromString: Invalid format. String must be in \"x,y\" format.");
                return null;
            }
        }

        public static TileDirection Zero { get{ return new TileDirection(0, 0); } }
        public static TileDirection Up { get { return new TileDirection(0, 1); } }
        public static TileDirection Down { get { return new TileDirection(0, -1); } }
        public static TileDirection Right { get { return new TileDirection(1, 0); } }
        public static TileDirection Left { get { return new TileDirection(-1, 0); } }
    }
}
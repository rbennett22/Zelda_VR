using UnityEngine;

// a Vector3 using ints instead of floats, for storing indexes and stuff

namespace Immersio.Utility
{
    public class Index3
    {
        public enum Direction
        {
            Up, Down, Right, Left, Forward, Back
        }


        public int x, y, z;


        public Index3()
        {
            x = 0;
            y = 0;
            z = 0;
        }
        public Index3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Index3(Vector3 v)
        {
            x = (int)v.x;
            y = (int)v.y;
            z = (int)v.z;
        }


        public Index3 GetAdjacentIndex(Direction direction)
        {
            if (direction == Direction.Down) return new Index3(x, y - 1, z);
            else if (direction == Direction.Up) return new Index3(x, y + 1, z);
            else if (direction == Direction.Left) return new Index3(x - 1, y, z);
            else if (direction == Direction.Right) return new Index3(x + 1, y, z);
            else if (direction == Direction.Back) return new Index3(x, y, z - 1);
            else if (direction == Direction.Forward) return new Index3(x, y, z + 1);
            else return null;
        }


        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return ("[" + x.ToString() + "," + y.ToString() + "," + z.ToString() + "]");

        }

        public bool IsEqual(Index3 n)
        {
            if (n == null)
            {
                return false;
            }

            return (
                x == n.x && 
                y == n.y && 
                z == n.z);
        }


        public static bool Compare(Index3 a, Index3 b)
        {
            if (b == null)
            {
                return false;
            }

            return (
                a.x == b.x &&
                a.y == b.y &&
                a.z == b.z);
        }

        public static Index3 FromString(string indexString)
        {
            string[] splitString = indexString.Split(',');

            try
            {
                return new Index3(int.Parse(splitString[0]), int.Parse(splitString[1]), int.Parse(splitString[2]));
            }
            catch (System.Exception)
            {
                Debug.LogError("Index3.FromString: Invalid format. String must be in \"x,y,z\" format.");
                return null;
            }
        }
    }
}
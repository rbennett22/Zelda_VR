using UnityEngine;

namespace Immersio.Utility
{
    public struct Index3
    {
        public enum Index3Direction
        {
            Up, Down, Right, Left, Forward, Back
        }


        public int x, y, z;


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


        public Index3 GetAdjacentIndex(Index3Direction direction)
        {
            if (direction == Index3Direction.Down) return new Index3(x, y - 1, z);
            else if (direction == Index3Direction.Up) return new Index3(x, y + 1, z);
            else if (direction == Index3Direction.Left) return new Index3(x - 1, y, z);
            else if (direction == Index3Direction.Right) return new Index3(x + 1, y, z);
            else if (direction == Index3Direction.Back) return new Index3(x, y, z - 1);
            else if (direction == Index3Direction.Forward) return new Index3(x, y, z + 1);
            else return new Index3();
        }


        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return ("[" + x.ToString() + "," + y.ToString() + "," + z.ToString() + "]");
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
                return new Index3();
            }
        }


        #region Operator Overloads

        public override bool Equals(object ob)
        {
            if (ob is Index3)
            {
                Index3 n = (Index3)ob;
                return (x == n.x) && (y == n.y) && (z == n.z);
            }
            return false;
        }
        public bool Equals(Index3 n)
        {
            return (x == n.x) && (y == n.y) && (z == n.z);
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() * y.GetHashCode() ^ z.GetHashCode();
        }

        public static bool operator ==(Index3 a, Index3 b)
        {
            return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
        }
        public static bool operator !=(Index3 a, Index3 b)
        {
            return !(a == b);
        }

        public static Index3 operator +(Index3 a, Index3 b)
        {
            Index3 n = new Index3();
            n.x = a.x + b.x;
            n.y = a.y + b.y;
            n.z = a.z + b.z;
            return n;
        }
        public static Index3 operator -(Index3 a, Index3 b)
        {
            Index3 n = new Index3();
            n.x = a.x - b.x;
            n.y = a.y - b.y;
            n.z = a.z - b.z;
            return n;
        }

        #endregion Operator Overloads
    }
}
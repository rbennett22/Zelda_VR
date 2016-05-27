using UnityEngine;

namespace Immersio.Utility
{
    public struct Index2
    {
        public int x, y;


        public Index2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Index2(Vector2 v)
        {
            x = (int)v.x;
            y = (int)v.y;
        }
        public Index2(IndexDirection2 t)
        {
            x = t.X;
            y = t.Y;
        }


        public static implicit operator Index2(IndexDirection2 t)
        {
            return new Index2(t);  // implicit conversion
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
        public Vector3 ToVector3()
        {
            return new Vector3(x, 0, y);    // we assume an XZ plane
        }

        public override string ToString()
        {
            return ("[" + x.ToString() + "," + y.ToString() + "]");
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
                return new Index2();
            }
        }


        #region Operator Overloads

        public override bool Equals(object ob)
        {
            if (ob is Index2)
            {
                Index2 n = (Index2)ob;
                return (x == n.x) && (y == n.y);
            }
            return false;
        }
        public bool Equals(Index2 n)
        {
            return (x == n.x) && (y == n.y);
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public static bool operator ==(Index2 a, Index2 b)
        {
            return (a.x == b.x) && (a.y == b.y);
        }
        public static bool operator !=(Index2 a, Index2 b)
        {
            return !(a == b);
        }

        public static Index2 operator +(Index2 a, Index2 b)
        {
            Index2 n = new Index2();
            n.x = a.x + b.x;
            n.y = a.y + b.y;
            return n;
        }
        public static Index2 operator -(Index2 a, Index2 b)
        {
            Index2 n = new Index2();
            n.x = a.x - b.x;
            n.y = a.y - b.y;
            return n;
        }

        #endregion Operator Overloads
    }
}
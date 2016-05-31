using UnityEngine;

namespace Immersio.Utility
{
    public struct IndexDirection2
    {
        public enum DirectionEnum
        {
            Zero, Up, Down, Right, Left
        }
        public static IndexDirection2[] AllValidNonZeroDirections
        {
            get { return new IndexDirection2[] { up, down, right, left }; }
        }
        public static DirectionEnum[] AllValidNonZeroDirectionsEnum
        {
            get { return new DirectionEnum[] { DirectionEnum.Up, DirectionEnum.Down, DirectionEnum.Right, DirectionEnum.Left }; }
        }


        int x, y;
        public int X {
            get { return x; }
            private set {
                x = Mathf.Clamp(value, -1, 1);
                if (x != 0) { y = 0; }    // Either _x can be nonzero or _y can, but not both
            }
        }
        public int Y {
            get { return y; }
            private set {
                y = Mathf.Clamp(value, -1, 1);
                if (y != 0) { x = 0; }    // Either _x can be nonzero or _y can, but not both
            }
        }


        public IndexDirection2(int x, int y)
        {
            this.x = x;
            this.y = y;
            X = x;
            Y = y;
        }
        public IndexDirection2(float pX, float pY)
        {
            x = (int)Mathf.Round(pX);
            y = (int)Mathf.Round(pY);
            X = x;
            Y = y;
        }
        public IndexDirection2(IndexDirection2 t)
        {
            x = t.X;
            y = t.Y;
            X = x;
            Y = y;
        }
        public IndexDirection2(Vector2 v)
        {
            x = (int)Mathf.Round(v.x);
            y = (int)Mathf.Round(v.y);
            X = x;
            Y = y;
        }
        public IndexDirection2(Vector3 v)
        {
            x = (int)Mathf.Round(v.x);
            y = (int)Mathf.Round(v.z);  // we assume an x-z plane
            X = x;
            Y = y;
        }     
        public IndexDirection2(DirectionEnum d)
        {
            IndexDirection2 n = FromDirectionEnum(d);
            x = n.X;
            y = n.Y;
            X = x;
            Y = y;
        }


        readonly public static IndexDirection2 zero = new IndexDirection2(0, 0);
        readonly public static IndexDirection2 up = new IndexDirection2(0, 1);
        readonly public static IndexDirection2 down = new IndexDirection2(0, -1);
        readonly public static IndexDirection2 right = new IndexDirection2(1, 0);
        readonly public static IndexDirection2 left = new IndexDirection2(-1, 0);

        public bool IsZero() { return Equals(zero); }
        public bool IsUp() { return Equals(up); }
        public bool IsDown() { return Equals(down); }
        public bool IsRight() { return Equals(right); }
        public bool IsLeft() { return Equals(left); }

        public IndexDirection2 Reversed { get { return new IndexDirection2(-x, -y); } }
        public void Reverse()
        {
            x *= -1;
            y *= -1;
        }


        public DirectionEnum ToDirectionEnum()
        {
            if (IsLeft()) { return DirectionEnum.Left; }
            if (IsRight()) { return DirectionEnum.Right; }
            if (IsDown()) { return DirectionEnum.Down; }
            if (IsUp()) { return DirectionEnum.Up; }

            return DirectionEnum.Zero;
        }
        public static IndexDirection2 FromDirectionEnum(DirectionEnum d)
        {
            IndexDirection2 n;
            switch (d)
            {
                case DirectionEnum.Zero: n = zero; break;
                case DirectionEnum.Up: n = up; break;
                case DirectionEnum.Down: n = down; break;
                case DirectionEnum.Right: n = right; break;
                case DirectionEnum.Left: n = left; break;
                default: n = zero; break;
            }
            return n;
        }

        public Uniblocks.Direction ToUniblocksDirectionEnum()
        {
            if (IsLeft()) { return Uniblocks.Direction.left; }
            if (IsRight()) { return Uniblocks.Direction.right; }
            if (IsDown()) { return Uniblocks.Direction.down; }
            if (IsUp()) { return Uniblocks.Direction.up; }

            return Uniblocks.Direction.up;
        }

        public Index2 ToIndex2()
        {
            return new Index2(x, y);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
        public Vector3 ToVector3()
        {
            return new Vector3(x, 0, y);      // we assume an x-z plane
        }

        public override string ToString()
        {
            return ("[" + x.ToString() + "," + y.ToString() + "]");
        }
        public static IndexDirection2 FromString(string indexString)
        {
            string[] splitString = indexString.Split(',');

            try
            {
                return new IndexDirection2(int.Parse(splitString[0]), int.Parse(splitString[1]));
            }
            catch (System.Exception)
            {
                Debug.LogError("TileDirection.FromString: Invalid format. String must be in \"x,y\" format.");
                return zero;
            }
        }


        #region Operator Overloads

        public override bool Equals(object ob)
        {
            if (ob is IndexDirection2)
            {
                IndexDirection2 t = (IndexDirection2)ob;
                return (x == t.x) && (y == t.y);
            }
            return false;
        }
        public bool Equals(IndexDirection2 t)
        {
            return (x == t.x) && (y == t.y);
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public static bool operator ==(IndexDirection2 a, IndexDirection2 b)
        {
            return (a.x == b.x) && (a.y == b.y);
        }
        public static bool operator !=(IndexDirection2 a, IndexDirection2 b)
        {
            return !(a == b);
        }

        #endregion Operator Overloads
    }
}
namespace Cubiquity
{
    namespace Impl
    {
        public struct CuOctreeNode
        {
            // Disable 'Field ... is never assigned to'
            // warnings as this structure is just for interop
#pragma warning disable 0649
            public int posX;
            public int posY;
            public int posZ;

            public uint structureLastChanged;
            public uint propertiesLastChanged;
            public uint meshLastChanged;
            public uint nodeOrChildrenLastChanged;

            public uint childHandle000;
            public uint childHandle001;
            public uint childHandle010;
            public uint childHandle011;
            public uint childHandle100;
            public uint childHandle101;
            public uint childHandle110;
            public uint childHandle111;

            public byte hasMesh;
            public byte renderThisNode;

            public byte height;
#pragma warning restore 0649
        }
    }
}
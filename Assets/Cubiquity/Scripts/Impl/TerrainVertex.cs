namespace Cubiquity
{
    namespace Impl
    {
        public struct TerrainVertex
        {
            // Disable 'Field ... is never assigned to'
            // warnings as this structure is just for interop
#pragma warning disable 0649
            public ushort x;
            public ushort y;
            public ushort z;
            public ushort normal;
            public byte m0;
            public byte m1;
            public byte m2;
            public byte m3;
            public byte m4;
            public byte m5;
            public byte m6;
            public byte m7;
#pragma warning restore 0649
        }
    }
}
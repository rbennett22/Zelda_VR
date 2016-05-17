namespace Cubiquity
{
    namespace Impl
    {
        public struct ColoredCubesVertex
        {
            // Disable 'Field ... is never assigned to'
            // warnings as this structure is just for interop
#pragma warning disable 0649
            public byte x;
            public byte y;
            public byte z;
            public byte normal;
            public QuantizedColor color;
#pragma warning restore 0649
        }
    }
}
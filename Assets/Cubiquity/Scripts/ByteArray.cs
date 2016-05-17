using System;

namespace Cubiquity
{
    /// Provides a simple array of bytes with direct access to each element.
    /**
	 * This structure exists because the built in C# array is a reference type, whereas for our purposes we want a value type. Apart
	 * from that, usage should be intuitive and what you would expect from an array. Note that user code is not expected to create a
	 * ByteArray directly, but should simply use any which are provided by %Cubiquity (e.g. to modify the weights in a MaterialSet).
	 */

    public struct ByteArray
    {
        // For our purposes we only need byte arrays with exactly eight elements, as the ByteArray is only used by MaterialSet and
        // a Terrain volume stores eight material weights per voxel. If we need the ByteArray for other purposes in the future then
        // we might make this more generic.
        private ulong data;

        /// The number of elements in this array.
        public int Length
        {
            get
            {
                // Currently all ByteArrays are store eight bytes.
                return 8;
            }
        }

        /// Provides access to the elements of the array.
        /**
		 * \param index The index of the element to access.
		 * \throws ArgumentOutOfRangeException Thrown if the specified index is greater of equal to the length of the array.
		 */
        public byte this[uint index]
        {
            get
            {
                if (index >= Length)
                {
                    throw new ArgumentOutOfRangeException("Index out of range");
                }

                return (byte)(getEightBitsAt(index * 8));
            }
            set
            {
                if (index >= Length)
                {
                    throw new ArgumentOutOfRangeException("Index out of range");
                }

                setEightBitsAt(index * 8, value);
            }
        }

        private ulong getEightBitsAt(uint offset)
        {
            ulong mask = 0x000000FF;
            ulong result = data;
            result >>= (int)offset;
            result &= mask;
            return result;
        }

        private void setEightBitsAt(uint offset, ulong val)
        {
            ulong mask = 0x000000FF;
            mask <<= (int)offset;

            data = (uint)(data & (uint)(~mask));

            val <<= (int)offset;
            val &= mask;

            data |= val;
        }
    }
}
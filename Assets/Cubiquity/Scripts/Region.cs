namespace Cubiquity
{
    [System.Serializable]
    /// Denotes a region of 3D space, typically representing the bounds for a volume.
    public class Region
    {
        /// The (inclusive) lower corner of the region.
        public Vector3i lowerCorner;

        /// The (inclusive) upper corner of the region.
        public Vector3i upperCorner;

        /// Constructs a Region from corners specified as seperate integer parameters.
        public Region(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ)
        {
            lowerCorner = new Vector3i(lowerX, lowerY, lowerZ);
            upperCorner = new Vector3i(upperX, upperY, upperZ);
        }

        /// Constructs a Region from corners specified as Vector3i parameters.
        public Region(Vector3i lowerCorner, Vector3i upperCorner)
        {
            this.lowerCorner = lowerCorner;
            this.upperCorner = upperCorner;
        }

        /// Returns a System.String that represents the current Cubiquity.Region.
        public override string ToString()
        {
            return string.Format("Region({0}, {1}, {2}, {3}, {4}, {5})",
                lowerCorner.x, lowerCorner.y, lowerCorner.z,
                upperCorner.x, upperCorner.y, upperCorner.z);
        }
    }
}
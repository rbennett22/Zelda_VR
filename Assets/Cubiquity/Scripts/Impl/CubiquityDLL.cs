using System;
using System.Runtime.InteropServices;
using System.Text;

using UnityEngine;

namespace Cubiquity
{
    namespace Impl
    {
        [StructLayout(
            LayoutKind.Sequential,      //must specify a layout
            CharSet = CharSet.Ansi)]    //if you intend to use char
        public struct ToBePassed
        {
            public Int32 Num1;
            public Int32 Num2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            public Char[] Data;    //specify the size using MarshalAs
        }

        public class CubiquityDLL
        {
            private const string dllToImport = "CubiquityC";
            private static string logFilePath;

            const int CU_OK = 0;

            const uint requiredMajorVersion = 1;
            const uint requiredMinorVersion = 2;
            const uint requiredPatchVersion = 0;
            const uint requiredBuildVersion = 0;

            // This static constructor is supposed to make sure that the Cubiquity.dll is in the right place before the DllImport is done.
            // It doesn't seem to work, because in Standalone builds the message below is printed after the exception about the .dll not
            // being found. We need to look into this further.
            static CubiquityDLL()
            {
                try
                {
                    Installation.ValidateAndFix();

                    uint majorVersion;
                    uint minorVersion;
                    uint patchVersion;
                    uint buildVersion;
                    cuGetVersionNumber(out majorVersion, out minorVersion, out patchVersion, out buildVersion);

                    if ((majorVersion != requiredMajorVersion) ||
                        (minorVersion != requiredMinorVersion) ||
                        (patchVersion != requiredPatchVersion) ||
                        (buildVersion != requiredBuildVersion))
                    {
                        throw new CubiquityInstallationException("Wrong version of Cubiquity native code library found! " +
                            "Expected version " + requiredMajorVersion + "." + requiredMinorVersion + "." + requiredPatchVersion + "." + requiredBuildVersion +
                            " but found version " + majorVersion + "." + minorVersion + "." + patchVersion + "." + buildVersion + ".\n" +
                            "If you are using the development version of Cubiquity (from the Git repository) then try a stable snapshot instead.\n");
                    }

                    logFilePath = GetLogFilePath();
                }
                // It seems we should not allow exceptions to propagate out of here, as Unity then
                // repeatedly calls this function (probably because it is a static constructor).
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            private static void Validate(int returnCode)
            {
                if (returnCode != CU_OK)
                {
                    throw new CubiquityException("An exception has occured inside the Cubiquity native code library.\n" +
                        "Error code \'" + GetErrorCodeAsString(returnCode) + "\' with message \"" + GetLastErrorMessage() + "\".\n" +
                        "Please see the log file '" + logFilePath + "' for more details.\n");
                }
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Version functions
            ////////////////////////////////////////////////////////////////////////////////
            [DllImport(dllToImport)]
            private static extern int cuGetVersionNumber(out uint majorVersion, out uint minorVersion, out uint patchVersion, out uint buildVersion);
            public static void GetVersionNumber(out uint majorVersion, out uint minorVersion, out uint patchVersion, out uint buildVersion)
            {
                Validate(cuGetVersionNumber(out majorVersion, out minorVersion, out patchVersion, out buildVersion));
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Logging functions
            ////////////////////////////////////////////////////////////////////////////////
            [DllImport(dllToImport)]
            private static extern IntPtr cuGetLogFilePath();
            public static string GetLogFilePath()
            {
                IntPtr result = cuGetLogFilePath();
                string stringResult = Marshal.PtrToStringAnsi(result);
                return stringResult;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Error handling functions
            ////////////////////////////////////////////////////////////////////////////////
            [DllImport(dllToImport)]
            private static extern IntPtr cuGetErrorCodeAsString(int errorCode);
            public static string GetErrorCodeAsString(int errorCode)
            {
                IntPtr result = cuGetErrorCodeAsString(errorCode);
                string stringResult = Marshal.PtrToStringAnsi(result);
                return stringResult;
            }

            [DllImport(dllToImport)]
            private static extern IntPtr cuGetLastErrorMessage();
            public static string GetLastErrorMessage()
            {
                IntPtr result = cuGetLastErrorMessage();
                string stringResult = Marshal.PtrToStringAnsi(result);
                return stringResult;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Volume functions
            ////////////////////////////////////////////////////////////////////////////////
            [DllImport(dllToImport)]
            private static extern int cuNewEmptyColoredCubesVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, StringBuilder datasetName, uint baseNodeSize, out uint result);
            public static uint NewEmptyColoredCubesVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, string datasetName, uint baseNodeSize)
            {
                uint result;
                Validate(cuNewEmptyColoredCubesVolume(lowerX, lowerY, lowerZ, upperX, upperY, upperZ, new StringBuilder(datasetName), baseNodeSize, out result));
                return result;
            }

            [DllImport(dllToImport)]
            private static extern int cuNewColoredCubesVolumeFromVDB(StringBuilder datasetName, uint writePermissions, uint baseNodeSize, out uint result);
            public static uint NewColoredCubesVolumeFromVDB(string datasetName, VolumeData.WritePermissions writePermissions, uint baseNodeSize)
            {
                uint result;
                Validate(cuNewColoredCubesVolumeFromVDB(new StringBuilder(datasetName), (uint)writePermissions, baseNodeSize, out result));
                return result;
            }

            [DllImport(dllToImport)]
            private static extern int cuNewColoredCubesVolumeFromVolDat(StringBuilder voldatFolder, StringBuilder datasetName, uint baseNodeSize, out uint result);
            public static uint NewColoredCubesVolumeFromVolDat(string voldatFolder, string datasetName, uint baseNodeSize)
            {
                uint result;
                Validate(cuNewColoredCubesVolumeFromVolDat(new StringBuilder(voldatFolder), new StringBuilder(datasetName), baseNodeSize, out result));
                return result;
            }

            [DllImport(dllToImport)]
            private static extern int cuNewColoredCubesVolumeFromHeightmap(StringBuilder heightmapFileName, StringBuilder colormapFileName, StringBuilder datasetName, uint baseNodeSize, out uint result);
            public static uint NewColoredCubesVolumeFromHeightmap(string heightmapFileName, string colormapFileName, string datasetName, uint baseNodeSize)
            {
                uint result;
                Validate(cuNewColoredCubesVolumeFromHeightmap(new StringBuilder(heightmapFileName), new StringBuilder(colormapFileName), new StringBuilder(datasetName), baseNodeSize, out result));
                return result;
            }

            [DllImport(dllToImport)]
            private static extern int cuUpdateVolume(uint volumeHandle, float eyePosX, float eyePosY, float eyePosZ, float lodThreshold, out uint isUpToDate);
            public static bool UpdateVolume(uint volumeHandle, float eyePosX, float eyePosY, float eyePosZ, float lodThreshold)
            {
                uint isUpToDate;
                Validate(cuUpdateVolume(volumeHandle, eyePosX, eyePosY, eyePosZ, lodThreshold, out isUpToDate));
                return isUpToDate != 0;
            }

            [DllImport(dllToImport)]
            private static extern int cuGetEnclosingRegion(uint volumeHandle, out int lowerX, out int lowerY, out int lowerZ, out int upperX, out int upperY, out int upperZ);
            public static void GetEnclosingRegion(uint volumeHandle, out int lowerX, out int lowerY, out int lowerZ, out int upperX, out int upperY, out int upperZ)
            {
                Validate(cuGetEnclosingRegion(volumeHandle, out lowerX, out lowerY, out lowerZ, out upperX, out upperY, out upperZ));
            }

            [DllImport(dllToImport)]
            private static extern int cuDeleteVolume(uint volumeHandle);
            public static void DeleteVolume(uint volumeHandle)
            {
                Validate(cuDeleteVolume(volumeHandle));
            }

            [DllImport(dllToImport)]
            private static extern int cuAcceptOverrideChunks(uint volumeHandle);
            public static void AcceptOverrideChunks(uint volumeHandle)
            {
                Validate(cuAcceptOverrideChunks(volumeHandle));
            }

            [DllImport(dllToImport)]
            private static extern int cuDiscardOverrideChunks(uint volumeHandle);
            public static void DiscardOverrideChunks(uint volumeHandle)
            {
                Validate(cuDiscardOverrideChunks(volumeHandle));
            }

            //--------------------------------------------------------------------------------

            [DllImport(dllToImport)]
            private static extern int cuNewEmptyTerrainVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, StringBuilder datasetName, uint baseNodeSize, out uint result);
            public static uint NewEmptyTerrainVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, string datasetName, uint baseNodeSize)
            {
                uint result;
                Validate(cuNewEmptyTerrainVolume(lowerX, lowerY, lowerZ, upperX, upperY, upperZ, new StringBuilder(datasetName), baseNodeSize, out result));
                return result;
            }

            [DllImport(dllToImport)]
            private static extern int cuNewTerrainVolumeFromVDB(StringBuilder datasetName, uint writePermissions, uint baseNodeSize, out uint result);
            public static uint NewTerrainVolumeFromVDB(string datasetName, VolumeData.WritePermissions writePermissions, uint baseNodeSize)
            {
                uint result;
                Validate(cuNewTerrainVolumeFromVDB(new StringBuilder(datasetName), (uint)writePermissions, baseNodeSize, out result));
                return result;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Voxel functions
            ////////////////////////////////////////////////////////////////////////////////
#if CUBIQUITY_USE_UNSAFE
            // It seems we can't make a generic version of this functions as it gives error CS0208.
            // Apparently that is not easily fixed in our situation, see here: http://goo.gl/blN834
            [DllImport(dllToImport)]
            unsafe private static extern int cuGetVoxel(uint volumeHandle, int x, int y, int z, void* result);
            unsafe public static QuantizedColor GetQuantizedColorVoxel(uint volumeHandle, int x, int y, int z)
            {
                QuantizedColor result;
                Validate(cuGetVoxel(volumeHandle, x, y, z, &result));
                return result;
            }
            unsafe public static MaterialSet GetMaterialSetVoxel(uint volumeHandle, int x, int y, int z)
            {
                MaterialSet result;
                Validate(cuGetVoxel(volumeHandle, x, y, z, &result));
                return result;
            }

            // It seems we can't make a generic version of this functions as it gives error CS0208.
            // Apparently that is not easily fixed in our situation, see here: http://goo.gl/blN834
            [DllImport(dllToImport)]
            unsafe private static extern int cuSetVoxel(uint volumeHandle, int x, int y, int z, void* value);
            unsafe public static void SetVoxel(uint volumeHandle, int x, int y, int z, QuantizedColor value)
            {
                   Validate(cuSetVoxel(volumeHandle, x, y, z, &value));
            }
            unsafe public static void SetVoxel(uint volumeHandle, int x, int y, int z, MaterialSet value)
            {
                   Validate(cuSetVoxel(volumeHandle, x, y, z, &value));
            }
#else
            [DllImport(dllToImport)]
            private static extern int cuGetVoxel(uint volumeHandle, int x, int y, int z, IntPtr result);
            public static VoxelType GetVoxel<VoxelType>(uint volumeHandle, int x, int y, int z)
            {
                IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VoxelType)));
                Validate(cuGetVoxel(volumeHandle, x, y, z, pointer));
                VoxelType value = (VoxelType)(Marshal.PtrToStructure(pointer, typeof(VoxelType)));
                Marshal.FreeHGlobal(pointer);
                return value;
            }
            // The unsafe version of this code cannot provide a generic version so
            // to mathch that interface we also provide non-generic versions here.
            public static QuantizedColor GetQuantizedColorVoxel(uint volumeHandle, int x, int y, int z)
            {
                return GetVoxel<QuantizedColor>(volumeHandle, x, y, z);
            }
            public static MaterialSet GetMaterialSetVoxel(uint volumeHandle, int x, int y, int z)
            {
                return GetVoxel<MaterialSet>(volumeHandle, x, y, z);
            }

            [DllImport(dllToImport)]
            private static extern int cuSetVoxel(uint volumeHandle, int x, int y, int z, IntPtr value);
            public static void SetVoxel<VoxelType>(uint volumeHandle, int x, int y, int z, VoxelType value)
            {
                // See http://stackoverflow.com/a/3939963
                IntPtr ptrValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VoxelType)));
                Marshal.StructureToPtr(value, ptrValue, true);
                Validate(cuSetVoxel(volumeHandle, x, y, z, ptrValue));
                Marshal.FreeHGlobal(ptrValue);
            }
#endif

            ////////////////////////////////////////////////////////////////////////////////
            // Octree functions
            ////////////////////////////////////////////////////////////////////////////////
            [DllImport(dllToImport)]
            private static extern int cuHasRootOctreeNode(uint volumeHandle, out uint result);
            public static uint HasRootOctreeNode(uint volumeHandle)
            {
                uint result;
                Validate(cuHasRootOctreeNode(volumeHandle, out result));
                return result;
            }

            [DllImport(dllToImport)]
            private static extern int cuGetRootOctreeNode(uint volumeHandle, out uint result);
            public static uint GetRootOctreeNode(uint volumeHandle)
            {
                uint result;
                Validate(cuGetRootOctreeNode(volumeHandle, out result));
                return result;
            }

            /*[DllImport (dllToImport)]
			private static extern int cuHasChildNode(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
			public static uint HasChildNode(uint nodeHandle, uint childX, uint childY, uint childZ)
			{
				uint result;
				Validate(cuHasChildNode(nodeHandle, childX, childY, childZ, out result));
				return result;
			}

			[DllImport (dllToImport)]
			private static extern int cuGetChildNode(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
			public static uint GetChildNode(uint nodeHandle, uint childX, uint childY, uint childZ)
			{
				uint result;
				Validate(cuGetChildNode(nodeHandle, childX, childY, childZ, out result));
				return result;
			}*/

            /*[DllImport (dllToImport)]
			private static extern int cuNodeHasMesh(uint nodeHandle, out uint result);
			public static uint NodeHasMesh(uint nodeHandle)
			{
				uint result;
				Validate(cuNodeHasMesh(nodeHandle, out result));
				return result;
			}*/

            /*[DllImport (dllToImport)]
			private static extern int cuGetNodePosition(uint nodeHandle, out int x, out int y, out int z);
			public static void GetNodePosition(uint nodeHandle, out int x, out int y, out int z)
			{
				Validate(cuGetNodePosition(nodeHandle, out x, out y, out z));
			}*/

            /*[DllImport (dllToImport)]
			private static extern int cuGetMeshLastUpdated(uint nodeHandle, out uint result);
			public static uint GetMeshLastUpdated(uint nodeHandle)
			{
				uint result;
				Validate(cuGetMeshLastUpdated(nodeHandle, out result));
				return result;
			}*/

            /*[DllImport(dllToImport)]
            private static extern int cuGetMeshOrChildMeshLastUpdated(uint nodeHandle, out uint result);
            public static uint GetMeshOrChildMeshLastUpdated(uint nodeHandle)
            {
                uint result;
                Validate(cuGetMeshOrChildMeshLastUpdated(nodeHandle, out result));
                return result;
            }*/

            /*[DllImport (dllToImport)]
			private static extern int cuRenderThisNode(uint nodeHandle, out uint result);
			public static uint RenderThisNode(uint nodeHandle)
			{
				uint result;
				Validate(cuRenderThisNode(nodeHandle, out result));
				return result;
			}*/

            /*[DllImport(dllToImport)]
            private static extern int cuGetLastChanged(uint nodeHandle, out uint result);
            public static uint GetLastChanged(uint nodeHandle)
            {
                uint result;
                Validate(cuGetLastChanged(nodeHandle, out result));
                return result;
            }*/

            [DllImport(dllToImport)]
            private static extern int cuGetOctreeNode(uint nodeHandle, out CuOctreeNode result);
            public static CuOctreeNode GetOctreeNode(uint nodeHandle)
            {
                CuOctreeNode result;
                Validate(cuGetOctreeNode(nodeHandle, out result));
                return result;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Mesh functions
            ////////////////////////////////////////////////////////////////////////////////

            [DllImport(dllToImport)]
            private static extern int cuSetLodRange(uint volumeHandle, int minimumLOD, int maximumLOD);
            public static void SetLodRange(uint volumeHandle, int minimumLOD, int maximumLOD)
            {
                Validate(cuSetLodRange(volumeHandle, minimumLOD, maximumLOD));
            }

#if CUBIQUITY_USE_UNSAFE
            // It seems we can't make a generic version of this functions as it gives error CS0208.
            // Apparently that is not easily fixed in our situation, see here: http://goo.gl/blN834
            [DllImport(dllToImport)]
            unsafe private static extern int cuGetMesh(uint octreeNodeHandle, ushort* noOfVertices, void** vertices, uint* noOfIndices, ushort** indices);
            unsafe public static void GetColoredCubesMesh(uint octreeNodeHandle, ushort* noOfVertices, ColoredCubesVertex** vertices, uint* noOfIndices, ushort** indices)
            {
                Validate(cuGetMesh(octreeNodeHandle, noOfVertices, (void**)vertices, noOfIndices, indices));
            }
            unsafe public static void GetTerrainMesh(uint octreeNodeHandle, ushort* noOfVertices, TerrainVertex** vertices, uint* noOfIndices, ushort** indices)
            {
                Validate(cuGetMesh(octreeNodeHandle, noOfVertices, (void**)vertices, noOfIndices, indices));
            }
#else
            [DllImport(dllToImport)]
            private static extern int cuGetMesh(uint octreeNodeHandle, out ushort noOfVertices, out IntPtr vertices, out uint noOfIndices, out IntPtr indices);
            public static void GetMesh<VertexType>(uint octreeNodeHandle, out VertexType[] vertices, out ushort[] indices)
            {
                ushort noOfVertices;
                IntPtr ptrVertices;
                uint noOfIndices;
                IntPtr ptrIndices;

                Validate(cuGetMesh(octreeNodeHandle, out noOfVertices, out ptrVertices, out noOfIndices, out ptrIndices));

                vertices = new VertexType[noOfVertices];

                // Based on http://stackoverflow.com/a/1086462
                long longPtrVertices = ptrVertices.ToInt64();
                for (ushort ct = 0; ct < noOfVertices; ct++)
                {
                    IntPtr offsetPtr = new IntPtr(longPtrVertices);
                    vertices[ct] = (VertexType)(Marshal.PtrToStructure(offsetPtr, typeof(VertexType)));
                    longPtrVertices += Marshal.SizeOf(typeof(VertexType));
                }

                indices = new ushort[noOfIndices];

                // Based on http://stackoverflow.com/a/1086462
                long longPtrIndices = ptrIndices.ToInt64();
                for (ushort ct = 0; ct < noOfIndices; ct++)
                {
                    IntPtr offsetPtr = new IntPtr(longPtrIndices);
                    indices[ct] = (ushort)(Marshal.PtrToStructure(offsetPtr, typeof(ushort)));
                    longPtrIndices += Marshal.SizeOf(typeof(ushort));
                }
            }
#endif

            ////////////////////////////////////////////////////////////////////////////////
            // Clock functions
            ////////////////////////////////////////////////////////////////////////////////
            [DllImport(dllToImport)]
            private static extern int cuGetCurrentTime(out uint result);
            public static uint GetCurrentTime()
            {
                uint result;
                Validate(cuGetCurrentTime(out result));
                return result;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Raycasting functions
            ////////////////////////////////////////////////////////////////////////////////
            [DllImport(dllToImport)]
            private static extern int cuPickFirstSolidVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ, out uint result);
            public static uint PickFirstSolidVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ)
            {
                uint result;
                Validate(cuPickFirstSolidVoxel(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
                return result;
            }

            [DllImport(dllToImport)]
            private static extern int cuPickLastEmptyVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ, out uint result);
            public static uint PickLastEmptyVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ)
            {
                uint result;
                Validate(cuPickLastEmptyVoxel(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
                return result;
            }

            [DllImport(dllToImport)]
            private static extern int cuPickTerrainSurface(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out float resultX, out float resultY, out float resultZ, out uint result);
            public static uint PickTerrainSurface(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out float resultX, out float resultY, out float resultZ)
            {
                uint result;
                Validate(cuPickTerrainSurface(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
                return result;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Editing functions
            ////////////////////////////////////////////////////////////////////////////////

            [DllImport(dllToImport)]
            private static extern int cuSculptTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount);
            public static void SculptTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount)
            {
                Validate(cuSculptTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount));
            }

            [DllImport(dllToImport)]
            private static extern int cuBlurTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount);
            public static void BlurTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount)
            {
                Validate(cuBlurTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount));
            }

            [DllImport(dllToImport)]
            private static extern int cuBlurTerrainVolumeRegion(uint volumeHandle, int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ);
            public static void BlurTerrainVolumeRegion(uint volumeHandle, int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ)
            {
                Validate(cuBlurTerrainVolumeRegion(volumeHandle, lowerX, lowerY, lowerZ, upperX, upperY, upperZ));
            }

            [DllImport(dllToImport)]
            private static extern int cuPaintTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount, uint materialIndex);
            public static void PaintTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount, uint materialIndex)
            {
                Validate(cuPaintTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount, materialIndex));
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Volume generation functions
            ////////////////////////////////////////////////////////////////////////////////
            [DllImport(dllToImport)]
            private static extern int cuGenerateFloor(uint volumeHandle, int lowerLayerHeight, uint lowerLayerMaterial, int upperLayerHeight, uint upperLayerMaterial);
            public static void GenerateFloor(uint volumeHandle, int lowerLayerHeight, uint lowerLayerMaterial, int upperLayerHeight, uint upperLayerMaterial)
            {
                Validate(cuGenerateFloor(volumeHandle, lowerLayerHeight, lowerLayerMaterial, upperLayerHeight, upperLayerMaterial));
            }
        }
    }
}
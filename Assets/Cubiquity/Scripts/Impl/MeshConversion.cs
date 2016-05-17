using System;
using UnityEngine;

namespace Cubiquity
{
    namespace Impl
    {
        public class MeshConversion
        {
#if CUBIQUITY_USE_UNSAFE
            unsafe public static void BuildMeshFromNodeHandleForColoredCubesVolume(Mesh mesh, uint nodeHandle, bool onlyPositions)
            {
                // Get the data from Cubiquity.
                ushort noOfVertices; ColoredCubesVertex* vertices; uint noOfIndices; ushort* indices;
                CubiquityDLL.GetColoredCubesMesh(nodeHandle, &noOfVertices, &vertices, &noOfIndices, &indices);
#else
            public static void BuildMeshFromNodeHandleForColoredCubesVolume(Mesh mesh, uint nodeHandle, bool onlyPositions)
            {
                // Get the data from Cubiquity.
                ColoredCubesVertex[] vertices;
                ushort[] indices;
                CubiquityDLL.GetMesh<ColoredCubesVertex>(nodeHandle, out vertices, out indices);
                int noOfVertices = vertices.Length;
                int noOfIndices = indices.Length;
#endif

                // Cubiquity uses 16-bit index arrays to save space, and it appears Unity does the same (at least, there is
                // a limit of 65535 vertices per mesh). However, the Mesh.triangles property is of the signed 32-bit int[]
                // type rather than the unsigned 16-bit ushort[] type. Perhaps this is so they can switch to 32-bit index
                // buffers in the future? At any rate, it means we have to perform a conversion.
                int[] indicesAsInt = new int[noOfIndices];
                for (int ct = 0; ct < noOfIndices; ct++)
                {
                    indicesAsInt[ct] = indices[ct];
                }

                // Clear any previous mesh data.
                mesh.Clear(true);

                // Required for the CubicVertex decoding process.
                Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);

                // Copy the vertex positions from Cubiquity into the Unity mesh.
                Vector3[] positions = new Vector3[noOfVertices];
                for (int ct = 0; ct < noOfVertices; ct++)
                {
                    // Get and decode the position
                    positions[ct].Set(vertices[ct].x, vertices[ct].y, vertices[ct].z);
                    positions[ct] -= offset;
                }
                // Assign vertex data to the mesh.
                mesh.vertices = positions;

                // For collision meshes the vertex positions are enough, but
                // for meshes which are rendered we want all vertex attributes.
                if (!onlyPositions)
                {
                    Color32[] colors32 = new Color32[noOfVertices];
                    for (int ct = 0; ct < noOfVertices; ct++)
                    {
                        // Get and decode the color
                        colors32[ct] = (Color32)vertices[ct].color;
                    }
                    // Assign vertex data to the mesh.
                    mesh.colors32 = colors32;
                }

                // Assign index data to the mesh.
                mesh.triangles = indicesAsInt;
            }

#if CUBIQUITY_USE_UNSAFE
            unsafe public static void BuildMeshFromNodeHandleForTerrainVolume(Mesh mesh, uint nodeHandle, bool onlyPositions)
            {
                // Get the data from Cubiquity.
                ushort noOfVertices; TerrainVertex* vertices; uint noOfIndices; ushort* indices;
                CubiquityDLL.GetTerrainMesh(nodeHandle, &noOfVertices, &vertices, &noOfIndices, &indices);
#else
            public static void BuildMeshFromNodeHandleForTerrainVolume(Mesh mesh, uint nodeHandle, bool onlyPositions)
            {
                // Get the data from Cubiquity.
                TerrainVertex[] vertices;
                ushort[] indices;
                CubiquityDLL.GetMesh<TerrainVertex>(nodeHandle, out vertices, out indices);
                int noOfVertices = vertices.Length;
                int noOfIndices = indices.Length;
#endif

                // Cubiquity uses 16-bit index arrays to save space, and it appears Unity does the same (at least, there is
                // a limit of 65535 vertices per mesh). However, the Mesh.triangles property is of the signed 32-bit int[]
                // type rather than the unsigned 16-bit ushort[] type. Perhaps this is so they can switch to 32-bit index
                // buffers in the future? At any rate, it means we have to perform a conversion.
                int[] indicesAsInt = new int[noOfIndices];
                for (int ct = 0; ct < noOfIndices; ct++)
                {
                    indicesAsInt[ct] = indices[ct];
                }

                // Clear any previous mesh data.
                mesh.Clear(true);

                // Create the arrays which we'll copy the data to.
                Vector3[] positions = new Vector3[noOfVertices];
                Vector3[] normals = new Vector3[noOfVertices];
                Color32[] colors32 = new Color32[noOfVertices];
                Vector2[] uv = new Vector2[noOfVertices];
                Vector2[] uv2 = new Vector2[noOfVertices];

                // Copy the vertex positions from Cubiquity into the Unity mesh.
                for (int ct = 0; ct < noOfVertices; ct++)
                {
                    // Get and decode the position
                    positions[ct].Set(vertices[ct].x, vertices[ct].y, vertices[ct].z);
                    positions[ct] *= (1.0f / 256.0f);
                }
                // Assign vertex data to the mesh.
                mesh.vertices = positions;

                // For collision meshes the vertex positions are enough, but
                // for meshes which are rendered we want all vertex attributes.
                if (!onlyPositions)
                {
                    for (int ct = 0; ct < noOfVertices; ct++)
                    {
                        // Get the materials. Some are stored in color...
                        colors32[ct].r = vertices[ct].m0;
                        colors32[ct].g = vertices[ct].m1;
                        colors32[ct].b = vertices[ct].m2;
                        colors32[ct].a = vertices[ct].m3;

                        // And some are stored in UVs.
                        uv[ct].Set(vertices[ct].m4 / 255.0f, vertices[ct].m5 / 255.0f);
                        uv2[ct].Set(vertices[ct].m6 / 255.0f, vertices[ct].m7 / 255.0f);

                        // Get and decode the normal
                        ushort ux = (ushort)((vertices[ct].normal >> (ushort)8) & (ushort)0xFF);
                        ushort uy = (ushort)((vertices[ct].normal) & (ushort)0xFF);

                        // Convert to floats in the range [-1.0f, +1.0f].
                        float ex = ux * (1.0f / 127.5f) - 1.0f;
                        float ey = uy * (1.0f / 127.5f) - 1.0f;

                        // Reconstruct the origninal vector. This is a C++ implementation
                        // of Listing 2 of http://jcgt.org/published/0003/02/01/
                        float vx = ex;
                        float vy = ey;
                        float vz = 1.0f - Math.Abs(ex) - Math.Abs(ey);

                        if (vz < 0.0f)
                        {
                            float refX = ((1.0f - Math.Abs(vy)) * (vx >= 0.0f ? +1.0f : -1.0f));
                            float refY = ((1.0f - Math.Abs(vx)) * (vy >= 0.0f ? +1.0f : -1.0f));
                            vx = refX;
                            vy = refY;
                        }
                        normals[ct].Set(vx, vy, vz);
                    }
                    // Assign vertex data to the mesh.
                    mesh.normals = normals;
                    mesh.colors32 = colors32;
                    mesh.uv = uv;
                    mesh.uv2 = uv2;
                }

                // Assign index data to the mesh.
                mesh.triangles = indicesAsInt;
            }
        }
    }
}
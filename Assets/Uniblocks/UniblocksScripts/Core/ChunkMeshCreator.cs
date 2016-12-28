using System;
using System.Collections.Generic;
using UnityEngine;

// Handles mesh creation and all related functions.

namespace Uniblocks
{
    public enum MeshRotation
    {
        none, back, left, right
    }

    public class ChunkMeshCreator : MonoBehaviour
    {
        public Mesh Cube;


        Chunk _chunk;
        int _sizeX, _sizeY, _sizeZ;
        GameObject _noCollideCollider;


        // variables for storing the mesh data

        List<Vector3> Vertices = new List<Vector3>();

        List<List<int>> Faces = new List<List<int>>();
        List<Vector2> UVs = new List<Vector2>();
        int FaceCount;


        // variables for storing collider data

        List<Vector3> SolidColliderVertices = new List<Vector3>();

        List<int> SolidColliderFaces = new List<int>();
        int SolidFaceCount;
        List<Vector3> NoCollideVertices = new List<Vector3>();
        List<int> NoCollideFaces = new List<int>();
        int NoCollideFaceCount;

        bool _initialized;


        public void Initialize()
        {
            // set variables
            _chunk = GetComponent<Chunk>();
            _sizeX = _chunk.sizeX;
            _sizeY = _chunk.sizeY;
            _sizeZ = _chunk.sizeZ;

            // make a list for each material (each material is a submesh)
            for (int i = 0; i < GetComponent<Renderer>().materials.Length; i++)
            {
                Faces.Add(new List<int>());
            }

            _initialized = true;
        }


        // ==== Voxel updates =====================================================================================

        public void RebuildMesh()
        {
            if (!_initialized)
            {
                Initialize();
            }

            // destroy additional mesh containers
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            int x = 0, y = 0, z = 0;

            // Refresh neighbor chunks
            _chunk.AssignNeighbors();

            // for each voxel in Voxels, check if any of the voxel's faces are exposed, and if so, add their faces to the main mesh arrays (named Vertices and Faces)
            while (x < _sizeX)
            {
                while (y < _sizeY)
                {
                    while (z < _sizeZ)
                    {
                        ushort voxel = _chunk.GetVoxel(x, y, z);     // the current voxel data
                        if (voxel != 0)     // don't render empty blocks.
                        { 
                            Voxel voxelType = Engine.GetVoxelType(voxel);
                            if (voxelType.VCustomMesh)      // if not cube
                            {
                                if (!CheckAllAdjacent(x, y, z))
                                {
                                    // if any adjacent voxel isn't opaque, we render the mesh
                                    CreateCustomMesh(voxel, x, y, z, voxelType.VMesh);
                                }
                            }
                            else    // if cube
                            { 
                                Transparency tr = voxelType.VTransparency;
                                ColliderType ct = voxelType.VColliderType;

                                foreach (Direction d in Enum.GetValues(typeof(Direction)))
                                {
                                    if (CheckAdjacent(x, y, z, d, tr))
                                        CreateFace(voxel, (Facing)d, ct, x, y, z);
                                }

                                // if no collider, create a trigger cube collider
                                if (ct == ColliderType.none && Engine.GenerateColliders)
                                {
                                    AddCubeMesh(x, y, z, false);
                                }
                            } 
                        }
                        z++;
                    }
                    z = 0;
                    y++;
                }
                y = 0;
                x++;
            }

            // update mesh using the values from the arrays
            UpdateMesh(GetComponent<MeshFilter>().mesh);
        }

        bool CheckAdjacent(int x, int y, int z, Direction direction, Transparency transparency)
        { 
            // returns true if a face should be spawned
            Index idx = _chunk.GetAdjacentIndex(x, y, z, direction);
            ushort adjacentVoxel = _chunk.GetVoxel(idx);

            if (adjacentVoxel == ushort.MaxValue)
            {
                bool neighborChunkIsMissing = Engine.ShowBorderFaces || direction == Direction.up;
                return neighborChunkIsMissing;
            }

            Transparency result = Engine.GetVoxelType(adjacentVoxel).VTransparency; // get the transparency of the adjacent voxel

            // parse the result (taking into account the transparency of the adjacent block as well as the one doing this check)
            if (transparency == Transparency.transparent)
            {
                return (result != Transparency.transparent);
                // don't draw a transparent block next to another transparent block
                // draw a transparent block next to a solid or semi-transparent
            }
            else
            {
                return (result != Transparency.solid);
                // don't draw a solid block or a semi-transparent block next to a solid block
                // draw a solid block or a semi-transparent block next to both transparent and semi-transparent
            }
        }

        public bool CheckAllAdjacent(int x, int y, int z)
        { 
            // returns true if all adjacent voxels are solid
            for (int dir = 0; dir < 6; dir++)
            {
                if (Engine.GetVoxelType(_chunk.GetVoxel(_chunk.GetAdjacentIndex(x, y, z, (Direction)dir))).VTransparency != Transparency.solid)
                {
                    return false;
                }
            }
            return true;
        }


        /*public void BuildInvisibleBlockCollisionMesh()
        {
            int x = 0, y = 0, z = 0;

            // for each voxel in Voxels, check if any of the voxel's faces are exposed, and if so, add their faces to the main mesh arrays (named Vertices and Faces)
            while (x < _sizeX)
            {
                while (y < _sizeY)
                {
                    while (z < _sizeZ)
                    {
                        ushort voxel = _chunk.GetVoxel(x, y, z);     // the current voxel data
                        if (voxel != 0)     // don't render empty blocks.
                        {
                            Voxel voxelType = Engine.GetVoxelType(voxel);
                            Transparency tr = voxelType.VTransparency;
                            ColliderType ct = voxelType.VColliderType;

                            foreach (Direction d in Enum.GetValues(typeof(Direction)))
                            {
                                if (CheckAdjacent(x, y, z, d, tr))
                                    CreateFace(voxel, (Facing)d, ct, x, y, z);
                            }

                            // if no collider, create a trigger cube collider
                            if (ct == ColliderType.none && Engine.GenerateColliders)
                            {
                                AddCubeMesh(x, y, z, false);
                            }
                        }
                        z++;
                    }
                    z = 0;
                    y++;
                }
                y = 0;
                x++;
            }

            // update mesh using the values from the arrays
            UpdateMesh(GetComponent<MeshFilter>().mesh);
        }*/


        // ==== mesh generation =======================================================================================

        void CreateFace(ushort voxel, Facing facing, ColliderType colliderType, int x, int y, int z)
        {
            const float H = 0.5f;
            const float H1 = 0.5001f;


            Voxel voxelComponent = Engine.GetVoxelType(voxel);
            List<int> FacesList = Faces[voxelComponent.VSubmeshIndex];

            // ==== Vertices ====

            // add the positions of the vertices depending on the facing of the face
            if (facing == Facing.forward)
            {
                Vertices.Add(new Vector3(x + H1, y + H1, z + H));
                Vertices.Add(new Vector3(x - H1, y + H1, z + H));
                Vertices.Add(new Vector3(x - H1, y - H1, z + H));
                Vertices.Add(new Vector3(x + H1, y - H1, z + H));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x + H, y + H, z + H));
                    SolidColliderVertices.Add(new Vector3(x - H, y + H, z + H));
                    SolidColliderVertices.Add(new Vector3(x - H, y - H, z + H));
                    SolidColliderVertices.Add(new Vector3(x + H, y - H, z + H));
                }
            }
            else if (facing == Facing.up)
            {
                Vertices.Add(new Vector3(x - H1, y + H, z + H1));
                Vertices.Add(new Vector3(x + H1, y + H, z + H1));
                Vertices.Add(new Vector3(x + H1, y + H, z - H1));
                Vertices.Add(new Vector3(x - H1, y + H, z - H1));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x - H, y + H, z + H));
                    SolidColliderVertices.Add(new Vector3(x + H, y + H, z + H));
                    SolidColliderVertices.Add(new Vector3(x + H, y + H, z - H));
                    SolidColliderVertices.Add(new Vector3(x - H, y + H, z - H));
                }
            }
            else if (facing == Facing.right)
            {
                Vertices.Add(new Vector3(x + H, y + H1, z - H1));
                Vertices.Add(new Vector3(x + H, y + H1, z + H1));
                Vertices.Add(new Vector3(x + H, y - H1, z + H1));
                Vertices.Add(new Vector3(x + H, y - H1, z - H1));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x + H, y + H, z - H));
                    SolidColliderVertices.Add(new Vector3(x + H, y + H, z + H));
                    SolidColliderVertices.Add(new Vector3(x + H, y - H, z + H));
                    SolidColliderVertices.Add(new Vector3(x + H, y - H, z - H));
                }
            }
            else if (facing == Facing.back)
            {
                Vertices.Add(new Vector3(x - H1, y + H1, z - H));
                Vertices.Add(new Vector3(x + H1, y + H1, z - H));
                Vertices.Add(new Vector3(x + H1, y - H1, z - H));
                Vertices.Add(new Vector3(x - H1, y - H1, z - H));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x - H, y + H, z - H));
                    SolidColliderVertices.Add(new Vector3(x + H, y + H, z - H));
                    SolidColliderVertices.Add(new Vector3(x + H, y - H, z - H));
                    SolidColliderVertices.Add(new Vector3(x - H, y - H, z - H));
                }
            }
            else if (facing == Facing.down)
            {
                Vertices.Add(new Vector3(x - H1, y - H, z - H1));
                Vertices.Add(new Vector3(x + H1, y - H, z - H1));
                Vertices.Add(new Vector3(x + H1, y - H, z + H1));
                Vertices.Add(new Vector3(x - H1, y - H, z + H1));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x - H, y - H, z - H));
                    SolidColliderVertices.Add(new Vector3(x + H, y - H, z - H));
                    SolidColliderVertices.Add(new Vector3(x + H, y - H, z + H));
                    SolidColliderVertices.Add(new Vector3(x - H, y - H, z + H));
                }
            }
            else if (facing == Facing.left)
            {
                Vertices.Add(new Vector3(x - H, y + H1, z + H1));
                Vertices.Add(new Vector3(x - H, y + H1, z - H1));
                Vertices.Add(new Vector3(x - H, y - H1, z - H1));
                Vertices.Add(new Vector3(x - H, y - H1, z + H1));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x - H, y + H, z + H));
                    SolidColliderVertices.Add(new Vector3(x - H, y + H, z - H));
                    SolidColliderVertices.Add(new Vector3(x - H, y - H, z - H));
                    SolidColliderVertices.Add(new Vector3(x - H, y - H, z + H));
                }
            }

            // ==== UVs =====
            float tUnit = Engine.TextureUnit;
            Vector2 tOffset = Engine.GetTextureOffset(voxel, facing); // get texture offset for this voxel type

            float pad = tUnit * Engine.TexturePadding;
            UVs.Add(new Vector2(tUnit * tOffset.x + pad, tUnit * tOffset.y + tUnit - pad)); // top left
            UVs.Add(new Vector2(tUnit * tOffset.x + tUnit - pad, tUnit * tOffset.y + tUnit - pad)); // top right
            UVs.Add(new Vector2(tUnit * tOffset.x + tUnit - pad, tUnit * tOffset.y + pad)); // bottom right
            UVs.Add(new Vector2(tUnit * tOffset.x + pad, tUnit * tOffset.y + pad)); // bottom left

            // ==== Faces ====

            // add the faces
            FacesList.Add(FaceCount + 0);
            FacesList.Add(FaceCount + 1);
            FacesList.Add(FaceCount + 3);
            FacesList.Add(FaceCount + 1);
            FacesList.Add(FaceCount + 2);
            FacesList.Add(FaceCount + 3);
            if (colliderType == ColliderType.cube && Engine.GenerateColliders)
            {
                SolidColliderFaces.Add(SolidFaceCount + 0);
                SolidColliderFaces.Add(SolidFaceCount + 1);
                SolidColliderFaces.Add(SolidFaceCount + 3);
                SolidColliderFaces.Add(SolidFaceCount + 1);
                SolidColliderFaces.Add(SolidFaceCount + 2);
                SolidColliderFaces.Add(SolidFaceCount + 3);
            }

            // Add to the face count
            FaceCount += 4; // we're adding 4 because there are 4 vertices in each face.
            if (colliderType == ColliderType.cube && Engine.GenerateColliders)
            {
                SolidFaceCount += 4;
            }

            // Check the amount of vertices so far and create a new mesh if necessary
            if (Vertices.Count > 65530)
            {
                CreateNewMeshObject();
            }
        }

        void CreateCustomMesh(ushort voxel, int x, int y, int z, Mesh mesh)
        {
            Voxel voxelComponent = Engine.GetVoxelType(voxel);
            List<int> FacesList = Faces[voxelComponent.VSubmeshIndex];

            // check if mesh exists
            if (mesh == null)
            {
                Debug.LogError("Uniblocks: The voxel id " + voxel + " uses a custom mesh, but no mesh has been assigned!");
                return;
            }


            // === mesh
            // check if we still have room for more vertices in the mesh
            if (Vertices.Count + mesh.vertices.Length > 65534)
            {
                CreateNewMeshObject();
            }

            // rotate vertices depending on the mesh rotation setting
            List<Vector3> rotatedVertices = new List<Vector3>();
            MeshRotation rotation = voxelComponent.VRotation;

            // 180 horizontal (reverse all x and z)
            if (rotation == MeshRotation.back)
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    rotatedVertices.Add(new Vector3(-vertex.x, vertex.y, -vertex.z));
                }
            }

            // 90 right
            else if (rotation == MeshRotation.right)
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    rotatedVertices.Add(new Vector3(vertex.z, vertex.y, -vertex.x));
                }
            }

            // 90 left
            else if (rotation == MeshRotation.left)
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    rotatedVertices.Add(new Vector3(-vertex.z, vertex.y, vertex.x));
                }
            }

            // no rotation
            else {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    rotatedVertices.Add(vertex);
                }
            }

            // vertices
            foreach (Vector3 vertex in rotatedVertices)
            {
                Vertices.Add(vertex + new Vector3(x, y, z)); // add all vertices from the mesh
            }

            // UVs
            foreach (Vector2 uv in mesh.uv)
            {
                UVs.Add(uv);
            }

            // faces
            foreach (int face in mesh.triangles)
            {
                FacesList.Add(FaceCount + face);
            }

            // Add to the face count
            FaceCount += mesh.vertexCount;


            // === collider
            if (Engine.GenerateColliders)
            {
                ColliderType colliderType = Engine.GetVoxelType(voxel).VColliderType;  // get collider type (solid/cube/none)

                // mesh collider
                if (colliderType == ColliderType.mesh)
                {
                    foreach (Vector3 vertex1 in rotatedVertices)
                    {
                        SolidColliderVertices.Add(vertex1 + new Vector3(x, y, z)); // if mesh collider, just add the vertices & faces from this mesh to the solid collider mesh
                    }
                    foreach (int face1 in mesh.triangles)
                    {
                        SolidColliderFaces.Add(SolidFaceCount + face1);
                    }
                    SolidFaceCount += mesh.vertexCount;
                }

                // cube collider
                if (colliderType == ColliderType.cube)
                {
                    AddCubeMesh(x, y, z, true); // if cube collider, add a cube to the solid mesh
                }
                // nocollide collider (for both ColliderType.mesh and ColliderType.none, but not for ColliderType.cube since it's redundant)
                else if (voxel != 0)
                { // only make a collider for non-empty voxels
                    AddCubeMesh(x, y, z, false); // if no cube collider, add a cube to the nocollide mesh (we still need a collider on noCollide blocks for raycasts and such)
                }
            }
        }

        void AddCubeMesh(int x, int y, int z, bool solid)
        { 
            // adds cube verts and faces to the chosen lists (for Solid or NoCollide colliders)
            if (solid)
            {
                // vertices
                foreach (Vector3 vertex in Cube.vertices)
                {
                    SolidColliderVertices.Add(vertex + new Vector3(x, y, z)); // add all vertices from the mesh
                }

                // faces
                foreach (int face in Cube.triangles)
                {
                    SolidColliderFaces.Add(SolidFaceCount + face);
                }

                // Add to the face count
                SolidFaceCount += Cube.vertexCount;
            }
            else
            {
                // vertices
                foreach (Vector3 vertex1 in Cube.vertices)
                {
                    NoCollideVertices.Add(vertex1 + new Vector3(x, y, z));
                }

                // faces
                foreach (int face1 in Cube.triangles)
                {
                    NoCollideFaces.Add(NoCollideFaceCount + face1);
                }

                // Add to the face count
                NoCollideFaceCount += Cube.vertexCount;
            }
        }

        void UpdateMesh(Mesh mesh)
        {
            // Update the mesh
            mesh.Clear();
            mesh.vertices = Vertices.ToArray();
            mesh.subMeshCount = GetComponent<Renderer>().materials.Length;

            for (int i = 0; i < Faces.Count; ++i)
            {
                mesh.SetTriangles(Faces[i], i);     // TODO: The "Dynamic batching: index buffer source is NULL..." errors are related to this line
            }

            mesh.uv = UVs.ToArray();//UVs.ToBuiltin(Vector2) as Vector2[]
            ;
            mesh.RecalculateNormals();

            if (Engine.GenerateColliders)
            {
                // Update solid collider
                Mesh colMesh = new Mesh();

                colMesh.vertices = SolidColliderVertices.ToArray();
                colMesh.triangles = SolidColliderFaces.ToArray();
                ;
                colMesh.RecalculateNormals();

                GetComponent<MeshCollider>().sharedMesh = null;
                GetComponent<MeshCollider>().sharedMesh = colMesh;

                // Update nocollide collider
                if (NoCollideVertices.Count > 0)
                {
                    // make mesh
                    Mesh nocolMesh = new Mesh();
                    nocolMesh.vertices = NoCollideVertices.ToArray();
                    nocolMesh.triangles = NoCollideFaces.ToArray();
                    ;
                    nocolMesh.RecalculateNormals();

                    _noCollideCollider = Instantiate(_chunk.chunkCollider, transform.position, transform.rotation) as GameObject;
                    _noCollideCollider.transform.parent = this.transform;
                    _noCollideCollider.GetComponent<MeshCollider>().sharedMesh = nocolMesh;
                }
                else if (_noCollideCollider != null)
                {
                    Destroy(_noCollideCollider); // destroy the existing collider if there is no NoCollide vertices
                }
            }


            // clear the main arrays for future use.
            Vertices.Clear();
            UVs.Clear();
            foreach (List<int> faceList in Faces)
            {
                faceList.Clear();
            }

            SolidColliderVertices.Clear();
            SolidColliderFaces.Clear();

            NoCollideVertices.Clear();
            NoCollideFaces.Clear();


            FaceCount = 0;
            SolidFaceCount = 0;
            NoCollideFaceCount = 0;
        }


        void CreateNewMeshObject()
        { 
            // in case the amount of vertices exceeds the maximum for one mesh, we need to create a new mesh
            GameObject meshContainer = Instantiate(_chunk.meshContainer, transform.position, transform.rotation) as GameObject;
            meshContainer.transform.parent = this.transform;

            UpdateMesh(meshContainer.GetComponent<MeshFilter>().mesh);
        }
    }
}
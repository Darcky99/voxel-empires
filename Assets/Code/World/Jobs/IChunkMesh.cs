using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelEmpires.VoxelUtilities;
using VoxelEmpires.Configuration;

namespace VoxelEmpires.World
{
    [BurstCompile]
    public struct IChunkMesh : IJob
    {
        public IChunkMesh(NativeGrid<byte> buildingChunk)
        {
            _voxelsConfig = new NativeArray<VoxelConfig>(GameConfig.Instance.VoxelConfiguration.GetVoxelsData(), Allocator.Persistent);
            _worldHeight = GameConfig.Instance.WorldConfiguration.WorldHeight;

            Vertices = new NativeList<Vector3>(Allocator.Persistent);
            Triangles = new NativeList<int>(Allocator.Persistent);
            UVs = new NativeList<Vector3>(Allocator.Persistent);

            _drawnFaces = new NativeHashMap<int3, FacesDrawn>(6000, Allocator.Persistent);

            _buildingChunk = new NativeGrid<byte>(buildingChunk, Allocator.Persistent);

            d = new NativeArray<int>(3, Allocator.Persistent);
            l = new NativeArray<int>(3, Allocator.Persistent);
            v = new NativeArray<int>(3, Allocator.Persistent);
        }

        public NativeList<Vector3> Vertices { get; private set; }
        public NativeList<int> Triangles { get; private set; }
        public NativeList<Vector3> UVs { get; private set; }

        private readonly NativeGrid<byte> _buildingChunk;
        private readonly NativeHashMap<int3, FacesDrawn> _drawnFaces;
        private readonly int _worldHeight;

        private NativeArray<VoxelConfig> _voxelsConfig;
        private NativeArray<int> d;
        private NativeArray<int> l;
        private NativeArray<int> v;

        public void Execute()
        {
            int3 real_chunk_size = new int3(_buildingChunk.Lenght.x - 2, 1, _buildingChunk.Lenght.x - 2);
            if (_buildingChunk.NativeArray.Length == 1)
            {
                return;
            }
            int a, b;
            l[0] = real_chunk_size.x;
            l[1] = _worldHeight;
            l[2] = real_chunk_size.z;
            for (int p = 0; p <= 2; p++)
            {
                a = (p + 1) % 3;
                b = (p + 2) % 3;
                for (d[p] = 0; d[p] < l[p]; d[p]++)
                {
                    for (d[a] = 0; d[a] < l[a]; d[a]++)
                    {
                        for (d[b] = 0; d[b] < l[b]; d[b]++)
                        {
                            d[1] = GetHeightMapValue(d[0], d[2]);
                            int3 pab = new int3(p, a, b);
                            RunDownwards(pab);
                            d[1] = _worldHeight + 1;
                        }
                    }
                }
            }
        }

        private void RunDownwards(int3 pab)
        {
            int max_height = d[1];
            int min_height = FindMinimumDrawHeight(pab, max_height);
            for (int currentHeight = min_height; currentHeight <= max_height; currentHeight++)
            {
                d[1] = currentHeight;
                int3 position = new int3(d[0], d[1], d[2]);
                byte voxelID = GetVoxelIDByHeight(max_height, currentHeight);
                DrawGreedyQuad(position, pab, voxelID, GetFaceIndex(pab.x, -1));
                DrawGreedyQuad(position, pab, voxelID, GetFaceIndex(pab.x, 1));
            }
        }
        private void DrawGreedyQuad(int3 position, int3 pab, byte voxelID, int faceIndex)
        {
            int p = pab.x, a = pab.y, b = pab.z;
            if (!IsDrawFace(voxelID, position, faceIndex) || (position.y == 0 && faceIndex == 1))
            {
                return;
            }
            v[p] = d[p];
            v[a] = d[a];
            v[b] = d[b];
            int3 min_limit = new int3(v[0], v[1], v[2]);
            int2 a_limits = new int2(d[a], d[a]);
            for (int al = v[a] + 1; al < l[a]; al++)
            {
                v[a] = al;
                if (IsDrawFace(voxelID, new int3(v[0], v[1], v[2]), faceIndex))
                {
                    a_limits.y = al;
                }
                else break;
            }
            int2 b_limits = new int2(d[b], d[b]);
            v[a] = a_limits.y;
            for (int bl = d[b] + 1; bl < l[b]; bl++)
            {
                v[b] = bl;
                int3 greater_b = new int3(v[0], v[1], v[2]);
                if (CanDrawFaceInArea(voxelID, min_limit, greater_b, faceIndex))
                {
                    b_limits.y = bl;
                }
                else break;
            }
            v[a] = a_limits.y;
            v[b] = b_limits.y;
            int3 max_limit = new int3(v[0], v[1], v[2]);

            float3 meshCenter = GetMeshCenter(min_limit, max_limit);
            float2 meshSize = new float2(a_limits.y - a_limits.x + 1, b_limits.y - b_limits.x + 1);

            int vertexIndex = Vertices.Length;
            NativeArray<float3> fv = Voxels.GetFaceVertices(faceIndex);
            foreach (float3 vertex in fv)
            {
                float3 global_vertex = vertex;
                global_vertex[a] *= meshSize.x;
                global_vertex[b] *= meshSize.y;
                global_vertex += meshCenter;
                Vertices.Add(global_vertex);
            }
            Triangles.AddRange(new NativeArray<int>(Voxels.GetFaceTriangles(vertexIndex), Allocator.Temp));
            NativeArray<Vector3> uvs = Voxels.GetUVs();
            foreach (Vector3 uv in uvs)
            {
                Vector3 u = uv;
                int index_a = a == 2 ? a - 1 - b : a;
                int index_b = b == 2 ? b - 1 - a : b;
                u[index_a] *= meshSize.x;
                u[index_b] *= meshSize.y;
                u.z = _voxelsConfig[voxelID/*  - 1 */].TextureIndex(faceIndex);
                UVs.Add(u);
            }
            SetAsDrawn(min_limit, max_limit, faceIndex);
        }

        private int GetFaceIndex(int p, int i)
        {
            switch (p, i)
            {
                case (1, 1): return 0;
                case (1, -1): return 1;
                case (0, 1): return 2;
                case (0, -1): return 3;
                case (2, 1): return 4;
                case (2, -1): return 5;
                default: return -1;
            }
        }

        private int FindMinimumDrawHeight(int3 pab, int naturalHeight)
        {
            int minimum_drawn_height = naturalHeight;
            for (int currentHeight = d[1]; currentHeight >= 0; currentHeight--)
            {
                int3 position = new int3(d[0], currentHeight, d[2]);
                if (IsDirectionEmpty(position, GetFaceIndex(pab.x, -1)) || IsDirectionEmpty(position, GetFaceIndex(pab.x, 1)))
                {
                    minimum_drawn_height = currentHeight;
                }
                else
                {
                    break;
                }
            }
            return minimum_drawn_height;
        }
        private byte GetVoxelIDByHeight(int nh, int h)
        {
            return VoxelEmpires.World.Terrain.GetVoxelIDByHeight(nh, h);
        }
        private byte GetHeightMapValue(int x, int z)
        {
            byte h = _buildingChunk.GetValue(x + 1, 0, z + 1);
            return h;
        }

        private void SetAsDrawn(int3 xyz, int faceIndex)
        {
            bool keyExists = _drawnFaces.ContainsKey(xyz);

            FacesDrawn faces = keyExists ? _drawnFaces[xyz] : new FacesDrawn();
            faces.SetByIndex(faceIndex);
            if (keyExists)
                _drawnFaces.Remove(xyz);
            _drawnFaces.Add(xyz, faces);
        }
        private void SetAsDrawn(int3 min, int3 max, int faceIndex)
        {
            int3 position = min;
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                    for (int x = min.x; x <= max.x; x++)
                    {
                        position.x = x;
                        position.y = y;
                        position.z = z;
                        SetAsDrawn(position, faceIndex);
                    }
        }

        private bool IsFaceDrawn(int3 xyz, int faceIndex)
        {
            bool isInDictionary = _drawnFaces.ContainsKey(xyz);
            return !isInDictionary ? false : _drawnFaces[xyz].IsFaceDrawn(faceIndex);
        }

        private bool IsDirectionEmpty(int3 xyz, int faceIndex)
        {
            int3 direction = xyz + Voxels.GetDirection(faceIndex);
            byte directionNaturalHeight = GetHeightMapValue(direction.x, direction.z);
            return direction.y > directionNaturalHeight;
        }
        private bool IsDrawFace(int3 xyz, int faceIndex)
        {
            byte originNaturalHeight = GetHeightMapValue(xyz.x, xyz.z);
            if (xyz.y > originNaturalHeight)
            {
                return false;
            }
            bool isDirectionEmpty = IsDirectionEmpty(xyz, faceIndex);
            bool isFaceNotDrawn = !IsFaceDrawn(xyz, faceIndex);
            return isDirectionEmpty && isFaceNotDrawn;
        }
        private bool IsDrawFace(byte voxelID, int3 xyz, int faceIndex)
        {
            int natural_height = GetHeightMapValue(xyz.x, xyz.z);
            byte originID = GetVoxelIDByHeight(natural_height, xyz.y);
            bool IsSameType = originID == voxelID;
            return IsSameType && IsDrawFace(xyz, faceIndex);
        }

        private bool CanDrawFaceInArea(byte voxelID, int3 min, int3 max, int faceIndex)
        {
            if (!(min.x == max.x || min.y == max.y || min.z == max.z))
            {
                Debug.LogError("At least one axis needs to be flat");
            }
            int3 position = min;
            for (int y = min.y; y <= max.y; y++)
            {
                for (int z = min.z; z <= max.z; z++)
                {
                    for (int x = min.x; x <= max.x; x++)
                    {
                        position.x = x;
                        position.y = y;
                        position.z = z;
                        if (!IsDrawFace(voxelID, position, faceIndex))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private float3 GetMeshCenter(int3 min, int3 max)
        {
            int3 one = new int3(1, 1, 1);
            min -= one;
            max -= one;
            float x_dif = max.x - min.x;
            float y_dif = max.y - min.y;
            float z_dif = max.z - min.z;
            float x_center = min.x + (x_dif / 2f);
            float y_center = min.y + (y_dif / 2f);
            float z_center = min.z + (z_dif / 2f);
            return new float3(x_center * 0.5f, y_center * 0.5f, z_center * 0.5f);
        }

        public void TempJobDispose()
        {
            d.Dispose();
            l.Dispose();
            v.Dispose();
            _buildingChunk.Dispose();
            _drawnFaces.Dispose();
            _voxelsConfig.Dispose();
        }
        public void Dispose()
        {
            Vertices.Dispose();
            Triangles.Dispose();
            UVs.Dispose();
        }
    }
}
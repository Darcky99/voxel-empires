using Unity.Mathematics;
using UnityEngine;

namespace VoxelUtils
{
    public struct VoxelMap
    {
        public byte[] FlatMap => m_FlatMap;

        private byte[] m_FlatMap;

        public void SetFlatMap(byte[] flatMap) => m_FlatMap = flatMap;

        public byte GetVoxel(Vector3Int xyz) => GetVoxel(xyz.x, xyz.y, xyz.z);
        public byte GetVoxel(int x, int y, int z)
        {
            if (x < 0 || x >= Voxels.s_ChunkSize || y < 0 || y >= Voxels.s_ChunkHeight || z < 0 || z >= Voxels.s_ChunkSize)
                return 0;

            return m_FlatMap[Voxels.Index(x, y, z)];
        }

        public void SetVoxel(int x, int y, int z, byte b) => m_FlatMap[Voxels.Index(x, y, z)] = b;
    }
}
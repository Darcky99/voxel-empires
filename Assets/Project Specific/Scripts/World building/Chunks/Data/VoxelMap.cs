using Unity.Mathematics;
using UnityEngine;

namespace VoxelUtils
{
    public struct VoxelMap
    {
        public VoxelMap(int chunkSize)
        {
            m_ChunkSize = chunkSize;
            m_FlatMap = new byte[]{ 0 };
        }


        public byte[] FlatMap => m_FlatMap;

        private int m_ChunkSize;
        private byte[] m_FlatMap;

        public void SetFlatMap(byte[] flatMap) => m_FlatMap = flatMap;

        public byte GetVoxel(Vector3Int xyz) => GetVoxel(xyz.x, xyz.y, xyz.z);
        public byte GetVoxel(int x, int y, int z)
        {
            if (x < 0 || x >= m_ChunkSize || y < 0 || y >= m_ChunkSize || z < 0 || z >= m_ChunkSize)
                return 0;

            return m_FlatMap[Voxels.Index(x, y, z)];
        }

        public void SetVoxel(int x, int y, int z, byte b) => m_FlatMap[Voxels.Index(x, y, z)] = b;
    }
}
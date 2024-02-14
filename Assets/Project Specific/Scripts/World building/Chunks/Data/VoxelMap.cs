using Unity.Mathematics;
using UnityEngine;

namespace VoxelUtils
{
    public struct VoxelMap
    {
        public VoxelMap(int chunkSize)
        {
            m_ChunkSize = chunkSize;
            m_FlatMap = new byte[m_ChunkSize * m_ChunkSize * m_ChunkSize];
        }


        public byte[] Expanded_FlatMap
        {
            get
            {
                bool isEmpty = true;
                int flatmapSize = (int)math.pow(m_ChunkSize + 2, 3);

                byte[] flatMap = new byte[flatmapSize];
                for (int y = 0; y < m_ChunkSize; y++)
                    for (int z = 0; z < m_ChunkSize; z++)
                        for (int x = 0; x < m_ChunkSize; x++)
                        {
                            byte voxel = m_FlatMap[Voxels.Index(x, y, z)];
                            if (voxel != 0)
                                isEmpty = false;
                            flatMap[Voxels.Expanted_Index(x + 1, y + 1, z + 1)] = m_FlatMap[Voxels.Index(x, y, z)];
                        }
                if (isEmpty)
                    return new byte[1] { 0 };
                //I might need suport for chunks 1 value but != 0

                return flatMap;
            }
        }
        public byte[] FlatMap => m_FlatMap;

        private int m_ChunkSize;
        private byte[] m_FlatMap;

        public void SetFlatMap(byte[] flatMap) {
            m_FlatMap = flatMap;
        }

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
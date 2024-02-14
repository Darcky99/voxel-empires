using Unity.Mathematics;
using UnityEngine;

namespace VoxelUtils
{
    public struct VoxelMap
    {
        private int ChunkSize => GameConfig.Instance.ChunkConfiguration.ChunkSize;

        public byte[] TermporalAccess => m_FlatMap;

        public byte[] Expanded_FlatMap
        {
            get
            {
                bool isEmpty = true;
                int chunkSize = ChunkSize;
                int flatmapSize = (int)math.pow(chunkSize + 2, 3);

                byte[] flatMap = new byte[flatmapSize];
                for (int y = 0; y < chunkSize; y++)
                    for (int z = 0; z < chunkSize; z++)
                        for (int x = 0; x < chunkSize; x++)
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

        private byte[] m_FlatMap;

        public void SetFlatMap(byte[] flatMap) {
            m_FlatMap = flatMap;
            
        }

        public byte GetVoxel(Vector3Int xyz) => GetVoxel(xyz.x, xyz.y, xyz.z);
        public byte GetVoxel(int x, int y, int z)
        {
            if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize || z < 0 || z >= ChunkSize)
                return 0;

            return m_FlatMap[Voxels.Index(x, y, z)];
        }

        public void SetVoxel(int x, int y, int z, byte b) => m_FlatMap[Voxels.Index(x, y, z)] = b;
    }
}
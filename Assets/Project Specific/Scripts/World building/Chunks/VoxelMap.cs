using Unity.Mathematics;
using UnityEngine;

namespace VoxelUtils
{
    public class VoxelMap
    {
        public VoxelMap(byte chunkSize)
        {
            m_ChunkSize = chunkSize;
        }

        public byte[] Expanded_FlatMap
        {
            get
            {
                bool isEmpty = true;

                int chunkSide = m_ChunkSize;
                int flatmapSize = (int)math.pow(chunkSide + 2, 3);

                byte[] flatMap = new byte[flatmapSize];

                for (int y = 0; y < chunkSide; y++)
                    for (int z = 0; z < chunkSide; z++)
                        for (int x = 0; x < chunkSide; x++)
                        {
                            byte voxel = m_FlatMap[Voxels.Index(x, y, z)];

                            if (voxel != 0)
                                isEmpty = false;

                            //flatMap[Voxels.Index(x, y, z)] = voxel;
                            flatMap[Voxels.Expanted_Index(x + 1, y + 1, z + 1)] = m_FlatMap[Voxels.Index(x, y, z)];
                        }

                if (isEmpty)
                    return new byte[1] { 0 };
                //I might need suport for chunks 1 value but != 0

                return flatMap;
            }
        }

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
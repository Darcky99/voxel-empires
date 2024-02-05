using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class VoxelMap /*: IGetVoxel*/
{
    public VoxelMap(byte chunkSize, byte voxel)
    {
        m_ChunkSize = chunkSize;
    }

    private int m_ChunkSize;

    private byte[] m_FlatMap;
    public byte[] FlatMap
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
                        byte voxel = m_FlatMap[Voxels.IndexnoExpanded(x, y, z)];

                        if (voxel != 0)
                            isEmpty = false;

                        //flatMap[Voxels.Index(x, y, z)] = voxel;
                        flatMap[Voxels.Index(x + 1, y + 1, z + 1)] = m_FlatMap[Voxels.IndexnoExpanded(x,y,z)];
                    }

            if (isEmpty)
                return new byte[1] { 0 };
            //I might need suport for chunks 1 value but != 0

            return flatMap;
        }
    }

    public async Task SetFlatMap(NativeArray<byte> flatVoxelMap, bool waitFlag)
    {
        m_FlatMap = flatVoxelMap.ToArray();

        flatVoxelMap.Dispose();
    }

    public byte GetVoxel(int3 xyz) => GetVoxel(xyz.x, xyz.y, xyz.z);
    public byte GetVoxel(int x, int y, int z)
    {
        if(x <  0 || x >= m_ChunkSize || y < 0 || y >= m_ChunkSize || z < 0 || z >= m_ChunkSize)
            return 0;

        return m_FlatMap[Voxels.IndexnoExpanded(x, y, z)];
    }

    public void SetVoxel(int x, int y, int z, byte b)
    {
        m_FlatMap[Voxels.IndexnoExpanded(x, y, z)] = b;
    }
}
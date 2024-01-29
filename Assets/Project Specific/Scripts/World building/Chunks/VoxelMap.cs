using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct VoxelMap : IGetVoxel
{
    public VoxelMap(byte chunkSize)
    {
        m_Layers = new IGetVoxel[chunkSize];

        for (byte i = 0; i < m_Layers.Length; i++)
            m_Layers[i] = new SingleVoxel(0);

    }

    private IGetVoxel[] m_Layers;
    
    public byte[] FlatMap
    {
        get
        {
            //byte value = GetVoxel(1, 1, 1);
            bool isEmpty = true;

            int chunkSide = (byte)m_Layers.Length;
            int flatmapSize = (int)math.pow(chunkSide + 2, 3);

            byte[] flatMap = new byte[flatmapSize];

            for (int y = 1; y <= chunkSide; y++)
                for (int z = 1; z <= chunkSide; z++)
                    for (int x = 1; x <= chunkSide; x++)
                    {
                        byte voxel = GetVoxel(x - 1, y - 1, z - 1);

                        if(voxel != 0)
                            isEmpty = false;

                        flatMap[Voxels.Index(x, y, z)] = voxel;
                    }

            if (isEmpty)
                return new byte[1] { 0 };

            return flatMap;
        }
    }
    public void SetFlatMap(NativeArray<byte> flatVoxelMap)
    {
        int chunkSize = ChunkConfiguration.ChunkSize;

        for (int y = 0; y < chunkSize; y++)
            for (int z = 0; z < chunkSize; z++)
                for (int x = 0; x < chunkSize; x++)
                    SetVoxel(x, y, z, flatVoxelMap[Voxels.Index(x + 1, y + 1, z + 1)]);

        flatVoxelMap.Dispose();
    }


    public byte GetVoxel(int x, int y, int z)
    {
        if(x <  0 || x >= m_Layers.Length || y < 0 || y >= m_Layers.Length || z < 0 || z >= m_Layers.Length)
            return 0;

        return m_Layers[y].GetVoxel(x, z);
    }
    public byte GetVoxel(int3 xyz) => GetVoxel(xyz.x, xyz.y, xyz.z);
    public void SetVoxel(int x, int y, int z, byte b)
    {
        byte layerValue = m_Layers[y].GetVoxel(x, z);

        if (m_Layers[y] is SingleVoxel && layerValue != b)
        {
            m_Layers[y] = new VoxelLayer(layerValue);
            m_Layers[y].SetVoxel(x,z,b);
        }
        m_Layers[y].SetVoxel(x, z, b);

        if(m_Layers[y] is VoxelLayer && ((VoxelLayer)m_Layers[y]).IsOneValue())
            m_Layers[y] = new SingleVoxel(b);

        //still, I need to check if all of the layers all the same and if so, 
    }

    public byte GetVoxel(int x, int z) => throw new System.NotImplementedException();
    public void SetVoxel(int x, int z, byte b) => throw new System.NotImplementedException();
}
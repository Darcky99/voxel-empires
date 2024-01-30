using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelLayers : IGetVoxel
{
    public VoxelLayers(byte b)
    {
        m_Layers = new IGetVoxel[ChunkConfiguration.ChunkSize];

        for (byte i = 0; i < m_Layers.Length; i++)
            m_Layers[i] = new SingleVoxel(b);
    }

    private IGetVoxel[] m_Layers;

    public bool IsOneValue()
    {
        if (m_Layers[0] is not SingleVoxel)
            return false;

        byte value = m_Layers[0].GetVoxel(0,0);

        for (int i = 1; i < m_Layers.Length; i++)
        {
            if(!(m_Layers[i] is SingleVoxel && value == m_Layers[i].GetVoxel(0, 0)))
                return false;
        }

        //si el layer no es singlvoxel, y el valor es igual a el primero, regresa no es un solo valor???
        //for (int i = 1; i < m_Layers.Length; i++)
        //    if (m_Layers[i] is not SingleVoxel && value == m_Layers[i].GetVoxel(0, 0))
        //        return false;


        return true;
    }

    public byte GetVoxel(int x, int y, int z)
    {
        if (x < 0 || x >= m_Layers.Length || y < 0 || y >= m_Layers.Length || z < 0 || z >= m_Layers.Length)
            return 0;

        return m_Layers[y].GetVoxel(x, z);
    }
    public void SetVoxel(int x, int y, int z, byte b)
    {
        byte layerValue = m_Layers[y].GetVoxel(x, z);

        if (m_Layers[y] is SingleVoxel && layerValue != b)
        {
            m_Layers[y] = new VoxelLayer(layerValue);
            m_Layers[y].SetVoxel(x, z, b);

            //if (m_Layers[y].GetVoxel(x, z) == 0)
            //    Debug.Log("Something's wrong");

            return;
        }
        
        m_Layers[y].SetVoxel(x, z, b);

        //if (m_Layers[y].GetVoxel(x, z) == 0)
        //    Debug.Log("Something's wrong");

        if (m_Layers[y] is VoxelLayer && ((VoxelLayer)m_Layers[y]).IsOneValue())
        {
            m_Layers[y] = new SingleVoxel(b);
        }


    }

    public byte GetVoxel(int x, int z)
    {
        Debug.LogError("Can't return a value without 'y'");
        return 0;
    }
    public void SetVoxel(int x, int z, byte b) => Debug.LogError("Can't set a value without 'yyy'");
}
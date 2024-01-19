using Unity.Collections;

public struct VoxelLayer : IGetVoxel
{
    public VoxelLayer(byte b)
    {
        m_ChunkSize = ChunkConfiguration.ChunkSize;
        int layerSize = m_ChunkSize * m_ChunkSize;

        m_Layer = new byte[layerSize];

        for (int i = 0; i < layerSize; i++)
            m_Layer[i] = b;
    }

    public byte[] Layer { get { return m_Layer; } }

    private byte[] m_Layer; //ID: 0 to 255
    private readonly int m_ChunkSize;

    private int Index(int x, int z) => x + (z * m_ChunkSize);

    public bool IsOneValue()
    {
        byte a = Layer[0];
        for (int i = 1; i < Layer.Length; i++)
            if(m_Layer[i] != a)
                return false;
        return true;
    }

    public byte GetVoxel(int x, int z) => m_Layer[Index(x, z)];
    public void SetVoxel(int x, int z, byte b) => m_Layer[Index(x, z)] = b;
}
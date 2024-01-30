public class SingleVoxel : IGetVoxel
{
    public SingleVoxel(byte b)
    {
        m_Value = b;
    }

    private byte m_Value;

    public byte GetVoxel(int x, int z) => m_Value;
    public void SetVoxel(int x, int z, byte b) => m_Value = b;

    public byte GetVoxel(int x, int y, int z) => m_Value;
    public void SetVoxel(int x, int y, int z, byte b) => m_Value = b;
}
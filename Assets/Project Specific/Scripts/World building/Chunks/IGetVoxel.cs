using System.Threading.Tasks;

public interface IGetVoxel
{
    public byte GetVoxel(int x, int z);
    public void SetVoxel(int x, int z, byte b);

    public byte GetVoxel(int x, int y, int z);
    public void SetVoxel(int x, int y, int z, byte b);
}
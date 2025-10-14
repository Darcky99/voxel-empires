using Unity.Mathematics;
using VE.VoxelUtilities;

namespace VE.World
{
    public struct Chunk
    {
        public Chunk(int2 ID)
        {
            _ChunkID = ID;
            _HeightMap = default;
            _ChunkState = eChunkState.Active;
        }

        public float2 WorldPosition => _ChunkID * ChunkConfiguration.KeyToWorld;
        public int2 ChunkID => _ChunkID;
        public eChunkState ChunkState => _ChunkState;
        public NativeGrid<byte> HeightMap => _HeightMap;

        private int2 _ChunkID;
        private NativeGrid<byte> _HeightMap;
        private eChunkState _ChunkState;

        public byte GetVoxel(int3 voxelPosition) => _HeightMap.GetValue(voxelPosition);
        public void SetVoxel(int3 voxelPosition) => _HeightMap.SetValue(voxelPosition.x, voxelPosition.y, voxelPosition.z, 1);

        public void SetHeightMap(NativeGrid<byte> heightMap) => _HeightMap = heightMap;
    }
}
using Unity.Mathematics;
using VE.VoxelUtilities;

namespace VE.World
{
    public struct Chunk
    {
        public Chunk(int2 ID)
        {
            _chunkID = ID;
            _heightMap = default;
            _chunkState = eChunkState.Active;
        }

        public float2 WorldPosition => _chunkID * ChunkConfiguration.KeyToWorld;
        public int2 ChunkID => _chunkID;
        public eChunkState ChunkState => _chunkState;
        public NativeGrid<byte> HeightMap => _heightMap;

        private int2 _chunkID;
        private NativeGrid<byte> _heightMap;
        private eChunkState _chunkState;

        public byte GetVoxel(int3 voxelPosition) => _heightMap.GetValue(voxelPosition);
        public void SetVoxel(int3 voxelPosition) => _heightMap.SetValue(voxelPosition.x, voxelPosition.y, voxelPosition.z, 1);

        public void SetHeightMap(NativeGrid<byte> heightMap) => _heightMap = heightMap;
    }
}
using UnityEngine;
using VoxelUtils;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;
using System;
using UnityEngine.Profiling;
using System.Threading.Tasks;
using System.Collections;
using Unity.Mathematics;
using Unity.Collections;

namespace World
{
    public struct Chunk
    {
        public Chunk(int2 ID)
        {
            _ChunkID = ID;
            _VoxelMap = new VoxelMap();
            _ChunkState = eChunkState.Active;
        }

        public float2 WorldPosition => _ChunkID * ChunkConfiguration.KeyToWorld;
        public int2 ChunkID => _ChunkID;
        public eChunkState ChunkState => _ChunkState;
        public VoxelMap VoxelMap => _VoxelMap;

        private int2 _ChunkID;
        private VoxelMap _VoxelMap;
        private eChunkState _ChunkState;

        public byte GetVoxel(Vector3Int voxelPosition) => _VoxelMap.GetVoxel(voxelPosition);
        public void SetVoxel(Vector3Int voxelPosition) => _VoxelMap.SetVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, 1);

        public void SetVoxelMap(NativeArray<byte> flatVoxelMap) => _VoxelMap.SetFlatMap(flatVoxelMap);
    }
}
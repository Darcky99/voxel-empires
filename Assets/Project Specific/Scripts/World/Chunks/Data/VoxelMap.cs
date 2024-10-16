using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelUtils
{
    public struct VoxelMap
    {
        public NativeArray<byte> FlatMap => _flatMap;

        private NativeArray<byte> _flatMap;

        public void SetFlatMap(NativeArray<byte> flatMap)
        {
            if (_flatMap != null)
            {
                _flatMap.Dispose();
            }
            _flatMap = flatMap;
        }

        public byte GetVoxel(Vector3Int xyz) => GetVoxel(xyz.x, xyz.y, xyz.z);
        public byte GetVoxel(int x, int y, int z)
        {
            if (x < 0 || x >= Voxels.s_ChunkSize || y < 0 || y >= Voxels.s_ChunkHeight || z < 0 || z >= Voxels.s_ChunkSize)
            {
                return 0;
            }
            return _flatMap[Voxels.Index(x, y, z)];
        }

        public void SetVoxel(int x, int y, int z, byte b) => _flatMap[Voxels.Index(x, y, z)] = b;
    }
}
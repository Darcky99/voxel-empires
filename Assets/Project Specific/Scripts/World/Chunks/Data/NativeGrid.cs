using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelUtilities
{
    public struct NativeGrid
    {
        public NativeGrid(int size, int height)
        {
            _NativeArray = default;
            _GridSize = size;
            _GridHeight = height;
        }

        public NativeArray<byte> NativeArray => _NativeArray;

        private NativeArray<byte> _NativeArray;

        private readonly int _GridSize;
        private readonly int _GridHeight;

        public void SetNativeArray(NativeArray<byte> nativeArray)
        {
            if (_NativeArray != null)
            {
                _NativeArray.Dispose();
            }
            _NativeArray = nativeArray;
        }

        private int Index(int x, int y, int z)
        {
            return x + (z * _GridSize) + (y * _GridSize * _GridSize);
        }
        private bool IsInsideBounds(int x, int y, int z)
        {
            bool inBounds = !(x < 0 || x >= _GridSize || y < 0 || y >= _GridHeight || z < 0 || z >= _GridSize);
            if (!inBounds)
            {
                Debug.LogError($"Index out of bounds: {x}, {y}, {z}");
            }
            return inBounds;
        }

        public byte GetValue(int x, int y, int z)
        {
            IsInsideBounds(x, y, z);
            return _NativeArray[Index(x, y, z)];
        }
        public void SetValue(int x, int y, int z, byte value)
        {
            IsInsideBounds(x, y, z);
            _NativeArray[Index(x, y, z)] = value;
        }


        public byte GetValue(int3 xyz) => GetValue(xyz.x, xyz.y, xyz.z);
        public void SetValue(int3 xyz, byte value) => SetValue(xyz.x, xyz.y, xyz.z, value);
    }
}
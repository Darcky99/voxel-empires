using Unity.Collections;
using Unity.Mathematics;

namespace VE.VoxelUtilities
{
    public struct NativeGrid<T> where T : struct
    {
        public NativeGrid(int3 lenght, Allocator allocatorType)
        {
            Lenght = lenght;
            _NativeArray = new NativeArray<T>(lenght.x * lenght.y * lenght.z, allocatorType);
        }
        public NativeGrid(NativeGrid<T> nativeGrid, Allocator allocatorType)
        {
            Lenght = nativeGrid.Lenght;
            _NativeArray = new NativeArray<T>(nativeGrid.NativeArray, allocatorType);
        }

        public NativeArray<T> NativeArray => _NativeArray;
        public int ArraySize => Lenght.x * Lenght.y * Lenght.z;

        public readonly int3 Lenght;

        private NativeArray<T> _NativeArray;

        public void SetNativeArray(NativeArray<T> nativeArray)
        {
            if (_NativeArray != null)
            {
                _NativeArray.Dispose();
            }
            _NativeArray = nativeArray;
        }

        public int Index(int x, int z)
        {
            return x + (z * Lenght.x);
        }
        public int Index(int x, int y, int z)
        {
            return Index(x, z) + (y * Lenght.x * Lenght.z);
        }

        private (int x, int y, int z) InfiniteBounds(int x, int y, int z)
        {
            x = x < 0 ? Lenght.x - 1 : x >= Lenght.x ? 0 : x;
            y = y < 0 ? Lenght.y - 1 : y >= Lenght.y ? 0 : y;
            z = z < 0 ? Lenght.z - 1 : z >= Lenght.z ? 0 : z;
            return (x, y, z);
        }

        public T GetValue(int x, int y, int z)
        {
            (x, y, z) = InfiniteBounds(x, y, z);
            return _NativeArray[Index(x, y, z)];
        }
        public void SetValue(int x, int y, int z, T value)
        {
            (x, y, z) = InfiniteBounds(x, y, z);
            _NativeArray[Index(x, y, z)] = value;
        }

        public T GetValue(int3 xyz) => GetValue(xyz.x, xyz.y, xyz.z);
        public void SetValue(int3 xyz, T value) => SetValue(xyz.x, xyz.y, xyz.z, value);

        public void Dispose()
        {
            if (_NativeArray != null)
            {
                _NativeArray.Dispose();
            }
        }
    }
}
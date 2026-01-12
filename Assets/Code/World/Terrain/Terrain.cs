using Unity.Collections;
using UnityEngine;
using VoxelEmpires.Configuration;

namespace VoxelEmpires.World
{
    public static class Terrain
    {
        static Terrain()
        {
            _voxelByHeightConfig = GameConfig.Instance.TerrainConfiguration.GetHeightConfig();
        }

        private static readonly NativeArray<VoxelHeightConfig> _voxelByHeightConfig;

        public static byte GetVoxelIDByHeight(int naturalHeight, int Y)
        {
            for (int i = 0; i < _voxelByHeightConfig.Length; i++)
            {
                VoxelHeightConfig current = _voxelByHeightConfig[i];
                int minLimit = current.AbsoluteY;
                if (Y >= minLimit)
                {
                    int middleLimit = naturalHeight - current.Width + 1;
                    if (Y >= middleLimit)
                    {
                        return current.VoxelID;
                    }
                    else
                    {
                        return current.FillerID >= 99 ? current.VoxelID : current.FillerID;
                    }
                }
            }
            Debug.LogError("Voxel ID couldn't be provided.");
            return 99;
        }
    }
}
using System;
using UnityEngine;

namespace VoxelEmpires.Configuration
{
    [Serializable]
    public class VoxelHeightConfiguration
    {
        /// <summary>
        /// Above or equal to this altitute this Voxel will be selected.
        /// </summary>
        [Tooltip("Above or equal to this altitute this Voxel will be selected.")]
        public int AbsoluteY;
        /// <summary>
        /// Width for this voxel's top layer. 
        /// </summary>
        [Tooltip("Width for this voxel's layer in case there is a filler voxel assigned."), Range(1, 99)]
        public int Width = 1;
        /// <summary>
        /// Voxel to draw at this Y value.
        /// </summary>
        [Tooltip("Voxel to draw at this Y value.")]
        public VoxelBaseSO Voxel;
        /// <summary>
        /// If not null, this voxel will be placed after the top layer is covered (Absolute Y - Width) and until the next height configuration is reached.
        /// </summary>
        [Tooltip("If not null, this voxel will be placed after the top layer is covered (Absolute Y - Width) and until the next height configuration is reached.")]
        public VoxelBaseSO Filler;
    }
    public struct VoxelHeightConfig
    {
        public int AbsoluteY;
        public int Width;
        public byte VoxelID;
        public byte FillerID;
    }
}
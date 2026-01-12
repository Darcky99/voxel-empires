using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;

namespace VoxelEmpires.Configuration
{
    [CreateAssetMenu(fileName = "Game Configuration")]
    public class GameConfig : ScriptableObjectSingleton<GameConfig>
    {
        public CameraConfiguration CameraConfiguration;
        [field: SerializeField] public CharacterConfiguration CharacterConfiguration { get; private set; }
        [field: SerializeField] public ChunkConfiguration ChunkConfiguration { get; private set; }
        [field: SerializeField] public GraphicsConfiguration GraphicsConfiguration { get; private set; }
        [field: SerializeField] public VoxelConfiguration VoxelConfiguration { get; private set; }
        [field: SerializeField] public WorldGenerationConfiguration WorldConfiguration { get; private set; }
        [field: SerializeField] public TerrainConfiguration TerrainConfiguration { get; private set; }
    }

    [Serializable]
    public class CameraConfiguration
    {
        public float CameraDragSensibility => _CameraDragSensibility;
        public float ZoomingSensibility => _ZoomingSensibility;
        public float MinimumDistance => _MinimumDistance;
        public float MaximumDistance => _MaximumDistance;
        public float MaximumInclinationAngle => _MaximumInclinationAngle;

        [SerializeField] private float _CameraDragSensibility;
        [SerializeField] private float _ZoomingSensibility;
        [SerializeField] private float _MinimumDistance;
        [SerializeField] private float _MaximumDistance;
        [SerializeField] private float _MaximumInclinationAngle;
    }

    [Serializable]
    public class CharacterConfiguration
    {
        [field: SerializeField] public float JumpHeight { get; private set; }
        [field: SerializeField] public float JumpDuration { get; private set; }
        [field: SerializeField] public AnimationCurve JumpCurve { get; private set; }
    }

    [Serializable]
    public class ChunkConfiguration
    {
        public static readonly int KeyToWorld = 16 / 2;
        public static readonly float ChunkToWorldDistance = 8f;
        public static readonly float VoxelToWorldDistance = 0.5f;

        public int3 ChunkSize => new int3(m_ChunkSize, m_ChunkHeight, m_ChunkSize);

        [SerializeField] private int m_ChunkSize;
        [SerializeField] private int m_ChunkHeight;
    }

    [Serializable]
    public class GraphicsConfiguration
    {
        public float WorldRenderDistance => m_RenderDistance * ChunkConfiguration.ChunkToWorldDistance;
        public int RenderDistance => m_RenderDistance;

        [SerializeField] private int m_RenderDistance = 16;
    }

    [Serializable]
    public class VoxelConfiguration
    {
        private Dictionary<VoxelBaseSO, byte> _IDLookUp;

        [field: SerializeField] public VoxelBaseSO[] Voxels { get; private set; }

        private void CreateIDsLookUp()
        {
            _IDLookUp = new Dictionary<VoxelBaseSO, byte>();
            for (int i = 0; i < Voxels.Length; i++)
            {
                _IDLookUp.Add(Voxels[i], (byte)(i));
            }
        }

        public VoxelConfig[] GetVoxelsData()
        {
            VoxelConfig[] nativeArray = new VoxelConfig[Voxels.Length];
            for (int i = 0; i < Voxels.Length; i++)
                nativeArray[i] = Voxels[i].GetConfig();
            return nativeArray;
        }
        public byte GetVoxelID(VoxelBaseSO voxel)
        {
            if (_IDLookUp == null)
            {
                CreateIDsLookUp();
            }
            return _IDLookUp[voxel];
        }
    }

    [Serializable]
    public class WorldGenerationConfiguration
    {
        [field: SerializeField] public int WorldHeight { get; private set; }

        [Title("Noise configuration")]
        [field: SerializeField] public int Seed { get; private set; }
        [field: SerializeField] public float Scale { get; private set; }


        [Title("Height Curves")]
        [field: SerializeField] public AnimationCurve Continentalness { get; private set; }
        [field: SerializeField] public AnimationCurve Erosion { get; private set; }
        [field: SerializeField] public AnimationCurve PeaksAndValleys { get; set; }
    }

    [Serializable]
    public class TerrainConfiguration
    {
        [field: SerializeField] public VoxelHeightConfiguration[] VoxelsByHeight { get; private set; }

        public NativeArray<VoxelHeightConfig> GetHeightConfig()
        {
            VoxelHeightConfig[] result = new VoxelHeightConfig[VoxelsByHeight.Length];
            for (int i = 0; i < result.Length; i++)
            {
                VoxelHeightConfiguration current = VoxelsByHeight[i];
                result[i] = new VoxelHeightConfig()
                {
                    AbsoluteY = current.AbsoluteY,
                    Width = current.Width,
                    VoxelID = GameConfig.Instance.VoxelConfiguration.GetVoxelID(current.Voxel),
                    FillerID = current.Filler == null ? (byte)99 : GameConfig.Instance.VoxelConfiguration.GetVoxelID(current.Filler),
                };
            }
            return new NativeArray<VoxelHeightConfig>(result, Allocator.Persistent);
        }
    }
}
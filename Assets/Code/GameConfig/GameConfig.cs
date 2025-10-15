using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "Game Configuration")]
public class GameConfig : ScriptableObjectSingleton<GameConfig>
{
    public CameraConfiguration CameraConfiguration;
    [field: SerializeField] public CharacterConfiguration CharacterConfiguration { get; private set; }
    [field: SerializeField] public ChunkConfiguration ChunkConfiguration { get; private set; }
    [field: SerializeField] public GraphicsConfiguration GraphicsConfiguration { get; private set; }
    [field: SerializeField] public VoxelConfiguration VoxelConfiguration { get; private set; }
    [field: SerializeField] public WorldGenerationConfiguration WorldConfiguration { get; private set; }
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
    [field: SerializeField] public VoxelBaseSO[] Voxels { get; private set; }

    public VoxelConfig[] GetVoxelsData()
    {
        VoxelConfig[] nativeArray = new VoxelConfig[Voxels.Length];
        for (int i = 0; i < Voxels.Length; i++)
            nativeArray[i] = Voxels[i].GetConfig();
        return nativeArray;
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
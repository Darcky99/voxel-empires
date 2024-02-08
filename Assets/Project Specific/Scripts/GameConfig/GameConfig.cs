using System;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Game Configuration")]
public class GameConfig : ScriptableObjectSingleton<GameConfig>
{
    [field: SerializeField] public ChunkConfiguration ChunkConfiguration { get; private set; }
    [field: SerializeField] public GraphicsConfiguration GraphicsConfiguration { get; private set; }
    [field: SerializeField] public TerrainGenerationConfiguration TerrainGenerationConfiguration { get; private set; }
    [field: SerializeField] public VoxelConfiguration VoxelConfiguration { get; private set; }
}

[Serializable]
public class ChunkConfiguration
{
    public static readonly int ChunkSize = 16;
    public static readonly int ChunkVoxelCount = ChunkSize * ChunkSize * ChunkSize;

    public static readonly int FlatChunkSize = ChunkSize + 2;
    public static readonly int FlatChunkLenght = FlatChunkSize * FlatChunkSize * FlatChunkSize;

    public static readonly int KeyToWorld = 16 / 2;

    //[SerializeField, Min(16)] private int m_ChunkSizeInVoxels;
}

[Serializable]
public class GraphicsConfiguration
{
    public static int RenderDistance = 16;
}

[Serializable]
public class TerrainGenerationConfiguration
{
    public readonly static float HeightNoiseScale = 0.01f;
    public readonly static float MinimumValue = 0;

    //[field: SerializeField, Range(-1f, 1f)] public float m_MinimumValue { get; private set; }
    //[field: SerializeField] public float Scale { get; private set; }
}

[Serializable]
public class VoxelConfiguration
{
    [field: SerializeField] public SolidVoxel_SO SolidVoxel { get; private set; }
}
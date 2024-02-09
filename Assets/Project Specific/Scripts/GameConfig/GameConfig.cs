using System;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using VoxelUtils;

[CreateAssetMenu(fileName = "Game Configuration")]
public class GameConfig : ScriptableObjectSingleton<GameConfig>
{
    [field: SerializeField] public ChunkConfiguration ChunkConfiguration { get; private set; }
    [field: SerializeField] public GraphicsConfiguration GraphicsConfiguration { get; private set; }
    [field: SerializeField] public WorldGenerationConfiguration WorldConfig { get; private set; }
    [field: SerializeField] public VoxelConfiguration VoxelConfiguration { get; private set; }
}

[Serializable]
public class ChunkConfiguration
{
    public static readonly int KeyToWorld = 16 / 2;

    public int ChunkVoxelCount => m_ChunkSize * m_ChunkSize * m_ChunkSize;
    public int ChunkSize => m_ChunkSize;
    public int Expanded_ChunkSize => ChunkSize + 2;
    //public int Expanded_ChunkLenght => Expanded_ChunkSize * Expanded_ChunkSize * Expanded_ChunkSize;


    [SerializeField] private int m_ChunkSize = 16;
}

[Serializable]
public class GraphicsConfiguration
{
    public int RenderDistance => m_RenderDistance;

    [SerializeField] private int m_RenderDistance = 16;
}

[Serializable]
public class WorldGenerationConfiguration
{
    [field: SerializeField] public int WorldSize { get; private set; }
    [field: SerializeField] public int WorldHeight { get; private set; }
    public float HeightNoiseScale => m_HeightNoiseScale;

    [SerializeField] private float m_HeightNoiseScale = 0.01f;
}

[Serializable]
public class VoxelConfiguration
{
    [field: SerializeField] public SolidVoxel_SO SolidVoxel { get; private set; }
}
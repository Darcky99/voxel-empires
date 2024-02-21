using System;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using VoxelUtils;
using Unity.Collections;

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
    public static readonly float ChunkToWorldDistance = 8f;
    public static readonly float VoxelToWorldDistance = 0.5f;

    public int ChunkVoxelCount => m_ChunkSize * m_ChunkSize * ChunkHeight;
    public int ChunkSize => m_ChunkSize;
    public int ChunkHeight => m_ChunkHeight;

    [SerializeField] private int m_ChunkSize = 16;
    [SerializeField] private int m_ChunkHeight = 512;
}

[Serializable]
public class GraphicsConfiguration
{
    public float WorldRenderDistance => m_RenderDistance * ChunkConfiguration.ChunkToWorldDistance;
    public int RenderDistance => m_RenderDistance;

    [SerializeField] private int m_RenderDistance = 16;
}

[Serializable]
public class WorldGenerationConfiguration
{
    [field: SerializeField] public int WorldSizeInChunks { get; private set; }
    [field: SerializeField] public int WorldHeightInChunks { get; private set; }
    public float HeightNoiseScale => m_HeightNoiseScale;

    [SerializeField] private float m_HeightNoiseScale = 0.01f;
}

[Serializable]
public class VoxelConfiguration
{
    [field: SerializeField] public VoxelBaseSO[] Voxels { get; private set; }

    public VoxelConfig[] GetVoxelsData()
    {
        VoxelConfig[] nativeArray = new VoxelConfig[Voxels.Length];
        for(int i = 0; i < Voxels.Length; i++)
            nativeArray[i] = Voxels[i].GetConfig();
        return nativeArray;
    }
}
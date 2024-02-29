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
    [field: SerializeField] public CharacterConfiguration CharacterConfiguration { get; private set; }
    [field: SerializeField] public ChunkConfiguration ChunkConfiguration { get; private set; }
    [field: SerializeField] public GraphicsConfiguration GraphicsConfiguration { get; private set; }
    [field: SerializeField] public VoxelConfiguration VoxelConfiguration { get; private set; }
    [field: SerializeField] public WorldGenerationConfiguration WorldConfiguration { get; private set; }
}

[Serializable]
public class CharacterConfiguration
{
    [field: SerializeField] public float JumpHeight { get; private set; }
    [field: SerializeField] public float JumpDuration { get; private set; }
    [field: SerializeField] public AnimationCurve JumpCurve { get; private set;}

    [field: SerializeField] public float AccelerationTime;
    [field: SerializeField] public AnimationCurve m_GravityAcceleration;
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

[Serializable]
public class WorldGenerationConfiguration
{
    [field: SerializeField] public int WorldSizeInChunks { get; private set; }
    [field: SerializeField] public int WorldHeightInChunks { get; private set; }

    [Title("Noise configuration")]
    [field: SerializeField] public uint Seed {  get; private set; }
    [field: SerializeField] public float Scale {  get; private set; }


    [Title("Height Curves")]
    [field: SerializeField] public int CurveResolution { get; private set; }
    [field: SerializeField] public AnimationCurve Continentalness { get; private set; }
    [field: SerializeField] public AnimationCurve Erosion { get; private set; }
    [field: SerializeField] public AnimationCurve PeaksAndValleys { get; set; }

    private float[] m_ContinentalnessValues;
    private float[] m_ErosionValues;
    private float[] m_PeaksAndValleysValues;

    private float[] curveValues(AnimationCurve curve)
    {
        float[] values = new float[CurveResolution + 1];

        values[0] = curve.Evaluate(-1);
        values[values.Length - 1] = curve.Evaluate(1);

        float tf = 2f / CurveResolution;
        for (int i = 1; i < values.Length - 1; i++)
        {
            float t = -1 + (tf * i);
            values[i] = curve.Evaluate(t);
        }
        return values;
    }
    public float[] GetCurveValues(int i)
    {
        switch (i)
        {
            case 0:
                if(m_ContinentalnessValues == null || m_ContinentalnessValues.Length != CurveResolution)
                    m_ContinentalnessValues = curveValues(Continentalness);
                return m_ContinentalnessValues;
            case 1:
                if (m_ErosionValues == null || m_ErosionValues.Length != CurveResolution)
                    m_ErosionValues = curveValues(Erosion);
                return m_ErosionValues;
            case 2:
                if (m_PeaksAndValleysValues == null || m_PeaksAndValleysValues.Length != CurveResolution)
                    m_PeaksAndValleysValues = curveValues(PeaksAndValleys);
                return m_PeaksAndValleysValues;
            default:
                Debug.LogError("CHINGUEN TODOS, ASU MADREEE");
                return null;
        }
    }
}
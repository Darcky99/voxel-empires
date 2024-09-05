using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelUtils;

[BurstCompile]
public struct ITerrainGeneration : IJob
{
    public ITerrainGeneration(Vector3Int ChunkID)
    {
        m_ChunkID = new int3(ChunkID.x, ChunkID.y, ChunkID.z);

        FlatVoxelMap = new NativeArray<byte>(GameConfig.Instance.ChunkConfiguration.ChunkVoxelCount, Allocator.Persistent);
        IsEmpty = new NativeArray<bool>(new bool[]{ true }, Allocator.Persistent);

        //generate a region data based on 3 curvese
        m_CurveResolution = GameConfig.Instance.WorldConfiguration.CurveResolution;

        Continentalness = new NativeArray<float>(GameConfig.Instance.WorldConfiguration.GetCurveValues(0), Allocator.Persistent);
        Erosion = new NativeArray<float>(GameConfig.Instance.WorldConfiguration.GetCurveValues(1), Allocator.Persistent);
        PeaksAndValleys = new NativeArray<float>(GameConfig.Instance.WorldConfiguration.GetCurveValues(2), Allocator.Persistent);

        m_Seed = GameConfig.Instance.WorldConfiguration.Seed;
        m_Scale = GameConfig.Instance.WorldConfiguration.Scale;
    }

    public NativeArray<byte> FlatVoxelMap;
    public NativeArray<bool> IsEmpty;

    private int m_CurveResolution;

    private uint m_Seed;
    private float m_Scale;

    private NativeArray<float> Continentalness;
    private NativeArray<float> Erosion;
    private NativeArray<float> PeaksAndValleys;


    private readonly int3 m_ChunkID;

    public void Execute()
    {
        int chunkSize = Voxels.s_ChunkSize;
        float p = 1f;
        float l = 1f;

        int3
            globalChunkPosition = m_ChunkID * chunkSize,
            voxelLocalPosition = 0;

        for (voxelLocalPosition.x = 0; voxelLocalPosition.x < chunkSize; voxelLocalPosition.x++)
            for (voxelLocalPosition.z = 0; voxelLocalPosition.z < chunkSize; voxelLocalPosition.z++)
            {
                int sampleX = globalChunkPosition.x + voxelLocalPosition.x;
                int sampleZ = globalChunkPosition.z + voxelLocalPosition.z;

                float c_noice = Noise.Perlin2D(sampleX, sampleZ, m_Seed, 0.15f * m_Scale, 4, 0.55f * p, 1.25f * l);
                float e_noice = Noise.Perlin2D(sampleX, sampleZ, m_Seed, 1 * m_Scale, 4, 0.76f * p, 2f * l);
                float pv_noice = Noise.Perlin2D(sampleX, sampleZ, m_Seed, 0.5f * m_Scale, 3, 0.95f * p, 3.1f * l);
                pv_noice = math.abs(pv_noice);
                float c = evaluate(c_noice, 0);
                float e = evaluate(e_noice, 1);
                float pv = evaluate(pv_noice, 2);

                float CEP = (c + (e * pv)) / 2f;

                int h = (int)math.floor(CEP * 128);
                int lh = (int)math.floor(h - globalChunkPosition.y);
                lh = math.clamp(lh, -1, Voxels.s_ChunkHeight - 1);

                for (voxelLocalPosition.y = lh; voxelLocalPosition.y >= 0; voxelLocalPosition.y--)
                {
                    int i = index(voxelLocalPosition);
                    int globalY = globalChunkPosition.y + voxelLocalPosition.y;

                    if (globalY >= 115)
                        FlatVoxelMap[i] = 3;
                    else if (globalY >= 85)
                        FlatVoxelMap[i] = 2;
                    else if(globalY >= 45)
                        FlatVoxelMap[i] = 4;
                    else if (globalY >= 15)
                        FlatVoxelMap[i] = 5;
                    else
                        FlatVoxelMap[i] = 1;

                    IsEmpty[0] = false;
                }
            }
    }
    public void Dispose()
    {
        FlatVoxelMap.Dispose();
        Continentalness.Dispose();
        Erosion.Dispose();
        PeaksAndValleys.Dispose();
    }

    private int index(int3 position) => Voxels.Index(position.x, position.y, position.z);
    private float evaluate(float time, int curve)
    {
        NativeArray<float> target = default;
        switch (curve)
        {
            case 0:
                target = Continentalness; 
            break;
            case 1:
                target = Erosion;
            break;
            case 2:
                target = PeaksAndValleys;
            break;
            default:
                Debug.LogError("Not valid");
                target = Continentalness;
            break;
        }

        int closestIndex = 0;
        double closestDistance = 2f;
        double resolution = 2f / m_CurveResolution;
        for (int i = 0; i < target.Length; i++)
        {
            double ct =  -1 + (resolution * i);
            double d = math.abs(time - ct);
            if(d < closestDistance)
            {
                closestIndex = i;
                closestDistance = d;
            }
        }
        return target[closestIndex];
    }
}
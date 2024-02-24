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
        m_CurveResolution = GameConfig.Instance.WorldConfig.CurveResolution;

        Continentalness = GameConfig.Instance.WorldConfig.ContinentalnessValues;
        Erosion = GameConfig.Instance.WorldConfig.ErosionValues;
        PeaksAndValleys = GameConfig.Instance.WorldConfig.PeaksAndValleysValues;

        m_Seed = GameConfig.Instance.WorldConfig.Seed;
        m_Scale = GameConfig.Instance.WorldConfig.Scale;
    }

    public NativeArray<byte> FlatVoxelMap;
    public NativeArray<bool> IsEmpty;

    private int m_CurveResolution;

    private uint m_Seed;
    private float m_Scale;

    private NativeHashMap<float, float> Continentalness;
    private NativeHashMap<float, float> Erosion;
    private NativeHashMap<float, float> PeaksAndValleys;


    private readonly int3 m_ChunkID;

    public void Execute()
    {
        int chunkSize = Voxels.s_ChunkSize;
        int seed = 255897;

        int3
            globalChunkPosition = m_ChunkID * chunkSize,
            voxelLocalPosition = 0;

        for (voxelLocalPosition.x = 0; voxelLocalPosition.x < chunkSize; voxelLocalPosition.x++)
            for (voxelLocalPosition.z = 0; voxelLocalPosition.z < chunkSize; voxelLocalPosition.z++)
            {
                int sampleX = globalChunkPosition.x + voxelLocalPosition.x;
                int sampleZ = globalChunkPosition.z + voxelLocalPosition.z;

                float c_noice = Noise.Perlin2D(sampleX, sampleZ, m_Seed, 17.3f * m_Scale, 3, 0.16f, 5.6f);
                float e_noice = Noise.Perlin2D(sampleX, sampleZ, m_Seed, 53.9f * m_Scale, 2, 0.33f, 4.9f);
                float pv_noice = Noise.Perlin2D(sampleX, sampleZ, m_Seed, 15.2f * m_Scale, 3, 0.83f, 1.6f);

                float c = evaluate(c_noice, 0);
                float e = evaluate(e_noice, 1);
                float pv = evaluate(pv_noice, 2);

                float CEP = (c + (e * pv)) / 2;

                int h = (int)math.round(CEP * 128);

                int localHeight = (int)math.clamp(math.round(h - globalChunkPosition.y), -1, Voxels.s_ChunkHeight - 1);

                for (voxelLocalPosition.y = localHeight; voxelLocalPosition.y >= 0; voxelLocalPosition.y--)
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
    private float evaluate(float noise, int curve)
    {
        NativeHashMap<float, float> target = default;

        //if i was to use an array, from noise look for the closest value
        //remmber it's position index
        //

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
                target = Continentalness;
            break;
        }

        float closestKey = 0f;
        float closestDistance = math.abs(noise - closestKey); ;

        for(int i = 0; i <= m_CurveResolution; i++)
        {
            float k =  -1 + ((2f / m_CurveResolution) * i);
            float d = math.abs(noise - k);

            if(d < closestDistance)
            {
                closestKey = k;
                closestDistance = d;
            }
        }
        return target[closestKey];
    }
}
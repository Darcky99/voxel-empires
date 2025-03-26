using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelUtilities;

[BurstCompile]
public struct ITerrainGeneration : IJob
{
    public ITerrainGeneration(int2 chunkID, int3 chunkSize)
    {
        _ChunkID = chunkID;
        _TerrainMaxHeight = (uint)GameConfig.Instance.WorldConfiguration.WorldHeight;

        HeightMap = new NativeGrid<byte>(chunkSize, Allocator.Persistent);
        IsEmpty = true;

        _CurveResolution = GameConfig.Instance.WorldConfiguration.CurveResolution;

        Continentalness = new NativeArray<float>(GameConfig.Instance.WorldConfiguration.GetCurveValues(0), Allocator.Persistent);
        Erosion = new NativeArray<float>(GameConfig.Instance.WorldConfiguration.GetCurveValues(1), Allocator.Persistent);
        PeaksAndValleys = new NativeArray<float>(GameConfig.Instance.WorldConfiguration.GetCurveValues(2), Allocator.Persistent);

        _Seed = GameConfig.Instance.WorldConfiguration.Seed;
        _Scale = GameConfig.Instance.WorldConfiguration.Scale;
    }

    public NativeGrid<byte> HeightMap;
    public bool IsEmpty;

    private int _CurveResolution;
    private uint _TerrainMaxHeight;

    private uint _Seed;
    private float _Scale;

    private NativeArray<float> Continentalness;
    private NativeArray<float> Erosion;
    private NativeArray<float> PeaksAndValleys;

    private readonly int2 _ChunkID;

    public void Execute()
    {
        int2 globalChunkPosition = new int2(_ChunkID.x * HeightMap.Lenght.x, _ChunkID.y * HeightMap.Lenght.z);
        float persistance = 1f;
        float lacunarity = 1f;

        for (int x = 0; x < HeightMap.Lenght.x; x++)
        {
            for (int z = 0; z < HeightMap.Lenght.z; z++)
            {
                int sampleX = globalChunkPosition.x + x;
                int sampleZ = globalChunkPosition.y + z;

                float continentalness_noice = Noise.Perlin2D(sampleX, sampleZ, _Seed, 0.15f * _Scale, 4, 0.55f * persistance, 1.25f * lacunarity);
                float erosion_noice = Noise.Perlin2D(sampleX, sampleZ, _Seed, 1 * _Scale, 4, 0.76f * persistance, 2f * lacunarity);
                float peaksandvalleys_noice = Noise.Perlin2D(sampleX, sampleZ, _Seed, 0.5f * _Scale, 3, 0.95f * persistance, 3.1f * lacunarity);
                peaksandvalleys_noice = math.abs(peaksandvalleys_noice);
                float c = Evaluate(continentalness_noice, 0);
                float e = Evaluate(erosion_noice, 1);
                float pv = Evaluate(peaksandvalleys_noice, 2);

                float CEP = (c + (e * pv)) / 2f;
                int terrainHeight = (int)math.floor(CEP * _TerrainMaxHeight);
                HeightMap.SetValue(new int3(x, 0, z), (byte)terrainHeight);
            }
        }
        IsEmpty = false;
    }
    public void Dispose()
    {
        Continentalness.Dispose();
        Erosion.Dispose();
        PeaksAndValleys.Dispose();
    }

    private float Evaluate(float time, int curve)
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
        double resolution = 2f / _CurveResolution;
        for (int i = 0; i < target.Length; i++)
        {
            double ct = -1 + (resolution * i);
            double d = math.abs(time - ct);
            if (d < closestDistance)
            {
                closestIndex = i;
                closestDistance = d;
            }
        }
        return target[closestIndex];
    }
}
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct TerrainGenerationJob : IJobParallelFor
{
    public TerrainGenerationJob(int3 ChunkID)
    {
        m_ChunkID = ChunkID;

        m_ChunkSize = ChunkConfiguration.ChunkSize;
        m_ChunkVoxelCount = m_ChunkSize * m_ChunkSize * m_ChunkSize;

        flatVoxelMap = new NativeArray<byte>(m_ChunkVoxelCount, Allocator.Persistent);
    }

    public NativeArray<byte> FlatVoxelMap => flatVoxelMap;

    private readonly int m_ChunkSize;
    private readonly int m_ChunkVoxelCount;
    private readonly int3 m_ChunkID;

    private NativeArray<byte> flatVoxelMap;

    public void Execute(int i)
    {
        int3
            globalChunkPosition = m_ChunkID * m_ChunkSize,
            voxelLocalPosition = default;

        float3 globalVoxelPositon;

        voxelLocalPosition = Voxels.XYZnoExpanded(i);

        //if (voxelLocalPosition.x <= 0 || voxelLocalPosition.x >= m_ChunkSize ||
        //    voxelLocalPosition.y <= 0 || voxelLocalPosition.y >= m_ChunkSize ||
        //    voxelLocalPosition.z <= 0 || voxelLocalPosition.z >= m_ChunkSize)
        //{
        //    flatVoxelMap[i] = 0;
        //    return;
        //}

        globalVoxelPositon = globalChunkPosition + voxelLocalPosition;

        float heightVariationNoise = (noise.cnoise(new float2(globalVoxelPositon.x, globalVoxelPositon.z) * TerrainGenerationConfiguration.HeightNoiseScale) + 1) / 2;

        float maxHeight = (heightVariationNoise * 35) + 10;
        float minHeight = (heightVariationNoise * 25) + 10;

        bool heightCondition = globalVoxelPositon.y < maxHeight || globalVoxelPositon.y <= minHeight;


        if (heightCondition)
        {
            flatVoxelMap[i] = 1;
            //Debug.Log("Un cubo");
        }
        else
        {
            flatVoxelMap[i] = 0;
            //Debug.Log("Nada");
        }
    }
}
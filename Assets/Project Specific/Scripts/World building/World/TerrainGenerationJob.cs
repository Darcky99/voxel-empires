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

        m_FlatChunkSize = ChunkConfiguration.FlatChunkSize;
        m_FlatChunkLenght = ChunkConfiguration.FlatChunkLenght;
        m_ChunkSize = ChunkConfiguration.ChunkSize;

        flatVoxelMap = new NativeArray<byte>(m_FlatChunkLenght, Allocator.Persistent);
    }

    public NativeArray<byte> FlatVoxelMap => flatVoxelMap;

    private readonly int m_FlatChunkSize;
    private readonly int m_FlatChunkLenght;
    private readonly int m_ChunkSize;
    private readonly int3 m_ChunkID;

    private NativeArray<byte> flatVoxelMap;

    public void Execute(int i)
    {
        int3
            globalChunkPosition = m_ChunkID * m_ChunkSize,
            voxelLocalPosition = default;

        float3 globalVoxelPositon;

        //for(int i = 0; i < m_FlatChunkLenght; i++)
        //{
        voxelLocalPosition = Voxels.XYZ(i);

        if (voxelLocalPosition.x <= 0 || voxelLocalPosition.x >= m_FlatChunkSize ||
            voxelLocalPosition.y <= 0 || voxelLocalPosition.y >= m_FlatChunkSize ||
            voxelLocalPosition.z <= 0 || voxelLocalPosition.z >= m_FlatChunkSize)
        {
            flatVoxelMap[i] = 0;
            return;
        }

        globalVoxelPositon = globalChunkPosition + voxelLocalPosition;

        //float densityNoise = noise.cnoise(globalVoxelPositon * TerrainGenerationConfiguration.NoiseScale * 0.05f);
        float heightVariationNoise = (noise.cnoise(new float2(globalVoxelPositon.x, globalVoxelPositon.z) * TerrainGenerationConfiguration.HeightNoiseScale) + 1) / 2;

        float maxHeight = (heightVariationNoise * 35) + 10;
        float minHeight = (heightVariationNoise * 25) + 10;

        bool heightCondition = globalVoxelPositon.y < maxHeight || globalVoxelPositon.y <= minHeight;

        //if (m_ChunkID.y == 2 && voxelLocalPosition.y >= 8)
        //    Debug.Log(heightCondition);

        if (/*(*//*densityNoise > TerrainGenerationConfiguration.MinimumValue*/ /*&&*/ heightCondition)
            flatVoxelMap[i] = 1;
        else
            flatVoxelMap[i] = 0;
        //}


    }
}
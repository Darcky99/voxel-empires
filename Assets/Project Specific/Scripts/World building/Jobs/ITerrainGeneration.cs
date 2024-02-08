using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoxelUtils;

[BurstCompile]
public struct ITerrainGeneration : IJobParallelFor
{
    public ITerrainGeneration(Vector3Int ChunkID)
    {
        m_ChunkID = new int3(ChunkID.x, ChunkID.y, ChunkID.z);

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

        voxelLocalPosition = Voxels.XYZ(i);

        globalVoxelPositon = globalChunkPosition + voxelLocalPosition;

        float heightVariationNoise = (noise.cnoise(new float2(globalVoxelPositon.x, globalVoxelPositon.z) * TerrainGenerationConfiguration.HeightNoiseScale) + 1) / 2;

        float maxHeight = (heightVariationNoise * 35) + 10;
        float minHeight = (heightVariationNoise * 25) + 10;

        bool heightCondition = globalVoxelPositon.y < maxHeight || globalVoxelPositon.y <= minHeight;

        flatVoxelMap[i] = (byte) (heightCondition ? 1 : 0);
    }
}
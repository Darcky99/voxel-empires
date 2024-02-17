using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelUtils;

[BurstCompile]
public struct ITerrainGeneration : IJobParallelFor
{
    public ITerrainGeneration(Vector3Int ChunkID)
    {
        m_ChunkID = new int3(ChunkID.x, ChunkID.y, ChunkID.z);

        m_ChunkSize = GameConfig.Instance.ChunkConfiguration.ChunkSize;
        m_ChunkVoxelCount = GameConfig.Instance.ChunkConfiguration.ChunkVoxelCount;
        m_HeightNoiseScale = GameConfig.Instance.WorldConfig.HeightNoiseScale;

        FlatVoxelMap = new NativeArray<byte>(m_ChunkVoxelCount, Allocator.Persistent);
    }

    public NativeArray<byte> FlatVoxelMap;

    private readonly int m_ChunkSize;
    private readonly int m_ChunkVoxelCount;
    private readonly float m_HeightNoiseScale;
    private readonly int3 m_ChunkID;

    public void Execute(int i)
    {
        int3
            globalChunkPosition = m_ChunkID * m_ChunkSize,
            voxelLocalPosition = Voxels.XYZ(i),
            globalVoxelPositon = globalChunkPosition + voxelLocalPosition;
        float2 XZ;
        XZ.x = globalVoxelPositon.x;
        XZ.y = globalVoxelPositon.z;

        float noiseValue = (noise.cnoise(XZ * m_HeightNoiseScale) + 1f) / 2f;
        float maxHeight = noiseValue * (512);
        float minGrass = noiseValue * (510f);

        if (globalVoxelPositon.y > maxHeight)
            return;

        if (globalVoxelPositon.y >= minGrass)
            FlatVoxelMap[i] = 3;
        else
            FlatVoxelMap[i] = 2;
    }

    public void Dispose()
    {
        FlatVoxelMap.Dispose();
    }
}
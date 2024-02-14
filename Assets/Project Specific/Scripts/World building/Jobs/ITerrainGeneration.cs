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

    //private NativeArray<byte> m_FlatVoxelMap;

    public void Execute(int i)
    {
        int3
            globalChunkPosition = m_ChunkID * m_ChunkSize,
            voxelLocalPosition = Voxels.XYZ(i),
            globalVoxelPositon = globalChunkPosition + voxelLocalPosition;

        float2 XZ;
        XZ.x = globalVoxelPositon.x;
        XZ.y = globalVoxelPositon.z;

        float heightValue = (noise.cnoise(XZ * m_HeightNoiseScale) + 1f) / 2f;

        bool heightCondition = globalVoxelPositon.y <= heightValue * 5 * 16;
        FlatVoxelMap[i] = (byte)(heightCondition ? 1 : 0);
    }

    public void Dispose()
    {
        FlatVoxelMap.Dispose();
    }
}
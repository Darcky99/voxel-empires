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

        m_ChunkSize = GameConfig.Instance.ChunkConfiguration.ChunkSize;
        m_HeightNoiseScale = GameConfig.Instance.WorldConfig.HeightNoiseScale;

        FlatVoxelMap = new NativeArray<byte>(GameConfig.Instance.ChunkConfiguration.ChunkVoxelCount, Allocator.Persistent);
        IsEmpty = new NativeArray<bool>(new bool[]{ true }, Allocator.Persistent);
    }

    public NativeArray<byte> FlatVoxelMap;
    public NativeArray<bool> IsEmpty;

    private readonly int m_ChunkSize;
    private readonly float m_HeightNoiseScale;
    private readonly int3 m_ChunkID;

    public void Execute()
    {
        int3
                globalChunkPosition = m_ChunkID * m_ChunkSize,
                voxelLocalPosition = 0,
                globalVoxelPositon = globalChunkPosition + voxelLocalPosition;

        for (voxelLocalPosition.x = 0; voxelLocalPosition.x < m_ChunkSize; voxelLocalPosition.x++)
            for (voxelLocalPosition.z = 0; voxelLocalPosition.z < m_ChunkSize; voxelLocalPosition.z++)
            {
                float2 XZ;
                XZ.x = globalChunkPosition.x + voxelLocalPosition.x;
                XZ.y = globalChunkPosition.z + voxelLocalPosition.z;

                float noiseValue = (noise.cnoise(XZ * m_HeightNoiseScale) + 1f) / 2f;
                int maxHeight = (int)math.round(noiseValue * 256);
                int minGrass = maxHeight - 1;
                int height = (int)math.clamp(math.round(maxHeight - globalChunkPosition.y), -1, 15);

                for (voxelLocalPosition.y = height; voxelLocalPosition.y >= 0; voxelLocalPosition.y--)
                {
                    globalVoxelPositon = globalChunkPosition + voxelLocalPosition;

                    int index = Index(voxelLocalPosition);
                    if (globalVoxelPositon.y >= minGrass)
                        FlatVoxelMap[index] = 3;
                    else
                        FlatVoxelMap[index] = 2;
                    IsEmpty[0] = false;
                }
            }
    }
    

    public void Dispose()
    {
        FlatVoxelMap.Dispose();
    }

    private int Index(int3 position) => Voxels.Index(position.x, position.y, position.z);

}
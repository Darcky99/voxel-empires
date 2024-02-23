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
    }

    public NativeArray<byte> FlatVoxelMap;
    public NativeArray<bool> IsEmpty;

    //region
        //cuve_a hashmap
        //cuve_b hashmap
        //cuve_c hashmap

    private readonly int3 m_ChunkID;

    public void Execute()
    {
        int chunkSize = Voxels.s_ChunkSize;
        float nf;

        int3
            globalChunkPosition = m_ChunkID * chunkSize,
            voxelLocalPosition = 0;

        for (voxelLocalPosition.x = 0; voxelLocalPosition.x < chunkSize; voxelLocalPosition.x++)
            for (voxelLocalPosition.z = 0; voxelLocalPosition.z < chunkSize; voxelLocalPosition.z++)
            {
                float2 XZ;
                XZ.x = globalChunkPosition.x + voxelLocalPosition.x;
                XZ.y = globalChunkPosition.z + voxelLocalPosition.z;

                nf = 0.0005f;
                float a1 = noise.cnoise(XZ * nf);
                float a2 = noise.cnoise(XZ * nf * 4)  / 4;
                float a3 = noise.cnoise(XZ * nf * 16)  / 16;
                float a4 = noise.cnoise(XZ * nf * 64)  / 64;

                float n = ((a1 + a2 + a3 + a4) + 1) / 2;
                //n from -1 to 1
                //ah = Evaluate(a);
                //same with b and maybe c

                //h = Combine(ah, bh, ch);

                int h = (int)math.round(n * 178);

                int localHeight = (int)math.clamp(math.round(h - globalChunkPosition.y), -1, Voxels.s_ChunkHeight - 1);
                for (voxelLocalPosition.y = localHeight; voxelLocalPosition.y >= 0; voxelLocalPosition.y--)
                {
                    int index = Index(voxelLocalPosition);
                    //if (voxelLocalPosition.y == localHeight)
                    //    FlatVoxelMap[index] = 3;
                    //else
                        FlatVoxelMap[index] = 5;
                    IsEmpty[0] = false;
                }
            }
    }
    

    public void Dispose()
    {
        FlatVoxelMap.Dispose();
    }

    private int Index(int3 position) => Voxels.Index(position.x, position.y, position.z);

    private float Evaluate(float n, float[] values)
    {
        //Look for the closest key, return the actual value of it
        float c = values[0];
        return c;
    }
}
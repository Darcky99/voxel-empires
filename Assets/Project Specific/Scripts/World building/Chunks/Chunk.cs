using Chunks.Manager;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Chunk
{
    private ChunksManager m_ChunksManager => ChunksManager.Instance;

    public Chunk(int3 ID)
    {
        m_VoxelMap = new VoxelMap((byte)ChunkConfiguration.ChunkSize);

        m_ChunkID = ID;
    }

    private int3 m_ChunkID;
    private VoxelMap m_VoxelMap;

    public byte GetVoxel(int3 voxelPosition)
    {
        return m_VoxelMap.GetVoxel(voxelPosition);
    }
    public void SetVoxel(int3 voxelPosition) {
        m_VoxelMap.SetVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, 1);
    }

    public void SetVoxelMap(NativeArray<byte> flatVoxelMap)
    {
        m_VoxelMap.SetFlatMap(flatVoxelMap);
    }
    public byte[] GetVoxelMap()
    {
        int flatChunkSizeMaxIndex = ChunkConfiguration.FlatChunkSize - 1;

        byte[] flatMap = m_VoxelMap.FlatMap;

        if (flatMap.Length == 1)
            return flatMap;

        for (int y = 0; y <= flatChunkSizeMaxIndex; y++)
            for (int z = 0; z <= flatChunkSizeMaxIndex; z++)
                for (int x = 0; x <= flatChunkSizeMaxIndex; x++)
                {
                    if (!(x == 0 || x == flatChunkSizeMaxIndex || y == 0 || y == flatChunkSizeMaxIndex || z == 0 || z == flatChunkSizeMaxIndex))
                        continue;

                    int3 adjacentChunkID = default, adjacentVoxel = default;

                    adjacentChunkID.x = x == 0 ? m_ChunkID.x - 1 : x == 17 ? m_ChunkID.x + 1 : m_ChunkID.x;
                    adjacentChunkID.y = y == 0 ? m_ChunkID.y - 1 : y == 17 ? m_ChunkID.y + 1 : m_ChunkID.y;
                    adjacentChunkID.z = z == 0 ? m_ChunkID.z - 1 : z == 17 ? m_ChunkID.z + 1 : m_ChunkID.z;

                    Chunk adjacentChunk = m_ChunksManager.GetChunk(adjacentChunkID);

                    if(adjacentChunk == null)
                    {
                        flatMap[Voxels.Index(x, y, z)] = 0;
                        continue;
                    }

                    adjacentVoxel.x = x == 0 ? 15 : x == 17 ? 0 : x - 1;
                    adjacentVoxel.y = y == 0 ? 15 : y == 17 ? 0 : y - 1;
                    adjacentVoxel.z = z == 0 ? 15 : z == 17 ? 0 : z - 1;

                    flatMap[Voxels.Index(x, y, z)] = adjacentChunk.GetVoxel(adjacentVoxel);
                }

        return flatMap;
    }

    public void SetMesh(NativeList<float3> vertices, NativeList<int> triangles, NativeList<float2> uvs) 
    {
        ChunkMesh chunkMesh  = ChunkMeshPool.s_Instance.DeQueue();
        chunkMesh.SetMesh(m_ChunkID, vertices, triangles, uvs);
    }
}
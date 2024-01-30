using Chunks.Manager;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class Chunk
{
    private ChunksManager m_ChunksManager => ChunksManager.Instance;

    #region Editor
    
    public void OnDrawGizmos()
    {
        int chunkSize = ChunkConfiguration.ChunkSize;

        int3 one = new int3(1, 1, 1);

        Vector3 offset = new Vector3(m_ChunkID.x, m_ChunkID.y, m_ChunkID.z) * chunkSize * .5f;

        for (int y = 0; y < chunkSize; y++)
            for (int z = 0; z < chunkSize; z++)
                for (int x = 0; x < chunkSize; x++)
                {
                    int3 position = new int3(x, y, z);
                    byte blockID = GetVoxel(position);

                    if (blockID == 0)
                        continue;

                    position -= one;
                    Vector3 voxelCenter = new Vector3(position.x * 0.5f, position.y * 0.5f, position.z * 0.5f);
                    position += one;

                    Gizmos.DrawWireCube(offset + voxelCenter, Vector3.one * 0.5f);
                }
    }

    #endregion


    public Chunk(int3 ID)
    {
        m_VoxelMap = new VoxelMap((byte)ChunkConfiguration.ChunkSize, 0);

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

    public async Task SetVoxelMap(NativeArray<byte> flatVoxelMap)
    {
        if (m_ChunkID.x == -1 && m_ChunkID.y == 1 && m_ChunkID.z == 0)
            await m_VoxelMap.SetFlatMap(flatVoxelMap, true);
        else
            await m_VoxelMap.SetFlatMap(flatVoxelMap, false);
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

    public async Task SetMesh(NativeList<float3> vertices, NativeList<int> triangles, NativeList<float2> uvs) 
    {
        ChunkMesh chunkMesh  = ChunkMeshPool.s_Instance.DeQueue();
        await chunkMesh.SetMesh(m_ChunkID, vertices, triangles, uvs);
    }
}
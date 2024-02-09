using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelUtils;

[BurstCompile]
public struct IChunkMesh : IJob
{
    public IChunkMesh(byte[] centralChunk, Vector3Int id)
    {
        m_ChunkSize = GameConfig.Instance.ChunkConfiguration.ChunkSize;
        m_Expanded_ChunkSize = GameConfig.Instance.ChunkConfiguration.Expanded_ChunkSize;

        Vertices = new NativeList<Vector3>(Allocator.Persistent);
        Triangles = new NativeList<int>(Allocator.Persistent);
        UVs = new NativeList<Vector2>(Allocator.Persistent);

        m_Expanded_Flat_Chunk = new NativeArray<byte>(centralChunk, Allocator.Persistent);
        m_ID = new int3(id.x, id.y, id.z);
        m_One = new int3(1, 1, 1);
    }

    private readonly int m_ChunkSize;
    private readonly int m_Expanded_ChunkSize;

    public NativeList<Vector3> Vertices { get; private set; }
    public NativeList<int> Triangles { get; private set; }
    public NativeList<Vector2> UVs { get; private set; }

    private readonly NativeArray<byte> m_Expanded_Flat_Chunk;
    private readonly int3 m_One;
    private readonly int3 m_ID;

    public void Execute()
    {
        if (m_Expanded_Flat_Chunk.Length == 1)
            return;

        for (int y = 1; y <= m_ChunkSize; y++)
            for (int z = 1; z <= m_ChunkSize; z++)
                for (int x = 1; x <= m_ChunkSize; x++)
                {
                    int3 position = new int3(x, y, z);
                    byte blockID = GetValue(position);

                    if (blockID == 0)
                        continue;

                    position -= m_One;
                    float3 voxelCenter = new float3(position.x * 0.5f, position.y * 0.5f, position.z * 0.5f);
                    position += m_One;

                    for (int faceIndex = 0; faceIndex <= 5; faceIndex++)
                    {
                        byte adjacentID = GetValue(position + Voxels.GetCheckDirection(faceIndex));

                        if (adjacentID != 0  || (m_ID.y == 0 && y == 1 && faceIndex == 1))
                            continue;

                        NativeArray<float3> faceVertices = Voxels.GetFaceVertices(faceIndex);
                        Triangles.AddRange(new NativeArray<int>(Voxels.GetFaceTriangles(Vertices.Length), Allocator.Temp));

                        foreach (float3 vertex in faceVertices)
                            Vertices.Add(voxelCenter + vertex);
                    }
                }
    }

    private byte GetValue(int x, int y, int z)
    {
        if (x < 0 || x >= m_Expanded_ChunkSize || y < 0 || y >= m_Expanded_ChunkSize || z < 0 || z >= m_Expanded_ChunkSize)
            Debug.LogError("Out of limits");

        return m_Expanded_Flat_Chunk[Voxels.Expanted_Index(x, y, z)];
    }
    private byte GetValue(int3 xyz) => GetValue(xyz.x, xyz.y, xyz.z);

    public void Dispose()
    {
        Vertices.Dispose();
        Triangles.Dispose();
        UVs.Dispose();
        m_Expanded_Flat_Chunk.Dispose();
    }
}
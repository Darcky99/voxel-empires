using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Chunks;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;


public class ChunkMesh : MonoBehaviour
{
    private Chunk m_Chunk;
    [SerializeField] private MeshFilter m_LODs;

    private void updateMesh()
    {
        IChunkMesh job = new IChunkMesh(m_Chunk.Get_Expanded_VoxelMap(), m_Chunk.ChunkID);
        JobHandle jobHandle = job.Schedule();
        jobHandle.Complete();

        Mesh mesh = new Mesh();
        mesh.vertices = job.Vertices.ToArrayNBC();
        mesh.triangles = job.Triangles.ToArrayNBC();
        mesh.uv = job.UVs.ToArrayNBC();
        job.Dispose();
        mesh.RecalculateNormals();
        m_LODs.mesh = mesh;
    }

    public void Initialize(Chunk chunk)
    {
        m_Chunk = chunk;
        Vector3Int chunkID = m_Chunk.ChunkID;
        Vector3 worldPosition = (new Vector3(chunkID.x, chunkID.y, chunkID.z) * 16) / 2f;
        transform.position = worldPosition;

        updateMesh();
    }
}
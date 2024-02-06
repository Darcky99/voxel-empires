using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Chunks;
using static UnityEngine.Mesh;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;


public class ChunkMesh : MonoBehaviour
{
    private Chunk m_Chunk;
    [SerializeField] private MeshFilter m_LODs;

    private void updateMesh()
    {
        ChunkMeshJob job = new ChunkMeshJob(m_Chunk.Get_Expanded_VoxelMap());
        JobHandle jobHandle = job.Schedule();
        jobHandle.Complete();

        Mesh mesh = new Mesh();
        mesh.vertices = job.Vertices.ToArrayNBC();
        mesh.triangles = job.Triangles.ToArrayNBC();
        mesh.uv = job.UVs.ToArrayNBC();
        mesh.RecalculateNormals();
        m_LODs.mesh = mesh;
    }

    public void Initialize(Chunk chunk)
    {
        m_Chunk = chunk;
        Vector3Int chunkID = m_Chunk.ChunkID;
        Vector3 worldPosition = (new Vector3(chunkID.x, chunkID.y, chunkID.z) * 16) / 2f;
        transform.position = worldPosition;

        float time = Time.realtimeSinceStartup;
        updateMesh();
        Debug.Log($"Updating mesh time {Time.realtimeSinceStartup - time}");
    }
}
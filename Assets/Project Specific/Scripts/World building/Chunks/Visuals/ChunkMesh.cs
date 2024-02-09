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

    private void setMesh(Mesh mesh)
    {
        m_LODs.mesh = mesh;
    }

    public void Initialize(Chunk chunk, Mesh mesh)
    {
        m_Chunk = chunk;

        Vector3Int chunkID = m_Chunk.ChunkID;
        Vector3 worldPosition = (new Vector3(chunkID.x, chunkID.y, chunkID.z) * 16) / 2f;
        transform.position = worldPosition;

        setMesh(mesh);
    }
    public void SetMesh(Mesh mesh) => setMesh(mesh);
}
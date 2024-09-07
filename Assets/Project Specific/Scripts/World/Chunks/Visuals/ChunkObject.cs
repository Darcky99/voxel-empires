using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using World;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;

namespace World
{
    public class ChunkObject : MonoBehaviour
    {
        private WorldManager _WorldManager => WorldManager.Instance;

        private Chunk _Chunk;
        [SerializeField] private MeshFilter lods;
        [SerializeField] private MeshCollider meshCollider;

        private void SetMesh(Mesh mesh)
        {
            lods.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }

        public void Set(Chunk chunk, Mesh mesh)
        {
            _Chunk = chunk;
            transform.position = ChunkUtils.ChunkIDToWorldCoordinates(_Chunk.ChunkID);
            SetMesh(mesh);
        }
    }
}
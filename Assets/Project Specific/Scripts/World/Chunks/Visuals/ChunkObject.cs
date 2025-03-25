using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using World;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;
using System;

namespace World
{
    public class ChunkObject : MonoBehaviour
    {
        private WorldManager _WorldManager => WorldManager.Instance;
        private CameraController _CameraController => CameraController.Instance;

        public Chunk Chunk => _Chunk;

        [field: SerializeField] public bool HasTerrain { get; private set; }
        [field: SerializeField] public bool HasMesh { get; private set; }

        private Chunk _Chunk;

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshCollider meshCollider;

        public void Initialize(int2 chunkID)
        {
            _Chunk = new Chunk(chunkID);
            transform.position = ChunkUtils.ChunkIDToWorldCoordinates(_Chunk.ChunkID);
            gameObject.name = $"Chunk {chunkID}";
            HasTerrain = false;
            HasMesh = false;
        }
        public void SetVoxels(NativeArray<byte> voxels)
        {
            _Chunk.SetVoxelMap(voxels);
            HasTerrain = true;
        }
        public void SetMesh(IChunkMesh meshJob)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = meshJob.Vertices.ToArrayNBC();
            mesh.triangles = meshJob.Triangles.ToArrayNBC();
            mesh.SetUVs(0, meshJob.UVs.ToArrayNBC());
            meshJob.Dispose();
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            HasMesh = true;
        }
    }
}
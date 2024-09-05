using UnityEngine;
using VoxelUtils;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;
using System;
using UnityEngine.Profiling;
using System.Threading.Tasks;
using System.Collections;

namespace World
{
    public struct Chunk
    {
        private WorldManager _ChunksManager => WorldManager.Instance;
        private GameConfig _GameConfig => GameConfig.Instance;

        public Chunk(Vector3Int ID)
        {
            m_ChunkID = ID;
            m_VoxelMap = new VoxelMap();
            m_ChunkState = eChunkState.Active;

            m_ChunkMesh = null;
            m_Job = default;
            m_JobHandle = default;

        }

        #region Callbacks
        private void onMeshReady()
        {
            m_JobHandle.Complete();

            if (m_Job.Vertices.Length == 0)
            {
                m_Job.Dispose();
                return;
            }

            if (m_ChunkMesh == null)
                m_ChunkMesh = ChunkMeshPool.s_Instance.DeQueue();

            Mesh mesh = new Mesh();
            mesh.vertices = m_Job.Vertices.ToArrayNBC();
            mesh.triangles = m_Job.Triangles.ToArrayNBC();
            mesh.SetUVs(0, m_Job.UVs.ToArrayNBC());
            m_Job.Dispose();
            mesh.RecalculateNormals();

            m_ChunkMesh.Initialize(this, mesh);
        }
        #endregion

        public Vector3 WorldPosition => m_ChunkID * ChunkConfiguration.KeyToWorld;
        public Vector3Int ChunkID => m_ChunkID;
        public eChunkState ChunkState => m_ChunkState;

        private Vector3Int m_ChunkID;
        private VoxelMap m_VoxelMap;
        private eChunkState m_ChunkState;

        private ChunkMesh m_ChunkMesh;

        public byte GetVoxel(Vector3Int voxelPosition) => m_VoxelMap.GetVoxel(voxelPosition);
        public void SetVoxel(Vector3Int voxelPosition) => m_VoxelMap.SetVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, 1);

        public void SetVoxelMap(byte[] flatVoxelMap) => m_VoxelMap.SetFlatMap(flatVoxelMap);

        private IChunkMesh m_Job;
        private JobHandle m_JobHandle;

        private async void checkMeshCompletition()
        {
            do
                await Task.Yield();
            while (!m_JobHandle.IsCompleted);
            onMeshReady();
        }

        public void RequestMesh()
        {
            if (m_VoxelMap.FlatMap.Length == 1)
                return;

            m_ChunkState = eChunkState.Drawn;

            _ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.right, out Chunk rightChunk);
            _ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.left, out Chunk leftChunk);
            _ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.forward, out Chunk frontChunk);
            _ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.back, out Chunk backChunk);

            m_Job = new IChunkMesh(ChunkID, m_VoxelMap.FlatMap,
                rightChunk.m_VoxelMap.FlatMap,
                leftChunk.m_VoxelMap.FlatMap,
                frontChunk.m_VoxelMap.FlatMap,
                backChunk.m_VoxelMap.FlatMap);

            m_JobHandle = m_Job.Schedule();
            checkMeshCompletition();
            //onMeshReady();
        }
    }
}
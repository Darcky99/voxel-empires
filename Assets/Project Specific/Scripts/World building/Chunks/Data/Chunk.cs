using UnityEngine;
using VoxelUtils;
using Project.Managers;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;
using System;
using UnityEngine.Profiling;
using System.Threading.Tasks;
using System.Collections;

namespace Chunks
{
    public struct Chunk
    {
        private ChunksManager m_ChunksManager => ChunksManager.Instance;
        private GameConfig m_GameConfig => GameConfig.Instance;

        public Chunk(Vector3Int ID)
        {
            m_ChunkID = ID;
            m_VoxelMap = new VoxelMap(GameConfig.Instance.ChunkConfiguration.ChunkSize);
            m_ChunkState = eChunkState.Active;

            m_ChunkMesh = null;
            m_Job = default;
            m_JobHandle = default;

        }

        #region Editor
        public void OnDrawGizmos()
        {
            int chunkSize = GameConfig.Instance.ChunkConfiguration.ChunkSize;
            Vector3 offset = new Vector3(m_ChunkID.x, m_ChunkID.y, m_ChunkID.z) * chunkSize * .5f;

            for (int y = 0; y < chunkSize; y++)
                for (int z = 0; z < chunkSize; z++)
                    for (int x = 0; x < chunkSize; x++)
                    {
                        Vector3Int position = new Vector3Int(x, y, z);
                        byte blockID = GetVoxel(position);

                        if (blockID == 0)
                            continue;

                        position -= Vector3Int.one;
                        Vector3 voxelCenter = new Vector3(position.x * 0.5f, position.y * 0.5f, position.z * 0.5f);
                        position += Vector3Int.one;

                        Gizmos.DrawWireCube(offset + voxelCenter, Vector3.one * 0.5f);
                    }
        }
        #endregion

        #region Callbacks
        private void onDraw(Vector3 cameraPosition, float maxDistance)
        {
            if (m_ChunkState == eChunkState.Drawn)
                return;
            float distanceToCamera = Vector3.Distance(cameraPosition, WorldPosition);
            if (distanceToCamera < maxDistance)
                RequestMesh();
        }
        private void onMeshReady()
        {
            m_JobHandle.Complete();

            //Debug.Log($"done {m_Job.Vertices.Length} ");

            if (m_Job.Vertices.Length == 0) {
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
        private ChunkMesh m_ChunkMesh;
        private eChunkState m_ChunkState;

        public byte GetVoxel(Vector3Int voxelPosition) => m_VoxelMap.GetVoxel(voxelPosition);
        public void SetVoxel(Vector3Int voxelPosition) => m_VoxelMap.SetVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, 1);

        public void EnableSimulation(bool state)
        {
            //in the future I will use this to enable simulation
            ChunkDrawer.OnDraw += onDraw;
        }
        public void SetVoxelMap(byte[] flatVoxelMap)
        {
            m_VoxelMap.SetFlatMap(flatVoxelMap);
        }

        public byte[] Get_Expanded_VoxelMap()
        {
            int expanded_ChunkSizeMaxIndex = GameConfig.Instance.ChunkConfiguration.Expanded_ChunkSize - 1;
            byte[] flatMap = m_VoxelMap.Expanded_FlatMap;
            if (flatMap.Length == 1)
                return flatMap;
            for (int y = 0; y <= expanded_ChunkSizeMaxIndex; y++)
                for (int z = 0; z <= expanded_ChunkSizeMaxIndex; z++)
                    for (int x = 0; x <= expanded_ChunkSizeMaxIndex; x++)
                    {
                        if (!(x == 0 || x == expanded_ChunkSizeMaxIndex || y == 0 || y == expanded_ChunkSizeMaxIndex || z == 0 || z == expanded_ChunkSizeMaxIndex))
                            continue;

                        Vector3Int adjacentChunkID = default, adjacentVoxel = default;

                        adjacentChunkID.x = x == 0 ? m_ChunkID.x - 1 : x == 17 ? m_ChunkID.x + 1 : m_ChunkID.x;
                        adjacentChunkID.y = y == 0 ? m_ChunkID.y - 1 : y == 17 ? m_ChunkID.y + 1 : m_ChunkID.y;
                        adjacentChunkID.z = z == 0 ? m_ChunkID.z - 1 : z == 17 ? m_ChunkID.z + 1 : m_ChunkID.z;

                        bool exists = m_ChunksManager.TryGetChunk(adjacentChunkID, out Chunk adjacentChunk);

                        if (!exists)
                        {
                            flatMap[Voxels.Expanted_Index(x, y, z)] = 0;
                            continue;
                        }

                        adjacentVoxel.x = x == 0 ? 15 : x == 17 ? 0 : x - 1;
                        adjacentVoxel.y = y == 0 ? 15 : y == 17 ? 0 : y - 1;
                        adjacentVoxel.z = z == 0 ? 15 : z == 17 ? 0 : z - 1;

                        flatMap[Voxels.Expanted_Index(x, y, z)] = adjacentChunk.GetVoxel(adjacentVoxel);
                    }

            return flatMap;
        }

        IChunkMesh m_Job;
        JobHandle m_JobHandle;

        async void check()
        {
            do
            {
                await Task.Yield();
            }
            while (!m_JobHandle.IsCompleted);
            onMeshReady();
        }

        public void RequestMesh()
        {
            m_ChunkState = eChunkState.Drawn;

            m_ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.up, out Chunk topChunk);
            m_ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.down, out Chunk downChunk);
            m_ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.right, out Chunk rightChunk);
            m_ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.left, out Chunk leftChunk);
            m_ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.forward, out Chunk frontChunk);
            m_ChunksManager.TryGetChunk(m_ChunkID + Vector3Int.back, out Chunk backChunk);

            m_Job = new IChunkMesh(ChunkID, m_VoxelMap.FlatMap, 
                topChunk.m_VoxelMap.FlatMap, 
                downChunk.m_VoxelMap.FlatMap,
                rightChunk.m_VoxelMap.FlatMap,
                leftChunk.m_VoxelMap.FlatMap,
                frontChunk.m_VoxelMap.FlatMap,
                backChunk.m_VoxelMap.FlatMap);

            m_JobHandle = m_Job.Schedule();
            check();
        }
    }
}
using UnityEngine;
using VoxelUtils;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;
using System;
using UnityEngine.Profiling;
using System.Threading.Tasks;
using System.Collections;
using Unity.Mathematics;

namespace World
{
    public struct Chunk
    {
        private WorldManager _ChunksManager => WorldManager.Instance;

        public Chunk(int3 ID)
        {
            _ChunkID = ID;
            _VoxelMap = new VoxelMap();
            _ChunkState = eChunkState.Active;
            _ChunkMesh = null;
            _Job = default;
            _JobHandle = default;
        }

        public float3 WorldPosition => _ChunkID * ChunkConfiguration.KeyToWorld;
        public int3 ChunkID => _ChunkID;
        public eChunkState ChunkState => _ChunkState;

        private int3 _ChunkID;
        private VoxelMap _VoxelMap;
        private eChunkState _ChunkState;

        private ChunkObject _ChunkMesh;

        public byte GetVoxel(Vector3Int voxelPosition) => _VoxelMap.GetVoxel(voxelPosition);
        public void SetVoxel(Vector3Int voxelPosition) => _VoxelMap.SetVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, 1);

        public void SetVoxelMap(byte[] flatVoxelMap) => _VoxelMap.SetFlatMap(flatVoxelMap);

        private IChunkMesh _Job;
        private JobHandle _JobHandle;

        #region Callbacks
        private void OnMeshReady()
        {
            _JobHandle.Complete();
            if (_Job.Vertices.Length == 0)
            {
                _Job.Dispose();
                return;
            }
            if (_ChunkMesh == null)
            {
                _ChunkMesh = ChunkMeshPool.s_Instance.DeQueue();
            }
            Mesh mesh = new Mesh();
            mesh.vertices = _Job.Vertices.ToArrayNBC();
            mesh.triangles = _Job.Triangles.ToArrayNBC();
            mesh.SetUVs(0, _Job.UVs.ToArrayNBC());
            _Job.Dispose();
            mesh.RecalculateNormals();
            _ChunkMesh.Set(this, mesh);
        }
        #endregion

        private async void CheckMeshCompletition()
        {
            do
            {
                await Task.Yield();
            }
            while (!_JobHandle.IsCompleted);
            OnMeshReady();
        }

        public void RequestMesh()
        {
            if (_VoxelMap.FlatMap.Length == 1)
            {
                return;
            }
            _ChunkState = eChunkState.Drawn;
            _ChunksManager.TryGetChunk(_ChunkID.Move(1, 0, 0), out Chunk rightChunk);
            _ChunksManager.TryGetChunk(_ChunkID.Move(-1, 0, 0), out Chunk leftChunk);
            _ChunksManager.TryGetChunk(_ChunkID.Move(0, 0, 1), out Chunk frontChunk);
            _ChunksManager.TryGetChunk(_ChunkID.Move(0, 0, -1), out Chunk backChunk);
            _Job = new IChunkMesh(_ChunkID, _VoxelMap.FlatMap,
                rightChunk._VoxelMap.FlatMap,
                leftChunk._VoxelMap.FlatMap,
                frontChunk._VoxelMap.FlatMap,
                backChunk._VoxelMap.FlatMap);
            _JobHandle = _Job.Schedule();
            CheckMeshCompletition();
        }
    }
}
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Stateless;
using System.Threading.Tasks;
using Unity.Jobs;
using System.Collections;
using System;
using Unity.Mathematics;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Collections;

namespace World
{
    public class WorldManager : Singleton<WorldManager>
    {
        public GameConfig _GameConfig => GameConfig.Instance;

        public event EventHandler<WorldState> StateChanged;

        public Dictionary<int3, ChunkObject> LoadedChunks => _LoadedChunks;
        public WorldState CurrentState => _fsm.State;

        private StateMachine<WorldState, WorldTrigger> _fsm;
        private Dictionary<int3, ChunkObject> _LoadedChunks;

        [Title("Configuration")]

        public void Initialize()
        {
            _LoadedChunks = new Dictionary<int3, ChunkObject>();

            _fsm = new StateMachine<WorldState, WorldTrigger>(WorldState.Idle);

            _fsm.Configure(WorldState.Idle)
            .Permit(WorldTrigger.Generate, WorldState.Generating);

            _fsm.Configure(WorldState.Generating)
            .Permit(WorldTrigger.Cancel, WorldState.Canceling)
            .Permit(WorldTrigger.GenerationFinished, WorldState.Idle);

            _fsm.Configure(WorldState.Canceling)
            .Permit(WorldTrigger.Generate, WorldState.Generating)
            .Permit(WorldTrigger.GenerationFinished, WorldState.Idle);
            _fsm.OnTransitionCompleted(WorldState_OnTransitionCompleted);

            _fsm.Fire(WorldTrigger.Generate);
        }

        private void WorldState_OnTransitionCompleted(StateMachine<WorldState, WorldTrigger>.Transition transition)
        {
            StateChanged?.Invoke(this, transition.Destination);
        }

        private ChunkObject CreateChunkObject(int3 chunkID)
        {
            ChunkObject chunkObject = ChunkObjectPool.s_Instance.DeQueue();
            chunkObject.Initialize(chunkID);
            _LoadedChunks.Add(chunkID, chunkObject);
            return chunkObject;
        }
        public bool TryGetChunkObject(int3 chunkID, out ChunkObject chunk)
        {
            return _LoadedChunks.TryGetValue(chunkID, out chunk); ;
        }
        public ChunkObject[] GetChunksObjects(NativeList<int3> chunkIDs)
        {
            ChunkObject[] chunks = new ChunkObject[chunkIDs.Length];
            for (int i = 0; i < chunkIDs.Length; i++)
            {
                TryGetChunkObject(chunkIDs[i], out chunks[i]);
            }
            return chunks;
        }

        public async UniTask LoadAll(NativeList<int3> toLoad)
        {
            int totalCount = toLoad.Length;
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(totalCount, Allocator.Persistent);
            NativeArray<ITerrainGeneration> terrainJobs = new NativeArray<ITerrainGeneration>(totalCount, Allocator.Persistent);
            for (int i = 0; i < totalCount; i++)
            {
                terrainJobs[i] = new ITerrainGeneration(toLoad[i]);
                JobHandle handler = terrainJobs[i].Schedule();
                jobHandles[i] = handler;
            }
            JobHandle combinedHandle = JobHandle.CombineDependencies(jobHandles);
            await UniTask.WaitUntil(() => combinedHandle.IsCompleted);
            combinedHandle.Complete();
            for (int i = 0; i < totalCount; i++)
            {
                ChunkObject chunkObj = CreateChunkObject(toLoad[i]);
                chunkObj.SetVoxels(terrainJobs[i].FlatVoxelMap);
            }
            jobHandles.Dispose();
            terrainJobs.Dispose();
        }
        public async UniTask DrawAll(NativeList<int3> toDraw)
        {
            int totalCount = toDraw.Length;
            ChunkObject[] chunks = GetChunksObjects(toDraw);
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(totalCount, Allocator.Persistent);
            NativeArray<IChunkMesh> meshJobs = new NativeArray<IChunkMesh>(totalCount, Allocator.Persistent);
            for (int i = 0; i < totalCount; i++)
            {
                int3 chunkID = toDraw[i];
                TryGetChunkObject(chunkID, out ChunkObject chunkObj);
                TryGetChunkObject(chunkID.Move(1, 0, 0), out ChunkObject rightChunkObj);
                TryGetChunkObject(chunkID.Move(-1, 0, 0), out ChunkObject leftChunkObj);
                TryGetChunkObject(chunkID.Move(0, 0, 1), out ChunkObject frontChunkObj);
                TryGetChunkObject(chunkID.Move(0, 0, -1), out ChunkObject backChunkObj);
                meshJobs[i] = new IChunkMesh(chunkID, chunkObj.Chunk.VoxelMap.FlatMap,
                    rightChunkObj.Chunk.VoxelMap.FlatMap,
                    leftChunkObj.Chunk.VoxelMap.FlatMap,
                    frontChunkObj.Chunk.VoxelMap.FlatMap,
                    backChunkObj.Chunk.VoxelMap.FlatMap);
                JobHandle handler = meshJobs[i].Schedule();
                jobHandles[i] = handler;
            }
            JobHandle combinedHandle = JobHandle.CombineDependencies(jobHandles);
            await UniTask.WaitUntil(() => combinedHandle.IsCompleted);
            combinedHandle.Complete();
            for (int i = 0; i < totalCount; i++)
            {
                chunks[i].SetMesh(meshJobs[i]);
            }
            jobHandles.Dispose();
            meshJobs.Dispose();
        }

        public void SetState(WorldTrigger trigger)
        {
            _fsm.Fire(trigger);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using Stateless;
using Unity.Jobs;
using System;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using VE.VoxelUtilities;
using VE.VoxelUtilities.Pooling;
using System.Threading;

namespace VE.World
{
    public class WorldManager : Singleton<WorldManager>
    {
        private GameConfig GameConfig => GameConfig.Instance;
        private GameManager GameManager => GameManager.Instance;

        public event EventHandler<WorldState> StateChanged;

        public Dictionary<int2, ChunkObject> LoadedChunks => _loadedChunks;
        public WorldState CurrentState => _fsm.State;

        private StateMachine<WorldState, WorldTrigger> _fsm;
        private Dictionary<int2, ChunkObject> _loadedChunks;

        public void Initialize()
        {
            _loadedChunks = new Dictionary<int2, ChunkObject>();

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

        private ChunkObject CreateChunkObject(int2 chunkID)
        {
            ChunkObject chunkObject = ChunkObjectPool.s_Instance.DeQueue();
            chunkObject.Initialize(chunkID);
            _loadedChunks.Add(chunkID, chunkObject);
            return chunkObject;
        }
        private (IChunkMesh, JobHandle) ScheduleDraw(int2 chunkID)
        {
            // float get_chunks_from_dictionary_time = Time.realtimeSinceStartup;
            NativeGrid<byte> builderHeightMap = JobHeightMap(chunkID);
            // float get_chunks_result_time = Time.realtimeSinceStartup - get_chunks_from_dictionary_time;
            // float time_previous_job_creation = Time.realtimeSinceStartup;
            IChunkMesh meshJob = new IChunkMesh(builderHeightMap);
            JobHandle handler = meshJob.Schedule();
            // float job_creation_time = Time.realtimeSinceStartup - time_previous_job_creation;
            // Debug.Log($"Individual schedules:\nGet neighbor: {get_chunks_result_time}\nJob creation and schedule: {job_creation_time}");
            return (meshJob, handler);
        }

        public void SetState(WorldTrigger trigger)
        {
            _fsm.Fire(trigger);
        }
        public bool TryGetChunkObject(int2 chunkID, out ChunkObject chunk)
        {
            return _loadedChunks.TryGetValue(chunkID, out chunk);
        }
        public ChunkObject[] GetChunksObjects(NativeList<int2> chunkIDs)
        {
            ChunkObject[] chunks = new ChunkObject[chunkIDs.Length];
            for (int i = 0; i < chunkIDs.Length; i++)
            {
                TryGetChunkObject(chunkIDs[i], out chunks[i]);
            }
            return chunks;
        }
        public NativeGrid<byte> JobHeightMap(int2 chunkID)
        {
            int3 size = GameConfig.ChunkConfiguration.ChunkSize;
            size.x += 2;
            size.z += 2;
            NativeGrid<byte> result = new NativeGrid<byte>(size, Allocator.Temp);
            TryGetChunkObject(chunkID, out ChunkObject centerChunkObj);
            TryGetChunkObject(chunkID.Move(1, 0), out ChunkObject rightChunkObj);
            TryGetChunkObject(chunkID.Move(-1, 0), out ChunkObject leftChunkObj);
            TryGetChunkObject(chunkID.Move(0, 1), out ChunkObject frontChunkObj);
            TryGetChunkObject(chunkID.Move(0, -1), out ChunkObject backChunkObj);
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    NativeGrid<byte> targetChunk = centerChunkObj.Chunk.HeightMap;
                    targetChunk = x < 1 ? leftChunkObj.Chunk.HeightMap : x == (size.x - 1) ? rightChunkObj.Chunk.HeightMap : targetChunk;
                    targetChunk = z < 1 ? backChunkObj.Chunk.HeightMap : z == (size.z - 1) ? frontChunkObj.Chunk.HeightMap : targetChunk;
                    int chunkX = x < 1 ? size.x - 3 : x == (size.x - 1) ? 0 : x - 1;
                    int chunkZ = z < 1 ? size.z - 3 : z == (size.z - 1) ? 0 : z - 1;
                    result.SetValue(x, 0, z, targetChunk.GetValue(chunkX, 0, chunkZ));
                }
            }
            return result;
        }
        
        public async UniTask LoadAll(NativeList<int2> toLoad, CancellationToken cancellationToken)
        {
            int totalCount = toLoad.Length;
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(totalCount, Allocator.Persistent);
            NativeArray<ITerrainGeneration> terrainJobs = new NativeArray<ITerrainGeneration>(totalCount, Allocator.Persistent);
            for (int i = 0; i < totalCount; i++)
            {
                terrainJobs[i] = new ITerrainGeneration(toLoad[i], GameConfig.ChunkConfiguration.ChunkSize);
                JobHandle handler = terrainJobs[i].Schedule();
                jobHandles[i] = handler;
            }
            JobHandle combinedHandle = JobHandle.CombineDependencies(jobHandles);
            await UniTask.WaitUntil(() => combinedHandle.IsCompleted, cancellationToken: cancellationToken);
            combinedHandle.Complete();
            for (int i = 0; i < totalCount; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                ChunkObject chunkObj = CreateChunkObject(toLoad[i]);
                chunkObj.SetHeightMap(terrainJobs[i].HeightMap);
            }
            jobHandles.Dispose();
            terrainJobs.Dispose();
        }
        public async UniTask DrawAll(NativeList<int2> toDraw, CancellationToken cancellationToken)
        {
            int totalCount = toDraw.Length;
            ChunkObject[] chunks = GetChunksObjects(toDraw);
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(totalCount, Allocator.Persistent);
            NativeArray<IChunkMesh> meshJobs = new NativeArray<IChunkMesh>(totalCount, Allocator.Persistent);
            float time_previous_job_cretion = Time.realtimeSinceStartup;
            for (int i = 0; i < totalCount; i++)
            {
                (meshJobs[i], jobHandles[i]) = ScheduleDraw(toDraw[i]);
            }
            JobHandle combinedHandle = JobHandle.CombineDependencies(jobHandles);
            JobHandle.ScheduleBatchedJobs();
            await UniTask.WaitUntil(() => combinedHandle.IsCompleted, cancellationToken: cancellationToken);
            combinedHandle.Complete();
            foreach (IChunkMesh meshJob in meshJobs)
            {
                meshJob.TempJobDispose();
            }
            Debug.Log("DrawAll Time: " + (Time.realtimeSinceStartup - time_previous_job_cretion));
            float time_previous_to_mesh_setting = Time.realtimeSinceStartup;

            for (int i = 0; i < totalCount; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                chunks[i].SetMesh(meshJobs[i]);
            }
            Debug.Log("Mesh setting time: " + (Time.realtimeSinceStartup - time_previous_to_mesh_setting));
            jobHandles.Dispose();
            meshJobs.Dispose();
        }
        public async UniTask LimitedDrawAll(NativeList<int2> toDraw, CancellationToken cancellationToken)
        {
            int totalCount = toDraw.Length;
            int sectionLenght = 16;
            ChunkObject[] chunks = GetChunksObjects(toDraw);
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(sectionLenght, Allocator.Persistent);
            NativeArray<IChunkMesh> meshJobs = new NativeArray<IChunkMesh>(sectionLenght, Allocator.Persistent);
            // float time_previous_job_cretion = Time.realtimeSinceStartup;
            for (int section = 0; section <= (totalCount / sectionLenght); section++)
            {
                int sectionAddition = section * sectionLenght;
                for (int i = 0; i < sectionLenght; i++)
                {
                    int absI = i + sectionAddition;
                    if (absI >= totalCount)
                    {
                        break;
                    }
                    (meshJobs[i], jobHandles[i]) = ScheduleDraw(toDraw[absI]);
                }
                // float result_job_scheduling_time = Time.realtimeSinceStartup - time_previous_job_cretion;
                // float time_previous_wait_completition = Time.realtimeSinceStartup;
                JobHandle combinedHandle = JobHandle.CombineDependencies(jobHandles);
                await UniTask.WaitUntil(() => combinedHandle.IsCompleted, cancellationToken: cancellationToken);
                combinedHandle.Complete();
                // float result_job_wait_time = Time.realtimeSinceStartup - time_previou s_wait_completition;
                // float time_previous_to_mesh_setting = Time.realtimeSinceStartup;
                for (int i = 0; i < sectionLenght; i++)
                {
                    int absI = i + sectionAddition;
                    if (absI >= totalCount)
                    {
                        break;
                    }
                    meshJobs[i].TempJobDispose();
                    chunks[i + sectionAddition].SetMesh(meshJobs[i]);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                // Debug.Log($"Job scheduling total time:{result_job_scheduling_time} \nJob wait time: {result_job_wait_time}\nMesh setting time: {Time.realtimeSinceStartup - time_previous_to_mesh_setting}"); //
            }
            jobHandles.Dispose();
            meshJobs.Dispose();
        }
    }
}
using System;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VE;

namespace World
{
    public class WorldController : MonoBehaviour
    {
        public GameManager _GameManager => GameManager.Instance;
        public GameConfig _GameConfig => GameConfig.Instance;

        private int2 _DrawOrigin;
        private bool _stopExpansiveLoadingFlag = false;

        [SerializeField] private WorldManager _WorldManager;
        [SerializeField] private Transform _CenterTransform;

        private void Awake()
        {
            Initialize();
        }
        private void FixedUpdate()
        {
            TryLoadingStart();
        }

        private void WorldManager_StateChanged(object sender, WorldState worldState)
        {
            switch (worldState)
            {
                case WorldState.Idle:
                    Idle();
                    break;
                case WorldState.Generating:
                    Generating();
                    break;
                case WorldState.Canceling:
                    Cancel();
                    break;
            }
        }

        private void Initialize()
        {
            _WorldManager.StateChanged += WorldManager_StateChanged;
        }

        private void Idle()
        {
            _stopExpansiveLoadingFlag = false;
        }
        private void Generating()
        {
            _ = ExpansiveLoading(_DrawOrigin);
        }
        private void Cancel()
        {
            _stopExpansiveLoadingFlag = true;
        }

        private void TryLoadingStart()
        {
            float3 center = _CenterTransform.position;
            int2 chunkID = ChunkUtils.WorldCoordinatesToChunkIndex(center);
            if (_DrawOrigin.Equals(chunkID))
            {
                return;
            }
            _DrawOrigin = chunkID;
            if (_WorldManager.CurrentState == WorldState.Idle)
            {
                _WorldManager.SetState(WorldTrigger.Generate);
            }
            else if (_WorldManager.CurrentState == WorldState.Generating)
            {
                _WorldManager.SetState(WorldTrigger.Cancel);
            }
        }

        private async UniTask ExpansiveLoading(int2 origin)
        {
            for (int r = 0; r < _GameConfig.GraphicsConfiguration.RenderDistance; r++)
            {
                GetChunksByRingJob chunks_to_draw_Job = new GetChunksByRingJob(origin, r);
                JobHandle chunks_to_draw_Handler = chunks_to_draw_Job.Schedule();
                await UniTask.WaitUntil(() => chunks_to_draw_Handler.IsCompleted);
                chunks_to_draw_Handler.Complete();
                if (HasMesh(chunks_to_draw_Job.ChunksInRing))
                {
                    chunks_to_draw_Job.Dispose();
                    continue;
                }
                await GenerateRing(origin, r);
                if (_stopExpansiveLoadingFlag)
                {
                    _stopExpansiveLoadingFlag = false;
                    _WorldManager.SetState(WorldTrigger.Generate);
                    return;
                }
            }
            _WorldManager.SetState(WorldTrigger.GenerationFinished);
        }
        private async UniTask GenerateRing(int2 origin, int ring)
        {
            GetChunksByRingJob chunks_to_load_Job = new GetChunksByRingJob(origin, ring);
            JobHandle chunks_to_load_Handler = chunks_to_load_Job.Schedule();

            GetChunksByRingJob chunks_to_preload_Job = new GetChunksByRingJob(origin, ring + 1);
            JobHandle chunks_to_preload_Handler = chunks_to_preload_Job.Schedule();

            GetChunksByRingJob chunks_to_draw_Job = new GetChunksByRingJob(origin, ring);
            JobHandle chunks_to_draw_Handler = chunks_to_draw_Job.Schedule();

            await UniTask.WaitUntil(() => chunks_to_load_Handler.IsCompleted && chunks_to_draw_Handler.IsCompleted && chunks_to_preload_Handler.IsCompleted);
            chunks_to_load_Handler.Complete();
            chunks_to_draw_Handler.Complete();
            chunks_to_preload_Handler.Complete();

            NativeList<int2> generateTerrain = RemoveItemsFromList(chunks_to_load_Job.ChunksInRing, HasTerrain);
            NativeList<int2> generateAround = RemoveItemsFromList(chunks_to_preload_Job.ChunksInRing, HasTerrain);
            NativeList<int2> draw = RemoveItemsFromList(chunks_to_draw_Job.ChunksInRing, HasMesh);
            await _WorldManager.LoadAll(generateTerrain);
            await _WorldManager.LoadAll(generateAround);
            await _WorldManager.LimitedDrawAll(draw);
            generateTerrain.Dispose();
            generateAround.Dispose();
            draw.Dispose();
            // await _WorldManager.DrawAll(draw);
        }
        private NativeList<int2> RemoveItemsFromList(NativeList<int2> ids, Func<int2, bool> conditionToRemove)
        {
            NativeList<int2> result = new NativeList<int2>(Allocator.Persistent);
            foreach (int2 id in ids)
            {
                if (conditionToRemove(id))
                {
                    continue;
                }
                result.Add(id);
            }
            ids.Dispose();
            return result;
        }
        private bool HasTerrain(int2 id)
        {
            bool exists = _WorldManager.TryGetChunkObject(id, out ChunkObject chunkObject);
            return exists && chunkObject.HasTerrain;
        }
        private bool HasMesh(int2 id)
        {
            bool exists = _WorldManager.TryGetChunkObject(id, out ChunkObject chunkObject);
            return exists && chunkObject.HasMesh;
        }
        private bool HasMesh(NativeList<int2> ids)
        {
            foreach (int2 id in ids)
            {
                if (!HasMesh(id))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
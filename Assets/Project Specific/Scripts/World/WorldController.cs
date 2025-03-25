using System;
using Cysharp.Threading.Tasks;
using Unity.Collections;
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

        private async UniTask ExpansiveLoading(int2 originChunkID)
        {
            for (int i = 0; i < _GameConfig.GraphicsConfiguration.RenderDistance; i++)
            {
                await GenerateRing(originChunkID, i);
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
            NativeList<int2> generateTerrain = RemoveItemsFromList(ChunkUtils.GetChunksByRing(origin, ring), HasTerrain);
            NativeList<int2> generateAround = RemoveItemsFromList(ChunkUtils.GetChunksByRing(origin, ring + 1), HasTerrain);
            NativeList<int2> draw = RemoveItemsFromList(ChunkUtils.GetChunksByRing(origin, ring), HasMesh);
            await _WorldManager.LoadAll(generateTerrain);
            await _WorldManager.LoadAll(generateAround);
            await _WorldManager.DrawAll(draw);
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
    }
}
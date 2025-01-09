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
        private WorldManager _WorldManager => WorldManager.Instance;
        public GameConfig _GameConfig => GameConfig.Instance;

        private int3 _DrawOrigin;
        private bool _stopExpansiveLoadingFlag = false;

        [SerializeField] private Transform _CenterTransform;

        private void Awake()
        {
            _ = Initialize();
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

        private async UniTask Initialize()
        {
            await UniTask.WaitUntil(() => _GameManager != null);
            _WorldManager.StateChanged += WorldManager_StateChanged;
        }

        private void Idle()
        {
            _stopExpansiveLoadingFlag = false;
        }
        private void Generating()
        {
            _ = ExpansiveLoading(new int3(_DrawOrigin.x, _DrawOrigin.y, _DrawOrigin.z));
        }
        private void Cancel()
        {
            _stopExpansiveLoadingFlag = true;
        }

        private void TryLoadingStart()
        {
            float3 center = _CenterTransform.position;
            int3 chunkID = ChunkUtils.WorldCoordinatesToChunkIndex(center);
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

        private async UniTask ExpansiveLoading(int3 originChunkID)
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
        private async UniTask GenerateRing(int3 origin, int ring)
        {
            NativeList<int3> generateTerrain = RemoveItemsFromList(ChunkUtils.GetChunksByRing(origin, ring), HasTerrain);
            NativeList<int3> generateAround = RemoveItemsFromList(ChunkUtils.GetChunksByRing(origin, ring + 1), HasTerrain);
            NativeList<int3> draw = RemoveItemsFromList(ChunkUtils.GetChunksByRing(origin, ring), HasMesh);
            await _WorldManager.LoadAll(generateTerrain);
            await _WorldManager.LoadAll(generateAround);
            await _WorldManager.DrawAll(draw);
        }
        private NativeList<int3> RemoveItemsFromList(NativeList<int3> ids, Func<int3, bool> conditionToRemove)
        {
            NativeList<int3> result = new NativeList<int3>(Allocator.Persistent);
            foreach (int3 id in ids)
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
        private bool HasTerrain(int3 id)
        {
            bool exists = _WorldManager.TryGetChunkObject(id, out ChunkObject chunkObject);
            return exists && chunkObject.HasTerrain;
        }
        private bool HasMesh(int3 id)
        {
            bool exists = _WorldManager.TryGetChunkObject(id, out ChunkObject chunkObject);
            return exists && chunkObject.HasMesh;
        }
    }
}
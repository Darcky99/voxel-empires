using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using Unity.Burst;
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
        private CameraController _CameraController => CameraController.Instance;
        public GameConfig _GameConfig => GameConfig.Instance;

        private void Awake()
        {
            _ = Initialize();
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

        private Vector3Int _worldDrawOrigin;
        private bool _stopExpansiveLoadingFlag = false;
        private bool _loadingOngoing = false;

        private UniTask _checker;

        private async UniTask Initialize()
        {
            await UniTask.WaitUntil(() => _GameManager != null);
            _WorldManager.StateChanged += WorldManager_StateChanged;
            _CameraController.Move += CameraController_Move;
        }

        private void Idle()
        {
            _stopExpansiveLoadingFlag = false;
        }
        private void Generating()
        {
            _ = ExpansiveLoading(new int3(_worldDrawOrigin.x, _worldDrawOrigin.y, _worldDrawOrigin.z));
        }
        private void Cancel()
        {
            _stopExpansiveLoadingFlag = true;
        }

        private void CameraController_Move(object sender, EventArgs eventArgs)
        {
            Vector3 cameraPosition = CameraController.Instance.transform.position;
            Vector3Int chunkID = ChunkUtils.WorldCoordinatesToChunkIndex(cameraPosition);

            if (_worldDrawOrigin != chunkID && _WorldManager.CurrentState == WorldState.Generating)
            {
                _worldDrawOrigin = chunkID;
                _WorldManager.SetState(WorldTrigger.Cancel);
                return;
            }
            else if (_worldDrawOrigin != chunkID && _WorldManager.CurrentState == WorldState.Idle)
            {
                _worldDrawOrigin = chunkID;
                _WorldManager.SetState(WorldTrigger.Generate);
                return;
            }
        }

        private async UniTask ExpansiveLoading(float3 centerPosition)
        {
            for (int i = 0; i < _GameConfig.GraphicsConfiguration.RenderDistance; i++)
            {
                NativeList<int3> generateTerraint = RemoveChunkObjects(ChunkUtils.GetChunkByRing(centerPosition, i), HasTerrain);
                NativeList<int3> generateAround = RemoveChunkObjects(ChunkUtils.GetChunkByRing(centerPosition, i + 1), HasTerrain);
                NativeList<int3> draw = RemoveChunkObjects(ChunkUtils.GetChunkByRing(centerPosition, i), HasMesh);
                Debug.Log($"Center: {_worldDrawOrigin}, Ring: {i},\nGenerate: {generateAround.Length}, Around: {generateAround.Length}, Draw: {draw.Length}");
                await _WorldManager.LoadAll(generateTerraint);
                await _WorldManager.LoadAll(generateAround);
                await _WorldManager.DrawAll(draw);
                if (_stopExpansiveLoadingFlag)
                {
                    _stopExpansiveLoadingFlag = false;
                    _WorldManager.SetState(WorldTrigger.Generate);
                    return;
                }
            }
            _WorldManager.SetState(WorldTrigger.GenerationFinished);
        }
        private NativeList<int3> RemoveChunkObjects(NativeList<int3> ids, Func<int3, bool> conditionToRemove)
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
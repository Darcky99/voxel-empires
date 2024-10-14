using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                case WorldState.Waiting:
                    Wait();
                    break;
                case WorldState.Loading:
                    Loading();
                    break;
                case WorldState.Drawing:
                    Drawing();
                    break;
            }
        }

        private Vector3Int _worldChunkRenderingCenter;
        private bool _stopExpansiveLoadingFlag = false;
        private bool _loadingOngoing = false;

        private async UniTask Initialize()
        {
            await UniTask.WaitUntil(() => _GameManager != null);
            _WorldManager.StateChanged += WorldManager_StateChanged;
            //I should start loading some stuff here, maybe 8x8 chunks + 1 for checkin
            //The initial load is different to the constant world load.

            //After initial loading, we start to load based on movement.
            _CameraController.Move += CameraController_Move;
        }

        private async void CameraController_Move(object sender, EventArgs eventArgs)
        {
            Vector3 cameraPosition = CameraController.Instance.transform.position;
            Vector3Int chunkID = ChunkUtils.WorldCoordinatesToChunkIndex(cameraPosition);
            if (_worldChunkRenderingCenter == chunkID)
            {
                return;
            }
            _stopExpansiveLoadingFlag = true;
            await UniTask.WaitUntil(() => _loadingOngoing == false);
            _worldChunkRenderingCenter = chunkID;
            _stopExpansiveLoadingFlag = false;
            _ = ExpansiveLoading(new int3(chunkID.x, chunkID.y, chunkID.z));
        }


        private async UniTask ExpansiveLoading(float3 centerPosition)
        {
            _loadingOngoing = true;
            for (int i = 0; i < _GameConfig.GraphicsConfiguration.RenderDistance; i++)
            {
                NativeList<int3> chunkIDs = ChunkUtils.GetChunkByRing(centerPosition, i);
                NativeList<int3> aroundIDs = ChunkUtils.GetChunkByRing(centerPosition, i + 1);
                //1. Load these and the surounding.
                //2. Draw the ones in the inner ring.
            }
            _loadingOngoing = false;
        }

        //Each movement 


        //(Load OR Try to Load) certain chunks within a distance ring.
        //The onces that required loading, check if their adjacent chunks exist.
        //If so, send to draw.
        //Don't restart the process until everything's complete
    }
}
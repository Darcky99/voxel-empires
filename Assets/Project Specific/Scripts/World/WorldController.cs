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
        public GameConfig _GameConfig => GameConfig.Instance;

        //public static event Action OnTerrainDrawn;

        private bool _StarOverFlag;

        private void Awake()
        {
            _ = Initialize();
        }
        private async UniTask Initialize()
        {
            await UniTask.WaitUntil(() => _GameManager != null);
            _StarOverFlag = false;
            _WorldManager.StateChanged += WorldManager_StateChanged;
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

        private void Wait()
        {
            if (_GameManager.CurrentState == GameState.Startup)
            {
                NativeList<int3> chunks = ChunkUtils.GetChunksByCircle(new float3(0, 0, 0), 8);
                _ = _WorldManager.Load(chunks);
                return;
            }
            //Load the next ring
            //if there's nothing to load, suscribe to player movement, and check on move

        }
        private void Loading()
        {
            //Every x time, check for completition
        }
        private void Drawing()
        {
            throw new NotImplementedException();
        }
    }
}
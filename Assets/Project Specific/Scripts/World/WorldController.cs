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

        private UniTask _generateTask;

        private async UniTask Initialize()
        {
            await UniTask.WaitUntil(() => _GameManager != null);
            _WorldManager.StateChanged += WorldManager_StateChanged;
        }

        private void SentToLoad(NativeList<int3> chunks)
        {
            _generateTask = _WorldManager.Load(chunks);
        }

        private void Wait()
        {
            if (_GameManager.CurrentState == GameState.Startup)
            {
                SentToLoad(ChunkUtils.GetChunksByCircle(new float3(0, 0, 0), 8));
                return;
            }
            //Load the next ring.
            //if there's nothing to load, suscribe to player movement, and check on move
        }
        private async UniTask Loading()
        {
            await _generateTask;
            //here check for chunks loaded but not drawn, 
        }
        private void Drawing()
        {
            throw new NotImplementedException();
        }
    }
}
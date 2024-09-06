using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
                //load an area of chunks around.
                return;
            }
            //Load the next ring
            //if there's nothing to load, suscribe to player movement, and check on move
        }
        private void Loading()
        {
            //This means that we are already loading some chunks, check for the completition of the load, then
            throw new NotImplementedException();
        }
        private void Drawing()
        {
            throw new NotImplementedException();
        }

        
    }
}
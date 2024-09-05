using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace Chunks
{
    public class ChunksController : MonoBehaviour
    {
        //Uses methods inside loader and maybe other managers to determine the general behaviour of chunks.
        //might trigger certain states of chunksmanager

        private ChunksManager _ChunksManager => ChunksManager.Instance;
        public GameConfig _GameConfig => GameConfig.Instance;

        public ChunksController()
        {
            _ChunksToDraw = new List<Vector3Int>();
            _StarOverFlag = false;
        }

        public static event Action OnTerrainDrawn;

        private List<Vector3Int> _ChunksToDraw;
        private Task _CheckDraw;
        private bool _StarOverFlag;

        private async Task drawRenderArea()
        {
            float time = Time.realtimeSinceStartup;
            int renderDistance = _GameConfig.GraphicsConfiguration.RenderDistance;

            for (int i = 0; i <= renderDistance; i++)
            {
                _ChunksToDraw = ChunkUtils.GetChunkByRing(_ChunksManager.CameraPosition, i);
                if (_ChunksToDraw.Count == 0)
                    continue;

                for (int j = 0; j < _ChunksToDraw.Count; j++)
                {
                    Vector3Int key = _ChunksToDraw[j];
                    bool exists = _ChunksManager.TryGetChunk(key, out Chunk chunk);
                    if (exists && chunk.ChunkState != eChunkState.Drawn)
                        chunk.RequestMesh();
                    if (j != 0 && j % 30 == 0)
                        await Task.Yield();
                }
                _ChunksToDraw.Clear();

                if (_StarOverFlag)
                {
                    i = -1;
                    _StarOverFlag = false;
                }
            }
            OnTerrainDrawn.Invoke();
            Debug.Log($"Time to draw everything: {Time.realtimeSinceStartup - time}");
        }

        public void Initialize()
        {
            //here start a coroutine, check for player pos every x seconds

        }
        public void CheckToDraw()
        {
            if (_CheckDraw != null && !_CheckDraw.IsCompleted)
            {
                _StarOverFlag = true;
            }
            else
            {
                _CheckDraw = drawRenderArea();
            }
        }
    }
}
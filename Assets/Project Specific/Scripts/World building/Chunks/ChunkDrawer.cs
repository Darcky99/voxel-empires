using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Chunks
{
    public class ChunkDrawer
    {
        public GameConfig m_GameConfig => GameConfig.Instance;

        public ChunkDrawer(ChunkLoader chunkLoader)
        {
            m_ChunkLoader = chunkLoader;
        }

        private ChunkLoader m_ChunkLoader;

        private async void checkToDraw()
        {
            List<Vector3Int> chunksToDraw = new List<Vector3Int>();
            while (Application.isPlaying)
            {
                await Task.Delay(500);
                float time = Time.realtimeSinceStartup;
                for (int i = 1; i <= m_GameConfig.GraphicsConfiguration.RenderDistance; i++)
                {
                    //I need to get the chunks once and then filter by distance....
                    //getting chunks 16 times in a frame seems to be too much.

                    chunksToDraw = m_ChunkLoader.GetChunksByDistance(i, (chunkID) => 
                    m_ChunkLoader.LoadedChunks.ContainsKey(chunkID) && m_ChunkLoader.LoadedChunks[chunkID].ChunkState != eChunkState.Drawn);
                    if (chunksToDraw.Count != 0)
                        break;
                    if(i != 0 && i % 8 == 0)
                        await Task.Yield();
                }
                Debug.Log($"Time: {Time.realtimeSinceStartup - time}");

                for (int i = 0; i < chunksToDraw.Count; i++)
                {
                    Vector3Int key = chunksToDraw[i];
                    m_ChunkLoader.GetChunk(key).DrawMesh();
                    if(i != 0 && i % 10 == 0)
                        await Task.Yield();
                }
                chunksToDraw.Clear();
            }
        }

        public void CheckToDraw() => checkToDraw();
    }
}
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
                for (int i = 1; i <= m_GameConfig.GraphicsConfiguration.RenderDistance; i++)
                {
                    chunksToDraw = m_ChunkLoader.GetChunksByDistance(i, (chunkID) => {
                        bool exist = m_ChunkLoader.LoadedChunks.TryGetValue(chunkID, out Chunk chunk);
                        return exist && chunk.ChunkState != eChunkState.Drawn;
                    });
                    if (chunksToDraw.Count != 0)
                        break;
                    await Task.Yield();
                }
                for (int i = 0; i < chunksToDraw.Count; i++)
                {
                    Vector3Int key = chunksToDraw[i];
                            m_ChunkLoader.GetChunk(key).DrawMesh();
                    if(i != 0 && i % 10 == 0)
                        await Task.Yield();
                }
                if(chunksToDraw.Count == 0)
                    await Task.Delay(500);
                else
                    chunksToDraw.Clear();
            }
        }

        public void CheckToDraw() => checkToDraw();
    }
}
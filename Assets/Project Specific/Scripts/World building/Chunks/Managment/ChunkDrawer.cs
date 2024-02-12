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
            m_ChunksToDraw = new List<Vector3Int>();
            m_StarOverFlag = false;

            PlayerCamera.OnCameraMove += CheckToDraw;
        }
        ~ChunkDrawer() => PlayerCamera.OnCameraMove -= CheckToDraw;

        private ChunkLoader m_ChunkLoader;
        private List<Vector3Int> m_ChunksToDraw;
        private Task m_CheckDraw;
        private bool m_StarOverFlag;

        private async Task checkToDraw()
        {
            int renderDistance = m_GameConfig.GraphicsConfiguration.RenderDistance;

            for (int i = 0; i <= renderDistance; i++)
            {
                m_ChunksToDraw = m_ChunkLoader.GetChunkByRing(i, (chunkID) => {
                    bool exist = m_ChunkLoader.LoadedChunks.TryGetValue(chunkID, out Chunk chunk);
                    return exist && chunk.ChunkState != eChunkState.Drawn;
                });
                if (m_ChunksToDraw.Count == 0)
                    continue;

                for (int j = 0; j < m_ChunksToDraw.Count; j++) {
                    Vector3Int key = m_ChunksToDraw[j];
                    m_ChunkLoader.GetChunk(key).DrawMesh();
                    
                    if (j != 0 && j % 35 == 0)
                        await Task.Yield();
                }
                m_ChunksToDraw.Clear();

                if (m_StarOverFlag) {
                    i = -1;
                    m_StarOverFlag = false;
                }
            }
            
        }



        public void CheckToDraw() 
        {
            if(m_CheckDraw != null && !m_CheckDraw.IsCompleted)
              m_StarOverFlag = true;
            else
              m_CheckDraw = checkToDraw();
        } 
    }
}
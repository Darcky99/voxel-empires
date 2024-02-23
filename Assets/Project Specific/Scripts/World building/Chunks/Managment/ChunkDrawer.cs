using Project.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace Chunks
{
    public class ChunkDrawer
    {
        private ChunksManager m_ChunksManager => ChunksManager.Instance;
        public GameConfig m_GameConfig => GameConfig.Instance;

        public ChunkDrawer()
        {
            m_ChunksToDraw = new List<Vector3Int>();
            m_StarOverFlag = false;
        }

        private List<Vector3Int> m_ChunksToDraw;
        private Task m_CheckDraw;
        private bool m_StarOverFlag;

        private async Task drawAll()
        {
            float time = Time.realtimeSinceStartup;
            int renderDistance = m_GameConfig.GraphicsConfiguration.RenderDistance;

            for (int i = 0; i <= renderDistance; i++)
            {
                m_ChunksToDraw = m_ChunksManager.GetChunkByRing(i);
                if (m_ChunksToDraw.Count == 0)
                    continue;

                for (int j = 0; j < m_ChunksToDraw.Count; j++)
                {
                    Vector3Int key = m_ChunksToDraw[j];
                    bool exists = m_ChunksManager.TryGetChunk(key, out Chunk chunk);
                    if (exists && chunk.ChunkState != eChunkState.Drawn)
                        chunk.RequestMesh();
                    if (j != 0 && j % 30 == 0)
                        await Task.Yield();
                }
                m_ChunksToDraw.Clear();

                if (m_StarOverFlag)
                {
                    i = -1;
                    m_StarOverFlag = false;
                }
            }
            Debug.Log($"Time to draw everything: {Time.realtimeSinceStartup -  time}");
        }

        public void CheckToDraw()
        {
            if (m_CheckDraw != null && !m_CheckDraw.IsCompleted)
                m_StarOverFlag = true;
            else
                m_CheckDraw = drawAll();
        }
    }
}
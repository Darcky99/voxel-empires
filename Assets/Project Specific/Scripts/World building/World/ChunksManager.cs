using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System;
using Unity.Collections.NotBurstCompatible;
using Chunks;

namespace Project.Managers
{
    public class ChunksManager : Singleton<ChunksManager>
    {
        public GameConfig m_GameConfig => GameConfig.Instance;

        #region Editor
        private void OnDrawGizmos()
        {
            if (DrawGizmos == false || LoadedChunks == null)
                return;

            foreach (Chunk chunk in LoadedChunks.Values)
                chunk.OnDrawGizmos();
        }
        #endregion

        #region Unity
        protected override void OnAwakeEvent()
        {
            m_ChunkDrawer = new ChunkDrawer(m_ChunkLoader);
            m_DrawnChunks = new Dictionary<Vector3Int, Chunk>();
        }
        public override void Start()
        {
            //buildRegion();
        }
        #endregion

        [SerializeField] private bool DrawGizmos = false;

        private Dictionary<Vector3Int, Chunk> LoadedChunks => m_ChunkLoader.LoadedChunks;
        private Dictionary<Vector3Int, Chunk> m_DrawnChunks;

        [SerializeField] private ChunkLoader m_ChunkLoader = new ChunkLoader();
        private ChunkDrawer m_ChunkDrawer;

        
        [Button]
        private async void Load_and_Draw_World()
        {
            await m_ChunkLoader.Load(GetChunksByDistance(m_GameConfig.GraphicsConfiguration.RenderDistance + 1,
                (chunkID) => (!LoadedChunks.ContainsKey(chunkID) && !m_DrawnChunks.ContainsKey(chunkID))));

            m_ChunkDrawer.CheckToDraw();
        }

        public Chunk GetChunk(Vector3Int chunkID) => m_ChunkLoader.GetChunk(chunkID);
        private Vector3Int worldCoordinatesToChunkIndex(Vector3 worldPosition) =>
            m_ChunkLoader.WorldCoordinatesToChunkIndex(worldPosition);
        public List<Vector3Int> GetChunksByDistance(int renderDistance, Func<Vector3Int, bool> condition) =>
            m_ChunkLoader.GetChunksByDistance(renderDistance, condition);

    }
}
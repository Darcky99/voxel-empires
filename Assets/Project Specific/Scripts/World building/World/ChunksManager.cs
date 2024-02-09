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
            m_ChunkLoader = new ChunkLoader();
            m_ChunkDrawer = new ChunkDrawer(m_ChunkLoader);
        }
        public override void Start()
        {
            base.Start();
            Load_and_Draw_World();
        }
        #endregion

        [Title("Configuration")]
        public Vector3 WorldCenter => m_WorldCenter.position;

        [SerializeField] private bool DrawGizmos = false;
        [SerializeField] private Transform m_WorldCenter;

        [Title("Handlers")]
        private Dictionary<Vector3Int, Chunk> LoadedChunks => m_ChunkLoader.LoadedChunks;

        private ChunkLoader m_ChunkLoader;
        private ChunkDrawer m_ChunkDrawer;

        
        //[Button]
        private async void Load_and_Draw_World()
        {
            await m_ChunkLoader.Load(GetChunksByDistance(m_GameConfig.WorldConfig.WorldSize,
                (chunkID) => (!LoadedChunks.ContainsKey(chunkID))));

            m_ChunkDrawer.CheckToDraw();
        }

        public Chunk GetChunk(Vector3Int chunkID) => m_ChunkLoader.GetChunk(chunkID);
        public Vector3Int WorldCoordinatesToChunkIndex(Vector3 worldPosition) =>
            m_ChunkLoader.WorldCoordinatesToChunkIndex(worldPosition);
        public List<Vector3Int> GetChunksByDistance(int renderDistance, Func<Vector3Int, bool> condition) =>
            m_ChunkLoader.GetChunksByDistance(renderDistance, condition);

    }
}
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
            _ChunkLoader = new ChunkLoader();
            _ChunkRenderer = new ChunkRenderer();
        }
        public override void Start()
        {
            base.Start();

            initialize();
        }
        #endregion

        [Title("Configuration")]
        public Vector3 CameraPosition => m_CameraTransform.position;

        [SerializeField] private bool DrawGizmos = false;
        [SerializeField] private Transform m_CameraTransform;

        [Title("Handlers")]
        private Dictionary<Vector3Int, Chunk> LoadedChunks => _ChunkLoader.LoadedChunks;

        private ChunkLoader _ChunkLoader;
        private ChunkRenderer _ChunkRenderer;

        private void initialize()
        {
            _ChunkLoader.Initialize();
            _ChunkRenderer.Initialize();
        }

        //private async void load_and_Draw_World()
        //{
        //    await _ChunkLoader.Load(ChunkUtils.GetChunksByDistance(m_CameraTransform.position, m_GameConfig.WorldConfiguration.WorldSizeInChunks,
        //        (chunkID) => (!LoadedChunks.ContainsKey(chunkID))));

        //    _ChunkRenderer.CheckToDraw();
        //}

        public bool TryGetChunk(Vector3Int chunkID, out Chunk chunk) => _ChunkLoader.TryGetChunk(chunkID, out chunk);
    }
}
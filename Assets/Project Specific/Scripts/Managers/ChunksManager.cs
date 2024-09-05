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


namespace Chunks
{
    public class ChunksManager : Singleton<ChunksManager>
    {
        //posible states:
        
        //Initialize => Initializing || Loads the first chunks 16*16 area
        //Draw       => Drawing      || Draws chunks by nearest
        //Load       => Loading      || Loads nearest missing chunks within limits
        //Wait       => Waiting      || Listen to player movement and wait until there's something to do 


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
            _ChunkLoader = new ChunksLoader();
            _ChunkRenderer = new ChunksController();
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

        private ChunksLoader _ChunkLoader;
        private ChunksController _ChunkRenderer;

        private void initialize()
        {
            //_ChunkLoader.Initialize();
            //_ChunkRenderer.Initialize();
            LoadAndDrawWorld();
        }

        private async void LoadAndDrawWorld()
        {
            await _ChunkLoader.Load(ChunkUtils.GetChunksByDistance(m_CameraTransform.position, m_GameConfig.WorldConfiguration.WorldSizeInChunks,
                (chunkID) => (!LoadedChunks.ContainsKey(chunkID))));

            _ChunkRenderer.CheckToDraw();
        }

        public bool TryGetChunk(Vector3Int chunkID, out Chunk chunk) => _ChunkLoader.TryGetChunk(chunkID, out chunk);
    }
}
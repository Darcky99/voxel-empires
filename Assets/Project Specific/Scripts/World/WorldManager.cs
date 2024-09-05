using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Stateless;
using System.Threading.Tasks;
using Unity.Jobs;
using System.Collections;

namespace World
{
    public class WorldManager : Singleton<WorldManager>
    {
        //posible states:

        //Initialize => Initializing || Loads the first chunks 16 * 16 area.
        //Draw       => Drawing      || Draws chunks by nearest.
        //Load       => Loading      || Loads nearest missing chunks within limits.
        //Wait       => Waiting      || Listen to player movement and wait until there's something to do.

        public GameConfig _GameConfig => GameConfig.Instance;

        public Dictionary<Vector3Int, Chunk> LoadedChunks => _LoadedChunks;
        public Vector3 CameraPosition => _CameraTransform.position;

        //private StateMachine<>
        private Dictionary<Vector3Int, Chunk> _LoadedChunks = new Dictionary<Vector3Int, Chunk>();

        [Title("Handlers")]
        [SerializeField] private WorldController _ChunkRenderer;

        [Title("Configuration")]
        [SerializeField] private Transform _CameraTransform;


        #region Unity
        protected override void OnAwakeEvent()
        {
        }
        public override void Start()
        {
            base.Start();
        }
        #endregion

        private void Initialize()
        {
            _LoadedChunks = new Dictionary<Vector3Int, Chunk>();
            LoadAndDrawWorld();
        }

        private async void LoadAndDrawWorld()
        {
            await Load(ChunkUtils.GetChunksByDistance(_CameraTransform.position, _GameConfig.WorldConfiguration.WorldSizeInChunks,
                (chunkID) => (!LoadedChunks.ContainsKey(chunkID))));
            _ChunkRenderer.CheckToDraw();
        }


        private async Task Load(List<Vector3Int> toLoad)
        {
            await GenerateAndAddChunks(toLoad);
            //here you could either load from disk or generate.
        }
        private async Task GenerateAndAddChunks(List<Vector3Int> toLoad)
        {
            for (int i = 0; i < toLoad.Count; i++)
            {
                ITerrainGeneration terrainJob = new ITerrainGeneration(toLoad[i]);
                JobHandle handler = terrainJob.Schedule();
                handler.Complete();

                Vector3Int id = toLoad[i];
                Chunk chunk = new Chunk(id);

                if (terrainJob.IsEmpty[0])
                    chunk.SetVoxelMap(new byte[] { 0 });
                else
                    chunk.SetVoxelMap(terrainJob.FlatVoxelMap.ToArray());

                _LoadedChunks[id] = chunk;
                terrainJob.Dispose();

                if (i % 60 == 0 && i != 0)
                    await Task.Yield();
            }
        }


        private IEnumerator loadLoop()
        {
            //constantly check the chunk origin position, 
            yield return null;
        }



        public bool TryGetChunk(Vector3Int chunkID, out Chunk chunk)
        {
            bool exists = LoadedChunks.TryGetValue(chunkID, out chunk);
            if (!exists)
            {
                chunk = new Chunk(chunkID);
                chunk.SetVoxelMap(new byte[] { 0 });
            }
            return exists;
        }
    }
}
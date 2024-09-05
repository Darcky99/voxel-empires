using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using System;
using UnityEngine.Profiling;
using System.Collections;

namespace Chunks
{
    [Serializable]
    public class ChunksLoader
    {
        //chunk loader it's just a factory class, it should only contain the methods to load stuff, and maybe keep track of loaded / unloaded chunks


        private GameConfig m_GameConfig => GameConfig.Instance;

        public ChunksLoader()
        {
            m_LoadedChunks = new Dictionary<Vector3Int, Chunk>();
        }

        public Dictionary<Vector3Int, Chunk> LoadedChunks => m_LoadedChunks;
        public Vector3 WorldCenter => ChunksManager.Instance.CameraPosition;

        private Dictionary<Vector3Int, Chunk> m_LoadedChunks = new Dictionary<Vector3Int, Chunk>();

        private async Task load(List<Vector3Int> toLoad)
        {
            await generateAndAddChunks(toLoad);
            //here you could either load from disk or generate.
        }
        private async Task generateAndAddChunks(List<Vector3Int> toLoad)
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

                m_LoadedChunks[id] = chunk;
                terrainJob.Dispose();

                if (i % 60 == 0 && i != 0)
                    await Task.Yield();
            }
        }

        private bool tryGetChunk(Vector3Int chunkID, out Chunk chunk)
        {
            bool exists = LoadedChunks.TryGetValue(chunkID, out chunk);
            if (!exists)
            {
                chunk = new Chunk(chunkID);
                chunk.SetVoxelMap(new byte[] { 0 });
            }
            return exists;
        }

        private IEnumerator loadLoop()
        {
            //constantly check the chunk origin position, 
            yield return null;
        }


        public void Initialize()
        {
            //every x frames check for pending loading, 
            //generate such chunks and continue.
        }
        public bool TryGetChunk(Vector3Int chunkID, out Chunk chunk) => tryGetChunk(chunkID, out chunk);
        public async Task Load(List<Vector3Int> toLoad) => await load(toLoad);
    }
}
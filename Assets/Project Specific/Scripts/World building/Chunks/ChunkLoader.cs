using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Project.Managers;
using Unity.Mathematics;
using System;

namespace Chunks
{
    [Serializable]
    public class ChunkLoader
    {
        public ChunkLoader()
        {
            m_LoadedChunks = new Dictionary<Vector3Int, Chunk>();
        }

        public Dictionary<Vector3Int, Chunk> LoadedChunks => m_LoadedChunks;

        private Dictionary<Vector3Int, Chunk> m_LoadedChunks;
        [SerializeField] private Transform m_WorldCenter;

        private async Task load(List<Vector3Int> toLoad)
        {
            await addNaturalTerrain(toLoad);
        }

        private async Task addNaturalTerrain(List<Vector3Int> toLoad)
        {
            float startTimeTerrainJobs = Time.realtimeSinceStartup;
            NativeList<TerrainGenerationJob> generationJobs = new NativeList<TerrainGenerationJob>(Allocator.Persistent);
            NativeList<JobHandle> handlers = new NativeList<JobHandle>(Allocator.Persistent);

            Debug.Log($"Started scheduling: {Time.realtimeSinceStartup - startTimeTerrainJobs}");
            for (int i = 0; i < toLoad.Count; i++)
            {
                Vector3Int id = toLoad[i];
                m_LoadedChunks.Add(id, new Chunk(id));
                generationJobs.Add(new TerrainGenerationJob(toLoad[i]));
                handlers.Add(generationJobs[i].Schedule(16 * 16 * 16, 64));
                if (i % 500 == 0)
                    await Task.Yield();
            }
            Debug.Log($"Finished scheduling");
            async Task Check()
            {
                bool allComplete = false;
                while (!allComplete)
                {
                    allComplete = true;

                    await Task.Delay(100);

                    foreach (var handler in handlers)
                        if (!handler.IsCompleted)
                            allComplete = false;

                    if (allComplete)
                        break;
                }
            }
            await Check();
            JobHandle.CompleteAll(handlers.AsArray());
            Debug.Log($"Time to generate terrain: {Time.realtimeSinceStartup - startTimeTerrainJobs}");
            for (int i = 0; i < toLoad.Count; i++)
            {
                m_LoadedChunks[toLoad[i]].SetVoxelMap(generationJobs[i].FlatVoxelMap.ToArray());
                generationJobs[i].FlatVoxelMap.Dispose();

                if (i % 100 == 0)
                    await Task.Yield();
            }
            generationJobs.Dispose();
            handlers.Dispose();
            Debug.Log($"Total time to assign terrain values: {Time.realtimeSinceStartup - startTimeTerrainJobs}");
        }

        private List<Vector3Int> getChunksByDistance(Vector3 worldPosition, int renderDistance, Func<Vector3Int, bool> condition)
        {
            Vector3Int center = worldCoordinatesToChunkIndex(worldPosition);

            int2 limitsX = new int2(center.x - renderDistance, center.x + renderDistance);
            int2 limitsY = new int2(0, 4);
            int2 limitsZ = new int2(center.z - renderDistance, center.z + renderDistance);

            List<Vector3Int> missingChunks = new List<Vector3Int>();
            Vector3Int pos = default;

            for (int x = limitsX.x; x <= limitsX.y; x++)
                for (int y = limitsY.x; y <= limitsY.y; y++)
                    for (int z = limitsZ.x; z <= limitsZ.y; z++)
                    {
                        pos.x = x; pos.y = y; pos.z = z;

                        if (condition(pos))
                            missingChunks.Add(pos);
                    }

            return missingChunks;
        }
        private Vector3Int worldCoordinatesToChunkIndex(Vector3 worldPosition)
        {
            Vector3 position = worldPosition + (Vector3.one * 0.25f);
            position /= 8;
            return new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));
        }
        private Chunk getChunk(Vector3Int chunkID)
        {
            if (LoadedChunks.ContainsKey(chunkID))
                return LoadedChunks[chunkID];
            else return null;
        }

        public Chunk GetChunk(Vector3Int chunkID) => getChunk(chunkID);
        public Vector3Int WorldCoordinatesToChunkIndex(Vector3 worldPosition) =>
            worldCoordinatesToChunkIndex(worldPosition);
        public List<Vector3Int> GetChunksByDistance(int renderDistance, Func<Vector3Int, bool> condition) =>
            getChunksByDistance(m_WorldCenter.position, renderDistance, condition);

        public async Task Load(List<Vector3Int> toLoad) => await load(toLoad);
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Project.Managers;
using Unity.Mathematics;
using System;
using System.Linq;

namespace Chunks
{
    [Serializable]
    public class ChunkLoader
    {
        private GameConfig m_GameConfig => GameConfig.Instance;

        public ChunkLoader()
        {
            m_LoadedChunks = new Dictionary<Vector3Int, Chunk>();
        }

        public Dictionary<Vector3Int, Chunk> LoadedChunks => m_LoadedChunks;
        public Vector3 WorldCenter => ChunksManager.Instance.WorldCenter;

        private Dictionary<Vector3Int, Chunk> m_LoadedChunks = new Dictionary<Vector3Int, Chunk>();
        

        private async Task load(List<Vector3Int> toLoad)
        {
            await addNaturalTerrain(toLoad);
        }

        private async Task addNaturalTerrain(List<Vector3Int> toLoad)
        {
            NativeList<ITerrainGeneration> generationJobs = new NativeList<ITerrainGeneration>(Allocator.Persistent);
            NativeList<JobHandle> handlers = new NativeList<JobHandle>(Allocator.Persistent);
            for (int i = 0; i < toLoad.Count; i++)
            {
                Vector3Int id = toLoad[i];
                m_LoadedChunks.Add(id, new Chunk(id));
                generationJobs.Add(new ITerrainGeneration(toLoad[i]));
                handlers.Add(generationJobs[i].Schedule(16 * 16 * 16, 64));
                if (i % 500 == 0)
                    await Task.Yield();
            }
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
            for (int i = 0; i < toLoad.Count; i++)
            {
                m_LoadedChunks[toLoad[i]].SetVoxelMap(generationJobs[i].FlatVoxelMap.ToArray());
                generationJobs[i].FlatVoxelMap.Dispose();

                if (i % 100 == 0)
                    await Task.Yield();
            }
            generationJobs.Dispose();
            handlers.Dispose();
        }

        #region Auxiliar
        private List<Vector3Int> getChunksByDistance(Vector3 worldPosition, int distance, Func<Vector3Int, bool> condition)
        {
            Vector3Int center = worldCoordinatesToChunkIndex(worldPosition);

            int2 x_limits = new int2(center.x - distance, center.x + distance);
            int2 y_limits = new int2(0, m_GameConfig.WorldConfig.WorldHeight);
            int2 z_limits = new int2(center.z - distance, center.z + distance);

            List<Vector3Int> missingChunks = new List<Vector3Int>();
            Vector3Int pos = default;

            int totalDistance = distance * 2 - 1;

            for (int x = x_limits.x; x <= x_limits.y; x++)
                for (int z = z_limits.x; z <= z_limits.y; z++)
                    for (int y = y_limits.x; y <= y_limits.y; y++)
                    {
                        pos.x = x; pos.y = y; pos.z = z;

                        if (condition(pos))
                            missingChunks.Add(pos);
                    }

            return missingChunks;
        }
        private List<Vector3Int> getChunkByRing(Vector3 worldPosition, int ring, Func<Vector3Int, bool> condition)
        {
            List<Vector3Int> missingChunks = new List<Vector3Int>();
            Vector3Int center = worldCoordinatesToChunkIndex(worldPosition);
            Vector3Int pos = default;

            int2 x_limits = new int2(center.x - ring, center.x + ring);
            int2 z_limits = new int2(center.z - ring, center.z + ring);
            int2 y_limits = new int2(0, m_GameConfig.WorldConfig.WorldHeight);

            Vector3Int pos1 = default;
            Vector3Int pos2 = default;

            for (int x = x_limits.x; x <= x_limits.y; x++)
                for (int y = y_limits.x; y <= y_limits.y; y++)
                {
                    pos1.x = x ; pos1.y = y; pos1.z = z_limits.x;
                    pos2.x = x ; pos2.y = y ; pos2.z = z_limits.y;

                    if (condition(pos1) && !missingChunks.Contains(pos1))
                        missingChunks.Add(pos1);
                    if (condition(pos2) && !missingChunks.Contains(pos2))
                        missingChunks.Add(pos2);
                }
            for (int z = z_limits.x + 1; z < z_limits.y; z++)
                for (int y = y_limits.x; y <= y_limits.y; y++)
                {
                    pos1.x = x_limits.x; pos1.y = y; pos1.z = z;
                    pos2.x = x_limits.y; pos2.y = y; pos2.z = z;

                    if (condition(pos1) && !missingChunks.Contains(pos1))
                        missingChunks.Add(pos1);
                    if (condition(pos2) && !missingChunks.Contains(pos2))
                        missingChunks.Add(pos2);
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
            getChunksByDistance(WorldCenter, renderDistance, condition);
        public List<Vector3Int> GetChunkByRing(int ring, Func<Vector3Int, bool> condition) =>
            getChunkByRing(WorldCenter, ring, condition);
        #endregion

        public async Task Load(List<Vector3Int> toLoad) => await load(toLoad);
    }
}
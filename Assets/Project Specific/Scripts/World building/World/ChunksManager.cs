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

namespace Chunks.Manager
{
    public class ChunksManager : Singleton<ChunksManager>
    {
        //Cargar los chunks
            //si es nuevo, genera terreno <---- Otra clase
            //si no, cargalo del disco <--- Otra clase
            
        //Como se carga
            //Por anillos 0 1 2 3 4 5 6 7 8 <--- otra clase? intercambiable?


        public GameConfig m_GameConfig => GameConfig.Instance;

        #region Editor
        private void OnDrawGizmos()
        {
            if (DrawGizmos == false || m_LoadedChunks == null)
                return;

            foreach (Chunk chunk in m_LoadedChunks.Values)
                chunk.OnDrawGizmos();
        }
        #endregion

        #region Unity
        protected override void OnAwakeEvent()
        {
            m_LoadedChunks = new Dictionary<int3, Chunk>();
            m_DrawnChunks = new Dictionary<int3, Chunk>();
        }
        public override void Start()
        {
            //buildRegion();
        }
        #endregion

        [SerializeField] private bool DrawGizmos = false;

        [SerializeField] private Transform m_WorldCenter;

        private Dictionary<int3, Chunk> m_LoadedChunks;
        private Dictionary<int3, Chunk> m_DrawnChunks;

        //No me gusta que este relog esté aquí. 
        private Coroutine m_Clock;
        private WaitForSeconds m_WaitForSeconds = new WaitForSeconds(2.5f);

        #region Not in use
        private IEnumerator clock()
        {
            while (true)
            {
                updateChunkMeshes();
                yield return m_WaitForSeconds;
            }
        }
        private void updateChunkMeshes()
        {
            //mesh sharing has noting to do with LODs.
            //mesh sharing it's calculated based on the LOD 0
        }
        #endregion

        [Button]
        private void buildRegion()
        {
            RunAsync();
        }
        private async Task RunAsync()
        {
            List<int3> chunksToLoad = null;
            List<int3> chunksToDraw = null;

            while (true)
            {
                for (int i = 1; i <= m_GameConfig.GraphicsConfiguration.RenderDistance; i++)
                {
                    chunksToLoad = getRegionChunks(i + 1, (chunkID) => (!m_LoadedChunks.ContainsKey(chunkID) && !m_DrawnChunks.ContainsKey(chunkID)));
                    chunksToDraw = getRegionChunks(i, (chunkID) => m_LoadedChunks.ContainsKey(chunkID) || chunksToLoad.Contains(chunkID));

                    if (chunksToDraw.Count != 0)
                        break;
                    //
                }
                if(chunksToLoad.Count != 0)
                {
                    await addNaturalTerrain(chunksToLoad); // the ones to generate terrain can't be already existing, so check both lists
                    await buidlMeshes(chunksToDraw); // the ones to build a mesh must be found inside the loaded chunk list
                }
                chunksToLoad.Clear();
                chunksToDraw.Clear();
                await Task.Delay(150);
            }
        }


        private async Task addNaturalTerrain(List<int3> regionChunkIDs)
        {
            float startTimeTerrainJobs = Time.realtimeSinceStartup;
            NativeList<TerrainGenerationJob> terrainJobs = new NativeList<TerrainGenerationJob>(Allocator.Persistent);
            NativeList<JobHandle> terrainJobHandler = new NativeList<JobHandle>(Allocator.Persistent);
            for (int i = 0; i < regionChunkIDs.Count; i++)
            {
                int3 currentID = regionChunkIDs[i];
                m_LoadedChunks.Add(currentID, new Chunk(currentID));
                terrainJobs.Add(new TerrainGenerationJob(currentID)); 
                terrainJobHandler.Add(terrainJobs[i].Schedule(16*16*16, 64));
                //if (i % 100 == 0)
                //    await Task.Yield();
            }
            async Task Check()
            {
                bool allComplete = false;
                while (!allComplete)
                {
                    allComplete = true;

                    await Task.Delay(10);
                    foreach (var handler in terrainJobHandler)
                        if (!handler.IsCompleted)
                            allComplete = false;

                    if (allComplete)
                        break;
                }
            }
            await Check();
            JobHandle.CompleteAll(terrainJobHandler.AsArray());
            Debug.Log($"Time to generate terrain: {Time.realtimeSinceStartup - startTimeTerrainJobs}");
            for (int i = 0; i < regionChunkIDs.Count; i++)
            {
                await m_LoadedChunks[regionChunkIDs[i]].SetVoxelMap(terrainJobs[i].FlatVoxelMap);
                //if (i % 10 == 0)
                //    await Task.Yield();
            }
            terrainJobs.Dispose();
            terrainJobHandler.Dispose();
            Debug.Log($"Total time to assign terrain values: {Time.realtimeSinceStartup - startTimeTerrainJobs}");
        }

        private async Task buidlMeshes(List<int3> regionChunkIDs)
        {
            float startTimeMeshJobs = Time.realtimeSinceStartup;
            NativeList<ChunkMeshJob> meshJobs = new NativeList<ChunkMeshJob>(Allocator.Persistent);
            NativeList<JobHandle> meshJobHandler = new NativeList<JobHandle>(Allocator.Persistent);
            for (int i = 0; i < regionChunkIDs.Count; i++)
            {
                Chunk chunk = m_LoadedChunks[regionChunkIDs[i]];
                meshJobs.Add(new ChunkMeshJob(chunk.GetVoxelMap()));
                meshJobHandler.Add(meshJobs[i].Schedule());

                if (i % 100 == 0)
                    await Task.Yield();
            }
            Debug.Log($"All mesh scheduled {Time.realtimeSinceStartup - startTimeMeshJobs}");
            async Task Check()
            {
                bool allComplete;
                while (true)
                {
                    allComplete = true;
                    await Task.Delay(250);
                    foreach (var handler in meshJobHandler)
                        if (!handler.IsCompleted)
                            allComplete = false;
                    if (allComplete)
                        break;
                }
            }
            await Task.Run(Check);
            JobHandle.CompleteAll(meshJobHandler.AsArray());

            Debug.Log($"Time generate meshes {Time.realtimeSinceStartup - startTimeMeshJobs}");
            for (int i = 0; i < regionChunkIDs.Count; i++)
            {
                int3 currentID = regionChunkIDs[i];
                await m_LoadedChunks[currentID].SetMesh(meshJobs[i].Vertices, meshJobs[i].Triangles, meshJobs[i].UVs);

                m_DrawnChunks.Add(currentID, m_LoadedChunks[currentID]);
                m_LoadedChunks.Remove(currentID);

                //meshJobs[i].Dispose();

                if (i % 50 == 0)
                    await Task.Yield();
            }
            meshJobs.Dispose();
            meshJobHandler.Dispose();
            Debug.Log($"Total time to assign meshes {Time.realtimeSinceStartup - startTimeMeshJobs}");
        }

        //private List<int3> unloadedRegionChunks(int renderDistance)
        //{

        //}
        private List<int3> getRegionChunks(int renderDistance, Func<int3, bool> condition)
        {
            int3 center = worldCoordinatesToChunkIndex(m_WorldCenter.position);

            //int renderDistance = m_GameConfig.GraphicsConfiguration.RenderDistance;
            //int halfRenderDistance = renderDistance / 2;

            //instead of half render distance, pass a distance

            int2 limitsX = new int2(center.x - renderDistance, center.x + renderDistance);
            int2 limitsY = new int2(0, 4);
            int2 limitsZ = new int2(center.z - renderDistance, center.z + renderDistance);

            List<int3> missingChunks = new List<int3>();

            int3 pos = default;

            //We don't actualy want to generate it based on the player position, but based in a noiser map (region's map).

            for (int x = limitsX.x; x <= limitsX.y; x++)
                for (int y = limitsY.x; y <= limitsY.y; y++)
                    for (int z = limitsZ.x; z <= limitsZ.y; z++)
                    {
                        pos.x = x; pos.y = y; pos.z = z;

                        //we dont care if it's in the loaded or not, we want the chunks 

                        if (condition(pos))
                            missingChunks.Add(pos);
                    }

            return missingChunks;
        }
        private int3 worldCoordinatesToChunkIndex(Vector3 worldPosition)
        {
            Vector3 position = worldPosition + (Vector3.one * 0.25f);
            position /= 8;
            return new int3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));
        }

        public Chunk GetChunk(int3 chunkID)
        {
            if (m_LoadedChunks.ContainsKey(chunkID))
                return m_LoadedChunks[chunkID];
            else if(m_DrawnChunks.ContainsKey(chunkID))
                return m_DrawnChunks[chunkID];
            return null;
        }

    }
}
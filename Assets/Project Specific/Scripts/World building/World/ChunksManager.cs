using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
//using WorldMeshBuilding;

public class ChunksManager : Singleton<ChunksManager>
{
    // It might be better to load by rings.
    // Check on each ring completition asyncronously.
    // Draw on completition.
    // Start the next ring.
    
    // OnCameraMove check rings from inside to outside.

    public GameConfig m_GameConfig => GameConfig.Instance;

    #region Unity
    protected override void OnAwakeEvent()
    {
        m_Chunks = new Dictionary<int3, Chunk>();
    }
    public override void Start()
    {
        //m_Clock = StartCoroutine(Clock());
        buildRegion();
    }
    #endregion

    [SerializeField] private Transform m_WorldCenter;
    private Dictionary<int3, Chunk> m_Chunks;

    private Coroutine m_Clock;
    private WaitForSeconds m_WaitForSeconds = new WaitForSeconds(2.5f);


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
    private void buildRegion()
    {
        List<int3> regionChunkIDs = getRegionChunks();

        if (regionChunkIDs.Count == 0)
            return;

        addNaturalTerrain(regionChunkIDs);
        addMeshes(regionChunkIDs);
    }

    private void addNaturalTerrain(List<int3> regionChunkIDs)
    {
        float startTimeTerrainJobs = Time.realtimeSinceStartup;
        
        NativeList<TerrainGenerationJob> terrainJobs = new NativeList<TerrainGenerationJob>(Allocator.Persistent);
        NativeList<JobHandle> terrainJobHandler = new NativeList<JobHandle>(Allocator.Persistent);

        for (int i = 0; i < regionChunkIDs.Count; i++)
        {
            int3 currentID = regionChunkIDs[i];

            m_Chunks.Add(currentID, new Chunk(currentID));

            terrainJobs.Add(new TerrainGenerationJob(currentID));
            terrainJobHandler.Add(terrainJobs[i].Schedule(ChunkConfiguration.FlatChunkLenght, 64));
        }
        
        JobHandle.CompleteAll(terrainJobHandler.AsArray());

        for (int i = 0; i < regionChunkIDs.Count; i++)
            m_Chunks[regionChunkIDs[i]].SetVoxelMap(terrainJobs[i].FlatVoxelMap);

        terrainJobs.Dispose();
        terrainJobHandler.Dispose();

        Debug.Log($"Time to generate terrain: {Time.realtimeSinceStartup - startTimeTerrainJobs}");
    }
    private void addMeshes(List<int3> regionChunkIDs)
    {
        float startTimeMeshJobs = Time.realtimeSinceStartup;

        NativeList<ChunkMeshJob> meshJobs = new NativeList<ChunkMeshJob>(Allocator.Persistent);
        NativeList<JobHandle> meshJobHandler = new NativeList<JobHandle>(Allocator.Persistent);

        for (int i = 0; i < regionChunkIDs.Count; i++)
        {
            Chunk centralChunk = m_Chunks[regionChunkIDs[i]];
            meshJobs.Add(new ChunkMeshJob(centralChunk.GetVoxelMap()));
            meshJobHandler.Add(meshJobs[i].Schedule());
        }
        JobHandle.CompleteAll(meshJobHandler.AsArray());

        //This passes the mesh data into some GO. One mesh per chunk.
        for (int i = 0; i < regionChunkIDs.Count; i++)
        {
            int3 currentID = regionChunkIDs[i];

            //I don't want to do it through the chunk.
            m_Chunks[currentID].SetMesh(meshJobs[i].Vertices, meshJobs[i].Triangles, meshJobs[i].UVs);

            //Ideally I would have a chunkMesh manager?
            //Somewhere I have to create constantly switch between max resolution and 

            meshJobs[i].Dispose();
        }
        meshJobs.Dispose();
        meshJobHandler.Dispose();

        Debug.Log($"Time to draw meshes {Time.realtimeSinceStartup - startTimeMeshJobs}");
    }

    private List<int3> getRegionChunks()
    {
        int3 center = worldCoordinatesToChunkIndex(m_WorldCenter.position);

        int renderDistance = m_GameConfig.GraphicsConfiguration.RenderDistance;
        int halfRenderDistance = renderDistance / 2;

        int2 limitsX = new int2(center.x - halfRenderDistance, center.x + halfRenderDistance);
        int2 limitsY = new int2(0, 6);
        int2 limitsZ = new int2(center.z - halfRenderDistance, center.z + halfRenderDistance);

        List<int3> missingChunks = new List<int3>();

        int3 pos = default;

        //We don't actualy want to generate it based on the player position, but based in a noiser map (region's map).

        for (int x = limitsX.x; x <= limitsX.y; x++)
            for(int y = limitsY.x; y <= limitsY.y; y++)
                for(int z = limitsZ.x; z <= limitsZ.y; z++)
                {
                    pos.x = x; pos.y = y; pos.z = z;

                    if (!m_Chunks.ContainsKey(pos))
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
        if (!m_Chunks.ContainsKey(chunkID))
            return null;

        return m_Chunks[chunkID];
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class ChunkUtils
{
    private static GameConfig s_GameConfig => GameConfig.Instance;

    public static List<Vector3Int> GetChunksByDistance(Vector3 worldPosition, int distance, Func<Vector3Int, bool> condition)
    {
        Vector3Int center = WorldCoordinatesToChunkIndex(worldPosition);
        int2 x_limits = new int2(center.x - distance, center.x + distance);
        int2 y_limits = new int2(0, s_GameConfig.WorldConfiguration.WorldHeightInChunks);
        int2 z_limits = new int2(center.z - distance, center.z + distance);

        List<Vector3Int> missingChunks = new List<Vector3Int>();
        Vector3Int pos = default;

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
    
    [BurstCompile]
    public static NativeList<int3> GetChunksByRing(int3 chunkID, int ring) //turn this into a job
    {
        NativeList<int3> chunksInRing = new NativeList<int3>(Allocator.Persistent);
        if (ring == 0)
        {
            chunksInRing.Add(chunkID);
            return chunksInRing;
        }
        int2 x_limits = new int2(chunkID.x - ring, chunkID.x + ring);
        int2 y_limits = new int2(0, s_GameConfig.WorldConfiguration.WorldHeightInChunks);
        int2 z_limits = new int2(chunkID.z - ring, chunkID.z + ring);
        int3 pos1 = default;
        int3 pos2 = default;

        for (int x = x_limits.x; x <= x_limits.y; x++)
            for (int y = y_limits.x; y <= y_limits.y; y++)
            {
                pos1.x = x; pos1.y = y; pos1.z = z_limits.x;
                pos2.x = x; pos2.y = y; pos2.z = z_limits.y;
                chunksInRing.Add(pos1);
                chunksInRing.Add(pos2);
            }
        for (int z = z_limits.x + 1; z < z_limits.y; z++)
            for (int y = y_limits.x; y <= y_limits.y; y++)
            {
                pos1.x = x_limits.x; pos1.y = y; pos1.z = z;
                pos2.x = x_limits.y; pos2.y = y; pos2.z = z;

                chunksInRing.Add(pos1);
                chunksInRing.Add(pos2);
            }
        return chunksInRing;
    }
    [BurstCompile]
    public static NativeList<int3> GetChunksByRing(float3 worldPosition, int ring)
    {
        int3 chunkID = WorldCoordinatesToChunkIndex(worldPosition);
        return GetChunksByRing(chunkID, ring);
    }
    [BurstCompile]
    public static NativeList<int3> GetChunksByCircle(float3 worldPosition, int radius)
    {
        int3 chunkID = WorldCoordinatesToChunkIndex(worldPosition);
        NativeList<int3> chunksInCircle = new NativeList<int3>(Allocator.Persistent);
        for (int ring = 0; ring <= radius; ring++)
        {
            NativeList<int3> chunksInRing = GetChunksByRing(chunkID, ring);
            for (int i = 0; i < chunksInRing.Length; i++)
            {
                if (!chunksInCircle.Contains(chunksInRing[i]))
                {
                    chunksInCircle.Add(chunksInRing[i]);
                }
            }
            chunksInRing.Dispose();
        }
        return chunksInCircle;
    }

    public static Vector3Int WorldCoordinatesToChunkIndex(Vector3 worldPosition)
    {
        Vector3 position = worldPosition + (Vector3.one * 0.25f);
        position.x /= (s_GameConfig.ChunkConfiguration.ChunkSize / 2);
        position.y /* / */= 0;//(s_GameConfig.ChunkConfiguration.ChunkHeight / 2);
        position.z /= (s_GameConfig.ChunkConfiguration.ChunkSize / 2);
        return new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));
    }
    public static int3 WorldCoordinatesToChunkIndex(float3 worldPosition)
    {
        float3 position = worldPosition + (new float3(1, 1, 1) * 0.25f);
        position.x /= (s_GameConfig.ChunkConfiguration.ChunkSize / 2);
        position.y /* / */= 0;//(s_GameConfig.ChunkConfiguration.ChunkHeight / 2);
        position.z /= (s_GameConfig.ChunkConfiguration.ChunkSize / 2);
        int x = (int)math.floor(position.x);
        int y = (int)math.floor(position.y);
        int z = (int)math.floor(position.z);
        return new int3(x, y, z);
    }

    public static float3 ChunkIDToWorldCoordinates(int3 chunkID) => (float3)chunkID * 16f / 2f;
}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
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
    public static List<Vector3Int> GetChunkByRing(Vector3 worldPosition, int ring)
    {
        Vector3Int center = WorldCoordinatesToChunkIndex(worldPosition);
        List<Vector3Int> chunksInRing = new List<Vector3Int>();
        int2 x_limits = new int2(center.x - ring, center.x + ring);
        int2 y_limits = new int2(0, s_GameConfig.WorldConfiguration.WorldHeightInChunks);
        int2 z_limits = new int2(center.z - ring, center.z + ring);

        Vector3Int pos1 = default;
        Vector3Int pos2 = default;

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
    public static Vector3Int WorldCoordinatesToChunkIndex(Vector3 worldPosition)
    {
        Vector3 position = worldPosition + (Vector3.one * 0.25f);
        position /= 8;
        return new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));
    }
}
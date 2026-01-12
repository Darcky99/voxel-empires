using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoxelEmpires.Configuration;

namespace VoxelEmpires.VoxelUtilities
{
    public static class ChunkUtils
    {
        private static GameConfig s_GameConfig => GameConfig.Instance;
        private static ChunkConfiguration ChunkConfiguration => s_GameConfig.ChunkConfiguration;

        public static List<Vector3Int> GetChunksByDistance(Vector3 worldPosition, int distance, Func<Vector3Int, bool> condition)
        {
            Vector2Int center = WorldCoordinatesToChunkIndex(worldPosition);
            int2 x_limits = new int2(center.x - distance, center.x + distance);
            int2 z_limits = new int2(center.y - distance, center.y + distance);
            List<Vector3Int> missingChunks = new List<Vector3Int>();
            Vector3Int pos = default;
            for (int x = x_limits.x; x <= x_limits.y; x++)
            {
                for (int z = z_limits.x; z <= z_limits.y; z++)
                {
                    pos.x = x; pos.y = z;
                    if (condition(pos))
                    {
                        missingChunks.Add(pos);
                    }
                }
            }
            return missingChunks;
        }

        [BurstCompile]
        public static NativeList<int2> GetChunksByRing(int2 chunkID, int ring) //turn this into a job
        {
            NativeList<int2> chunksInRing = new NativeList<int2>(Allocator.Persistent);
            if (ring == 0)
            {
                chunksInRing.Add(chunkID);
                return chunksInRing;
            }
            int2 x_limits = new int2(chunkID.x - ring, chunkID.x + ring);
            int2 z_limits = new int2(chunkID.y - ring, chunkID.y + ring);
            int2 pos1 = default;
            int2 pos2 = default;
            for (int x = x_limits.x; x <= x_limits.y; x++)
            {
                pos1.x = x; pos1.y = z_limits.x;
                pos2.x = x; pos2.y = z_limits.y;
                chunksInRing.Add(pos1);
                chunksInRing.Add(pos2);
            }
            for (int z = z_limits.x + 1; z < z_limits.y; z++)
            {
                pos1.x = x_limits.x; pos1.y = z;
                pos2.x = x_limits.y; pos2.y = z;
                chunksInRing.Add(pos1);
                chunksInRing.Add(pos2);
            }
            return chunksInRing;
        }

        /// <summary>
        /// //Block size '0.25' (which is contained within this method) should be a constant in GameConfig.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static Vector2Int WorldCoordinatesToChunkIndex(Vector3 worldPosition)
        {
            Vector3 adjustWorldPosition = worldPosition + (Vector3.one * 0.25f);
            Vector2 flatPosition = new Vector2(adjustWorldPosition.x, adjustWorldPosition.z);
            flatPosition.x /= (s_GameConfig.ChunkConfiguration.ChunkSize.x / 2);
            flatPosition.y /= (s_GameConfig.ChunkConfiguration.ChunkSize.z / 2);
            return new Vector2Int(Mathf.FloorToInt(flatPosition.x), Mathf.FloorToInt(flatPosition.y));
        }
        public static int2 WorldCoordinatesToChunkIndex(float3 worldPosition)
        {
            float3 position = worldPosition + (new float3(1, 1, 1) * 0.25f);
            position.x /= (s_GameConfig.ChunkConfiguration.ChunkSize.x / 2);
            position.z /= (s_GameConfig.ChunkConfiguration.ChunkSize.z / 2);
            int x = (int)math.floor(position.x);
            int z = (int)math.floor(position.z);
            return new int2(x, z);
        }

        public static float3 ChunkIDToWorldCoordinates(int2 chunkID)
        {
            return new float3(chunkID.x * ChunkConfiguration.ChunkSize.x, 0 * ChunkConfiguration.ChunkSize.y, chunkID.y * ChunkConfiguration.ChunkSize.z) / 2f;
        }
    }
}
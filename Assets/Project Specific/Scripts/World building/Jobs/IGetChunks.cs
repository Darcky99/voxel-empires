using Chunks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct IGetChunks : IJob
{
    //    public IGetChunks(Vector3Int central, int ring, Dictionary<Vector3Int, Chunk> chunks)
    //    {
    //        int resultCount = ring == 0 ? 1 : ring * 2 * 4;
    //        m_Ring = ring;
    //        m_Central = central;
    //        IDs = new NativeArray<Vector3Int>(resultCount, Allocator.Persistent);
    //        //m_Chunks = new NativeHashMap<Vector3Int, Chunk>(chunks, Allocator.TempJob);
    //    }

    //    public NativeArray<Vector3Int> IDs;

    //    private int m_Ring;
    //    private Vector3Int m_Central;
    //    private NativeHashMap<Vector3Int, Chunk> m_Chunks;

    public void Execute()
    {
    }
        //        int2 x_limits = new int2(m_Central.x - m_Ring, m_Central.x + m_Ring);
        //        int2 y_limits = new int2(0, 32);
        //        int2 z_limits = new int2(m_Central.z - m_Ring, m_Central.z + m_Ring);
        //        Vector3Int pos1 = default;
        //        Vector3Int pos2 = default;
        //        pos1.z = z_limits.x;
        //        pos2.z = z_limits.y;
        //        for (int x = x_limits.x; x <= x_limits.y; x++)
        //            for (int y = y_limits.x; y <= y_limits.y; y++)
        //            {
        //                pos1.x = x;
        //                pos1.y = y;
        //                if (condition(pos1))
        //                    missingChunks.Add(pos1);
        //                pos2.x = x;
        //                pos2.y = y;
        //                if (condition(pos2))
        //                    missingChunks.Add(pos2);
        //            }
        //        pos1.x = x_limits.x;
        //        pos2.x = x_limits.y;
        //        for (int z = z_limits.x + 1; z < z_limits.y; z++)
        //            for (int y = y_limits.x; y <= y_limits.y; y++)
        //            {
        //                pos1.y = y; pos1.z = z;
        //                if (condition(pos1))
        //                    missingChunks.Add(pos1);
        //                pos2.y = y; pos2.z = z;
        //                if (condition(pos2))
        //                    missingChunks.Add(pos2);
        //            }
        //    }

        //    private bool condition(chunkID)
        //    {
        //        bool exist = m_ChunkLoader.LoadedChunks.TryGetValue(chunkID, out Chunk chunk);
        //        return exist && chunk.ChunkState != eChunkState.Drawn;
        //    }
    }
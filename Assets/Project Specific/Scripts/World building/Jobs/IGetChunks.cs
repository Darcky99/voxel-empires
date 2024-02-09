using Chunks;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct IGetChunks : IJob
{
    //public IGetChunks(Dictionary<Vector3Int, Chunk> chunks)
    //{
    //    m_Chunks = new NativeHashMap<Vector3Int, Chunk>(chunks.Count, Allocator.TempJob);
    //}

    //private NativeHashMap<Vector3Int, Chunk> m_Chunks;

    public void Execute()
    {
        throw new System.NotImplementedException();
    }
}
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public struct GetChunksByRingJob : IJob
{
    public GetChunksByRingJob(int2 chunkID, int ring)
    {
        ChunkID = chunkID;
        Ring = ring;
        ChunksInRing = new NativeList<int2>(Allocator.Persistent);
    }

    public int2 ChunkID;
    public int Ring;
    public NativeList<int2> ChunksInRing;

    public void Execute()
    {
        if (Ring == 0)
        {
            ChunksInRing.Add(ChunkID);
            return;
        }
        int2 x_limits = new int2(ChunkID.x - Ring, ChunkID.x + Ring);
        int2 z_limits = new int2(ChunkID.y - Ring, ChunkID.y + Ring);
        int2 pos1 = default;
        int2 pos2 = default;
        for (int x = x_limits.x; x <= x_limits.y; x++)
        {
            pos1.x = x; pos1.y = z_limits.x;
            pos2.x = x; pos2.y = z_limits.y;
            ChunksInRing.Add(pos1);
            ChunksInRing.Add(pos2);
        }
        for (int z = z_limits.x + 1; z < z_limits.y; z++)
        {
            pos1.x = x_limits.x; pos1.y = z;
            pos2.x = x_limits.y; pos2.y = z;
            ChunksInRing.Add(pos1);
            ChunksInRing.Add(pos2);
        }
    }

    public void Dispose()
    {
        ChunksInRing.Dispose();
    }
}
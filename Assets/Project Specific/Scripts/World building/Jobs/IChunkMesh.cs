using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelUtils;

[BurstCompile]
public struct IChunkMesh : IJob
{
    public IChunkMesh(Vector3Int id, byte[] centralChunk, byte[] rightChunk, byte[] leftChunk, byte[] frontChunk, byte[] backChunk)
    {
        m_VoxelsConfig = new NativeArray<VoxelConfig>(GameConfig.Instance.VoxelConfiguration.GetVoxelsData(), Allocator.Persistent);
        m_ID = new int3(id.y, id.y, id.z);

        Vertices = new NativeList<Vector3>(Allocator.Persistent);
        Triangles = new NativeList<int>(Allocator.Persistent);
        UVs = new NativeList<Vector3>(Allocator.Persistent);

        m_DrawnFaces = new NativeHashMap<int3, FacesDrawn>(6000,Allocator.Persistent);
        m_Central_Chunk = new NativeArray<byte>(centralChunk, Allocator.Persistent);

        m_Right_Chunk = new NativeArray<byte>(rightChunk, Allocator.Persistent);
        m_Left_Chunk = new NativeArray<byte>(leftChunk, Allocator.Persistent);
        m_Front_Chunk = new NativeArray<byte>(frontChunk, Allocator.Persistent);
        m_Back_Chunk = new NativeArray<byte>(backChunk, Allocator.Persistent);
    }

    private NativeArray<VoxelConfig> m_VoxelsConfig;
    
    public NativeList<Vector3> Vertices { get; private set; }
    public NativeList<int> Triangles { get; private set; }
    public NativeList<Vector3> UVs { get; private set; }

    private readonly int3 m_ID;
    private readonly NativeArray<byte> m_Central_Chunk;

    private readonly NativeArray<byte> m_Right_Chunk;
    private readonly NativeArray<byte> m_Left_Chunk;
    private readonly NativeArray<byte> m_Front_Chunk;
    private readonly NativeArray<byte> m_Back_Chunk;

    private readonly NativeHashMap<int3, FacesDrawn> m_DrawnFaces;

    public void Execute()
    {
        if (m_Central_Chunk.Length == 1)
            return;

        NativeArray<int> d = new NativeArray<int>(3, Allocator.Temp);
        NativeArray<int> l = new NativeArray<int>(3, Allocator.Temp);
        NativeArray<int> v = new NativeArray<int>(3, Allocator.Temp);
        int a, b;
        l[0] = Voxels.s_ChunkSize; 
        l[1] = Voxels.s_ChunkHeight; 
        l[2] = Voxels.s_ChunkSize;
        for (int p = 0; p <= 2; p++)
        {
            a = (p + 1) % 3;
            b = (p + 2) % 3;

            for (d[p] = 0; d[p] < l[p]; d[p]++)
            {
                for (d[a] = 0; d[a] < l[a]; d[a]++)
                    for (d[b] = 0; d[b] < l[b]; d[b]++)
                    {
                        int3 abs_position = new int3(d[0], d[1], d[2]);
                        byte blockID = getValue(abs_position);
                        if (blockID == 0)
                            continue;

                        for(int i = -1; i <= 1; i += 2)
                        {
                            int faceIndex = getFaceIndex(p, i);
                            if (!isDrawFace(blockID, abs_position, faceIndex) || (m_ID.y == 0 && abs_position.y == 0 && faceIndex == 1))
                                continue;
                            v[p] = d[p];
                            v[a] = d[a];
                            v[b] = d[b];
                            int3 min_limit = new int3(v[0], v[1], v[2]);
                            int2 a_limits = new int2(d[a], d[a]);
                            for (int al = v[a] + 1; al < l[a]; al++)
                            {
                                v[a] = al;
                                if (isDrawFace(blockID, new int3(v[0], v[1], v[2]), faceIndex))
                                    a_limits.y = al;
                                else break;
                            }
                            int2 b_limits = new int2(d[b], d[b]);
                            v[a] = a_limits.y;
                            for (int bl = d[b] + 1; bl < l[b]; bl++)
                            {
                                v[b] = bl;
                                int3 greater_b = new int3(v[0], v[1], v[2]);
                                if (canDrawFace(blockID, min_limit, greater_b, faceIndex))
                                    b_limits.y = bl;
                                else break;
                            }
                            v[a] = a_limits.y;
                            v[b] = b_limits.y;
                            int3 max_limit = new int3(v[0], v[1], v[2]);

                            float3 meshCenter = getMeshCenter(min_limit, max_limit);
                            float2 meshSize = new float2(a_limits.y - a_limits.x + 1, b_limits.y - b_limits.x + 1);

                            int vertexIndex = Vertices.Length;
                            NativeArray<float3> fv = Voxels.GetFaceVertices(faceIndex);
                            foreach (float3 vertex in fv)
                            {
                                float3 global_vertex = vertex;
                                global_vertex[a] *= meshSize.x;
                                global_vertex[b] *= meshSize.y;
                                global_vertex += meshCenter;
                                Vertices.Add(global_vertex);
                            }
                            Triangles.AddRange(new NativeArray<int>(Voxels.GetFaceTriangles(vertexIndex), Allocator.Temp));
                            NativeArray<Vector3> uvs = Voxels.GetUVs();
                            foreach(Vector3 uv in uvs)
                            {
                                Vector3 u = uv;
                                u[a == 2 ? a - 1 - b : a] *= meshSize.x;
                                u[b == 2 ? b - 1 - a : b] *= meshSize.y; 
                                u.z = m_VoxelsConfig[blockID - 1].TextureIndex(faceIndex);
                                UVs.Add(u);
                            }
                            setAsDrawn(min_limit, max_limit, faceIndex);
                        }
                    }
            }
        }
    }

    private int getFaceIndex(int p, int i)
    {
        switch(p , i)
        {
            case (1, 1):  return 0;
            case (1, -1): return 1;
            case (0, 1):  return 2;
            case (0, -1): return 3;
            case (2, 1):  return 4;
            case (2, -1): return 5;
            default : return -1;
        }
    }

    private byte getValue(int x, int y, int z)
    {
        NativeArray<byte> targetChunk = m_Central_Chunk;
        if (y < 0 && y >= Voxels.s_ChunkHeight)
            return 0;
        targetChunk = x < 0 ? m_Left_Chunk : x == Voxels.s_ChunkSize ? m_Right_Chunk : targetChunk;
        targetChunk = z < 0 ? m_Back_Chunk : z == Voxels.s_ChunkSize ? m_Front_Chunk : targetChunk;

        return targetChunk.Length == 1 ? (byte)0 : targetChunk[Voxels.Index(x, y, z)];
    }
    private byte getValue(int3 xyz) => getValue(xyz.x, xyz.y, xyz.z);

    private void setAsDrawn(int3 xyz, int faceIndex)
    {
        bool keyExists = m_DrawnFaces.ContainsKey(xyz);

        FacesDrawn faces = keyExists ? m_DrawnFaces[xyz] : new FacesDrawn();
        faces.SetByIndex(faceIndex);
        if (keyExists)
            m_DrawnFaces.Remove(xyz);
        m_DrawnFaces.Add(xyz, faces);
    }
    private void setAsDrawn(int3 min, int3 max, int faceIndex)
    {
        int3 position = min;
        for (int y = min.y; y <= max.y; y++)
            for (int z = min.z; z <= max.z; z++)
                for (int x = min.x; x <= max.x; x++)
                {
                    position.x = x;
                    position.y = y;
                    position.z = z;
                    setAsDrawn(position, faceIndex);
                }
    }

    private bool isFaceDrawn(int3 xyz, int faceIndex)
    {
        bool isInDictionary = m_DrawnFaces.ContainsKey(xyz);
        return !isInDictionary ? false : m_DrawnFaces[xyz].IsFaceDrawn(faceIndex);
    }

    private bool isDrawFace(byte value, int3 xyz, int faceIndex)
    {
        int3 direction = Voxels.GetCheckDirection(faceIndex);
        return getValue(xyz) == value && getValue(xyz + direction) == 0 && !isFaceDrawn(xyz, faceIndex);
    }
    private bool canDrawFace(byte value, int3 min, int3 max, int faceIndex)
    {
        if (!(min.x == max.x || min.y == max.y || min.z == max.z))
            Debug.LogError("At least one axis needs to be flat");

        int3 position = min;
        for (int y = min.y; y <= max.y; y++)
            for (int z = min.z; z <= max.z; z++)
                for(int x = min.x; x <= max.x; x++)
                {
                    position.x = x;
                    position.y = y;
                    position.z = z;
                    if (!isDrawFace(value, position, faceIndex))
                        return false;
                }
        return true;
    }

    private float3 getMeshCenter(int3 min, int3 max)
    {
        int3 one = new int3(1,1,1);
        min -= one;
        max -= one;
        float x_dif = max.x - min.x;
        float y_dif = max.y - min.y;
        float z_dif = max.z - min.z;
        float x_center = min.x + (x_dif / 2f);
        float y_center = min.y + (y_dif / 2f);
        float z_center = min.z + (z_dif / 2f);

        return new float3(x_center * 0.5f, y_center * 0.5f, z_center * 0.5f);
    }

    public void Dispose()
    {
        m_Right_Chunk.Dispose();
        m_Left_Chunk.Dispose();
        m_Front_Chunk.Dispose();
        m_Back_Chunk.Dispose();
        m_Central_Chunk.Dispose();
        Vertices.Dispose();
        Triangles.Dispose();
        UVs.Dispose();
        m_DrawnFaces.Dispose();
        m_VoxelsConfig.Dispose();
    }
}

public struct FacesDrawn
{
    public FacesDrawn(bool top = false, bool bottom = false, bool right = false, bool left = false, bool front = false, bool back = false)
    {
        Top = top;
        Bottom = bottom;
        Right = right;
        Left = left;
        Front = front;
        Back = back;
    }

    public bool Top;
    public bool Bottom;
    public bool Right;
    public bool Left;
    public bool Front;
    public bool Back;

    public bool IsFaceDrawn(int faceIndex)
    {
        switch(faceIndex)
        {
            case 0: return Top; 
            case 1: return Bottom; 
            case 2: return Right; 
            case 3: return Left; 
            case 4: return Front; 
            case 5: return Back;
            default:
                Debug.LogError("Error");
                return false;
        }
    }
    public void SetByIndex(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0: 
                Top = true;
                break;
            case 1: 
                Bottom = true; 
                break;
            case 2:
                Right = true;
                break;
            case 3:
                Left = true;
                break;
            case 4:
                Front = true;
                break;
            case 5:
                Back = true;
                break;
            default:
                Debug.LogError("Error");
                break;
        }
    }
}
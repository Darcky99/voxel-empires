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
    public IChunkMesh(byte[] centralChunk, Vector3Int id)
    {
        m_ChunkSize = GameConfig.Instance.ChunkConfiguration.ChunkSize;
        m_Expanded_ChunkSize = GameConfig.Instance.ChunkConfiguration.Expanded_ChunkSize;

        Vertices = new NativeList<Vector3>(Allocator.Persistent);
        Triangles = new NativeList<int>(Allocator.Persistent);
        UVs = new NativeList<Vector2>(Allocator.Persistent);

        m_Expanded_Flat_Chunk = new NativeArray<byte>(centralChunk, Allocator.Persistent);
        m_ID = new int3(id.x, id.y, id.z);

        m_DrawnFaces = new NativeHashMap<int3, FacesDrawn>(6000,Allocator.Persistent);
    }

    private readonly int m_ChunkSize;
    private readonly int m_Expanded_ChunkSize;

    public NativeList<Vector3> Vertices { get; private set; }
    public NativeList<int> Triangles { get; private set; }
    public NativeList<Vector2> UVs { get; private set; }

    private readonly NativeArray<byte> m_Expanded_Flat_Chunk;
    private readonly NativeHashMap<int3, FacesDrawn> m_DrawnFaces;

    private readonly int3 m_ID;

    public void Execute()
    {
        if (m_Expanded_Flat_Chunk.Length == 1)
            return;
        int3 one = new int3(1, 1, 1);

        //the 3 dimensions
        NativeArray<int> d = new NativeArray<int>(3, Allocator.Temp);
        //a grows at last
        //all inside a d loop, which controlls the faces that we draw
        //in this scenario, we can draw up to two faces per block
        //
        int a, b;

        for(int p = 0; p <= 2; p++) //the plane
        {
            a = (p + 1) % 3;
            b = (p + 2) % 3;

            //increase on the main axis
            for (d[p] = 1; d[p] <= m_ChunkSize; d[p]++)
            {
                for (d[a] = 1; d[a] <= m_ChunkSize; d[a]++)
                    for (d[b] = 1; d[b] <= m_ChunkSize; d[b]++)
                    {
                        //we can use p[b] & p[c] to draw meshes by plane
                        int3 abs_position = new int3(d[0], d[1], d[2]);
                        byte blockID = getValue(abs_position);
                        if (blockID == 0)
                            continue;

                        abs_position -= one;
                        float3 voxelCenter = new float3(abs_position.x * 0.5f, abs_position.y * 0.5f, abs_position.z * 0.5f);
                        abs_position += one;

                        for (int faceIndex = 0; faceIndex <= 5; faceIndex++)
                        {
                            if (!canDrawFace(abs_position, faceIndex) || (m_ID.y == 0 && abs_position.y == 1 && faceIndex == 1))
                                continue;

                            int vertexIndex = Vertices.Length;

                            NativeArray<float3> faceVertices = Voxels.GetFaceVertices(faceIndex);
                            foreach (float3 vertex in faceVertices)
                                Vertices.Add(voxelCenter + vertex);

                            Triangles.AddRange(new NativeArray<int>(Voxels.GetFaceTriangles(vertexIndex), Allocator.Temp));
                        }
                    }
            }
            break;
        }



        //for (int y = 1; y <= m_ChunkSize; y++)
        //    for (int z = 1; z <= m_ChunkSize; z++)
        //        for (int x = 1; x <= m_ChunkSize; x++)
        //        {
        //            int3 position = new int3(x, y, z);
        //            byte blockID = getValue(position);

        //            if (blockID == 0)
        //                continue;

        //            position -= one;
        //            float3 voxelCenter = new float3(position.x * 0.5f, position.y * 0.5f, position.z * 0.5f);
        //            position += one;

        //            for (int faceIndex = 0; faceIndex <= 5; faceIndex++)
        //            {
        //                if (!canDrawFace(position, faceIndex) || (m_ID.y == 0 && y == 1 && faceIndex == 1))
        //                    continue;
                        
        //                int vertexIndex = Vertices.Length;

        //                if (!(faceIndex == 0 || faceIndex == 1))
        //                {
        //                    Triangles.AddRange(new NativeArray<int>(Voxels.GetFaceTriangles(Vertices.Length), Allocator.Temp));

        //                    NativeArray<float3> faceVertices = Voxels.GetFaceVertices(faceIndex);
        //                    foreach (float3 vertex in faceVertices)
        //                        Vertices.Add(voxelCenter + vertex);

        //                    continue;
        //                }

        //                int2 x_limits = new int2(x, x);
        //                for (int gx = x + 1; gx <= m_ChunkSize; gx++)
        //                {
        //                    if (canDrawFace(new int3(gx, y, z), faceIndex))
        //                        x_limits.y = gx;
        //                    else break;
        //                }
        //                int2 z_limits = new int2(z, z);
        //                for (int gz = z + 1; gz <= m_ChunkSize; gz++)
        //                {
        //                    if (canDrawFace(new int3(x_limits.x, y, gz), new int3(x_limits.y, y, gz), faceIndex))
        //                        z_limits.y = gz;
        //                    else break;
        //                }

        //                setAsDrawn(x_limits, y, z_limits, faceIndex);

        //                NativeArray<float3> fv = Voxels.GetFaceVertices(faceIndex);
        //                float3 meshCenter = getMeshCenter(x_limits, position.y, z_limits);

        //                foreach (float3 vertex in fv)
        //                {
        //                    float2 size = getMeshSize(x_limits, z_limits);
        //                    float3 v = new float3(vertex.x * size.x, vertex.y, vertex.z * size.y);
        //                    Vertices.Add(meshCenter + v);
        //                }

        //                Triangles.AddRange(new NativeArray<int>(Voxels.GetFaceTriangles(vertexIndex), Allocator.Temp));
        //                continue;
        //            }
        //        }
        
    }

    private byte getValue(int x, int y, int z) => m_Expanded_Flat_Chunk[Voxels.Expanted_Index(x, y, z)];
    private byte getValue(int3 xyz) => getValue(xyz.x, xyz.y, xyz.z);

    private void setAsDrawn(int3 xyz, int faceIndex)
    {
        if (m_DrawnFaces.ContainsKey(xyz))
        {
            FacesDrawn faces = m_DrawnFaces[xyz];
            faces.SetByIndex(faceIndex);
            m_DrawnFaces.Remove(xyz);
            m_DrawnFaces.Add(xyz, faces);
        }
        else
        {
            FacesDrawn faces = new FacesDrawn();
            faces.SetByIndex(faceIndex);
            m_DrawnFaces.Add(xyz, faces);
        }
    }
    private void setAsDrawn(int2 x_limits, int y, int2 z_limits, int faceIndex)
    {
        int3 position = new int3(x_limits.x, y, z_limits.x);
        for (int z = z_limits.x; z <= z_limits.y; z++)
            for (int x = x_limits.x; x <= x_limits.y; x++)
            {
                position.x = x;
                position.z = z;
                setAsDrawn(position, faceIndex);
            }
    }

    private bool isFaceDrawn(int3 xyz, int faceIndex)
    {
        bool isInDictionary = m_DrawnFaces.ContainsKey(xyz);
        return !isInDictionary ? false : m_DrawnFaces[xyz].IsFaceDrawn(faceIndex);
    }

    private bool canDrawFace(int3 xyz, int faceIndex)
    {
        int3 direction = Voxels.GetCheckDirection(faceIndex);
        return getValue(xyz) != 0 && getValue(xyz + direction) == 0 && !isFaceDrawn(xyz, faceIndex);
    }
    private bool canDrawFace(int3 min, int3 max, int faceIndex)
    {
        if (min.y != max.y)
            Debug.LogError("Different Y axis on min max, this is not allowed");

        int3 position = min;
        for (int z = min.z; z <= max.z; z++)
            for(int x = min.x; x <= max.x; x++)
            {
                position.x = x;
                position.z = z;

                if (!canDrawFace(position, faceIndex))
                    return false;
            }
        return true;
    }

    private float3 getMeshCenter(int2 x_limits, float y, int2 z_limits)
    {
        x_limits.x -= 1; 
        x_limits.y -= 1;
        z_limits.x -= 1;
        z_limits.y -= 1;
        y -= 1;

        float x_dif = x_limits.y - x_limits.x;
        float z_dif = z_limits.y - z_limits.x;

        float x_center = x_limits.x + (x_dif / 2f);
        float z_center = z_limits.x + (z_dif / 2f);

        return new float3(x_center * 0.5f, y * 0.5f, z_center * 0.5f);
    }
    private float2 getMeshSize(int2 x_limits, int2 z_limits)
    {
        float x_dif = x_limits.y - x_limits.x;
        float z_dif = z_limits.y - z_limits.x;
        return new float2((x_dif + 1) * 1.0f, (z_dif + 1) * 1.0f);
    }


    public void Dispose()
    {
        Vertices.Dispose();
        Triangles.Dispose();
        UVs.Dispose();
        m_Expanded_Flat_Chunk.Dispose();
        m_DrawnFaces.Dispose();
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
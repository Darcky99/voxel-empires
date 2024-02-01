using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class MeshData
{
    public MeshData(NativeList<Vector3> vertices, NativeList<int> triangles, NativeList<Vector2> uvs)
    {
        Vertices = vertices;
        Triangles = triangles;
        UVs = uvs;
    }

    public NativeList<Vector3> Vertices { get; private set; }
    public NativeList<int> Triangles { get; private set; }
    public NativeList<Vector2> UVs { get; private set; }

    public void Dispose()
    {
        Vertices.Dispose();
        Triangles.Dispose();
        UVs.Dispose();
    }

    //public static MeshData operator + (MeshData left, MeshData right)
    //{
    //    left.Vertices.AddRange(right.Vertices);
    //    left.Triangles.AddRange(right.Triangles);
    //    left.UVs.AddRange(right.UVs);
    //    return left;
    //}
}
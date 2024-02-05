using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;

public class MeshData
{
    public MeshData(Vector3[] vertices, int[] triangles, Vector2[] uvs)
    {
        Vertices = vertices;
        Triangles = triangles;
        UVs = uvs;
    }

    public Vector3[] Vertices { get; private set; }
    public int[] Triangles { get; private set; }
    public Vector2[] UVs { get; private set; }

    //public void Dispose()
    //{
    //    Vertices.Dispose();
    //    Triangles.Dispose();
    //    UVs.Dispose();
    //}
}
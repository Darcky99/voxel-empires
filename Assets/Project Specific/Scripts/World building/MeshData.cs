using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public struct MeshData
{
    public MeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Vertices = vertices;
        Triangles = triangles;
        UVs = uvs;
    }

    public List<Vector3> Vertices { get; private set; }
    public List<int> Triangles { get; private set; }
    public List<Vector2> UVs { get; private set; }

    public static MeshData operator + (MeshData left, MeshData right)
    {
        left.Vertices.AddRange(right.Vertices);
        left.Triangles.AddRange(right.Triangles);
        left.UVs.AddRange(right.UVs);
        return left;
    }
}
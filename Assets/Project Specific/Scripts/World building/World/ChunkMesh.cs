using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using Unity.Collections.NotBurstCompatible;

public class ChunkMesh : MonoBehaviour
{
    [SerializeField] private MeshFilter m_MeshFilter;
    //[SerializeField, Sirenix.OdinInspector.ReadOnly] private Vector3[] m_VertexCount;
    //[SerializeField, Sirenix.OdinInspector.ReadOnly] private int[] m_TrianglesCount;

    public async Task SetMesh(int3 chunkID, NativeList<Vector3> vertices, NativeList<int> triangles, NativeList<Vector2> uvs)
    {
        Vector3 worldPosition = (new Vector3(chunkID.x, chunkID.y, chunkID.z) * 16) / 2f;

        transform.position = worldPosition;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArrayNBC();
        mesh.triangles = triangles.ToArrayNBC();
        mesh.uv = uvs.ToArrayNBC();

        mesh.RecalculateNormals();

        m_MeshFilter.mesh = mesh;

        //m_VertexCount = NativeArrayFloat3ToVector3(vertices);
        //m_TrianglesCount = NativeArrayIntToInt(triangles);

        vertices.Dispose();
        triangles.Dispose();
        uvs.Dispose();
    }

    public static Vector3[] NativeArrayFloat3ToVector3(NativeList<float3> nativeArray)
    {
        int length = nativeArray.Length;
        Vector3[] resultArray = new Vector3[length];

        for (int i = 0; i < length; i++)
        {
            resultArray[i] = new Vector3(nativeArray[i].x, nativeArray[i].y, nativeArray[i].z);
        }

        return resultArray;
    }
    public static int[] NativeArrayIntToInt(NativeList<int> nativeArray)
    {
        int length = nativeArray.Length;
        int[] resultArray = new int[length];

        for (int i = 0; i < length; i++)
            resultArray[i] = nativeArray[i];

        return resultArray;
    }
    public static Vector2[] NativeArrayFloat2ToVector2(NativeList<float2> nativeArray)
    {
        int length = nativeArray.Length;
        Vector2[] resultArray = new Vector2[length];

        for (int i = 0; i < length; i++)
        {
            resultArray[i] = new Vector2(nativeArray[i].x, nativeArray[i].y);
        }

        return resultArray;
    }

}
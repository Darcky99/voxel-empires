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
    [SerializeField] private MeshFilter[] m_LODs;

    public void SetMesh(int3 chunkID, MeshData[] meshData)
    {
        Vector3 worldPosition = (new Vector3(chunkID.x, chunkID.y, chunkID.z) * 16) / 2f;
        transform.position = worldPosition;

        for(int i = 0 ; i < meshData.Length; i++)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = meshData[i].Vertices;
            mesh.triangles = meshData[i].Triangles;
            mesh.uv = meshData[i].UVs;
            mesh.RecalculateNormals();

            m_LODs[i].mesh = mesh;
        }
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
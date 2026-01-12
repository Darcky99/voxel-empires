
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelEmpires.VoxelUtilities
{
    public static class Voxels
    {
        private static float3 GetVoxelVertice(int vertexIndex)
        {
            switch (vertexIndex)
            {
                case 0: return new float3(-0.25f, 0.25f, 0.25f);
                case 1: return new float3(0.25f, 0.25f, 0.25f);
                case 2: return new float3(0.25f, 0.25f, -0.25f);
                case 3: return new float3(-0.25f, 0.25f, -0.25f);
                case 4: return new float3(-0.25f, -0.25f, 0.25f);
                case 5: return new float3(0.25f, -0.25f, 0.25f);
                case 6: return new float3(0.25f, -0.25f, -0.25f);
                case 7: return new float3(-0.25f, -0.25f, -0.25f);
                default: return default;
            }
        }
        private static int GetFaceVertexByIndex(int faceIndex, int faceVertexIndex)
        {
            switch (faceIndex, faceVertexIndex)
            {
                //Up
                case (0, 0): return 0;
                case (0, 1): return 1;
                case (0, 2): return 3;
                case (0, 3): return 2;
                //Down
                case (1, 0): return 7;
                case (1, 1): return 6;
                case (1, 2): return 4;
                case (1, 3): return 5;
                //Right
                case (2, 0): return 2;
                case (2, 1): return 1;
                case (2, 2): return 6;
                case (2, 3): return 5;
                //Left
                case (3, 0): return 0;
                case (3, 1): return 3;
                case (3, 2): return 4;
                case (3, 3): return 7;
                //Front
                case (4, 0): return 1;
                case (4, 1): return 0;
                case (4, 2): return 5;
                case (4, 3): return 4;
                //Behind
                case (5, 0): return 3;
                case (5, 1): return 2;
                case (5, 2): return 7;
                case (5, 3): return 6;

                default: return default;
            }
        }

        public static int3 GetDirection(int directionIndex)
        {
            switch (directionIndex)
            {
                case 0: return new int3(0, 1, 0);
                case 1: return new int3(0, -1, 0);
                case 2: return new int3(1, 0, 0);
                case 3: return new int3(-1, 0, 0);
                case 4: return new int3(0, 0, 1);
                case 5: return new int3(0, 0, -1);
                default: return default;
            }
        }
        public static NativeArray<float3> GetFaceVertices(int faceIndex)
        {
            NativeArray<float3> i_vertexArray = new NativeArray<float3>(4, Allocator.Temp);

            i_vertexArray[0] = GetVoxelVertice(GetFaceVertexByIndex(faceIndex, 0));
            i_vertexArray[1] = GetVoxelVertice(GetFaceVertexByIndex(faceIndex, 1));
            i_vertexArray[2] = GetVoxelVertice(GetFaceVertexByIndex(faceIndex, 2));
            i_vertexArray[3] = GetVoxelVertice(GetFaceVertexByIndex(faceIndex, 3));

            return i_vertexArray;
        }
        public static NativeArray<int> GetFaceTriangles(int vertexCount)
        {
            NativeArray<int> i_triangleIndexList = new NativeArray<int>(6, Allocator.Temp);

            i_triangleIndexList[0] = vertexCount + 0;
            i_triangleIndexList[1] = vertexCount + 1;
            i_triangleIndexList[2] = vertexCount + 2;
            i_triangleIndexList[3] = vertexCount + 1;
            i_triangleIndexList[4] = vertexCount + 3;
            i_triangleIndexList[5] = vertexCount + 2;
            return i_triangleIndexList;
        }
        public static NativeArray<Vector3> GetUVs()
        {
            NativeArray<Vector3> i_uvs = new NativeArray<Vector3>(4, Allocator.Temp);
            i_uvs[0] = new Vector3(0, 1, 0);
            i_uvs[1] = new Vector3(1, 1, 0);
            i_uvs[2] = new Vector3(0, 0, 0);
            i_uvs[3] = new Vector3(1, 0, 0);
            return i_uvs;
        }

        
    }
}
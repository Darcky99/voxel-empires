
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelUtils
{
    public static class Voxels
    {
        private static readonly int s_ChunkSize = 16;
        private static int s_Expanded_ChunkSize => s_ChunkSize + 2;

        public static int Index(int x, int y, int z)
        {
            if (x < 0 || x >= s_ChunkSize || y < 0 || y >= s_ChunkSize || z < 0 || z >= s_ChunkSize)
                Debug.LogError($"Asking for coordinates out of range {x}, {y}, {z}");

            return x + (z * s_ChunkSize) + (y * s_ChunkSize * s_ChunkSize);
        }
        public static int3 XYZ(int i)
        {
            int squareChunkSize = s_ChunkSize * s_ChunkSize;

            int3 xyz = new int3();

            xyz.y = (int)math.floor(i / (float)squareChunkSize);
            i -= xyz.y * squareChunkSize;
            xyz.z = (int)math.floor(i / (float)s_ChunkSize);
            i -= xyz.z * s_ChunkSize;
            xyz.x = i;

            return xyz;
        }

        public static int Expanted_Index(int x, int y, int z)
        {
            if (x < 0 || x >= s_Expanded_ChunkSize || y < 0 || y >= s_Expanded_ChunkSize || z < 0 || z >= s_Expanded_ChunkSize)
                Debug.LogError($"Asking for coordinates out of range");

            return x + (z * s_Expanded_ChunkSize) + (y * s_Expanded_ChunkSize * s_Expanded_ChunkSize);
        }
        public static int3 Expanded_XYZ(int i)
        {
            int squareChunkSize = s_Expanded_ChunkSize * s_Expanded_ChunkSize;

            int3 xyz = new int3();

            xyz.y = (int)math.floor(i / (float)squareChunkSize);
            i -= xyz.y * squareChunkSize;
            xyz.z = (int)math.floor(i / (float)s_Expanded_ChunkSize);
            i -= xyz.z * s_Expanded_ChunkSize;
            xyz.x = i;

            return xyz;
        }


        public static float3 GetVoxelVertice(int vertexIndex)
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
        public static int3 GetCheckDirection(int directionIndex)
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
        public static int GetFaceVertexByIndex(int faceIndex, int faceVertexIndex)
        {
            switch (faceIndex, faceVertexIndex)
            {
                case (0, 0): return 0;
                case (0, 1): return 1;
                case (0, 2): return 3;
                case (0, 3): return 2;

                case (1, 0): return 7;
                case (1, 1): return 6;
                case (1, 2): return 4;
                case (1, 3): return 5;

                case (2, 0): return 2;
                case (2, 1): return 1;
                case (2, 2): return 6;
                case (2, 3): return 5;

                case (3, 0): return 0;
                case (3, 1): return 3;
                case (3, 2): return 4;
                case (3, 3): return 7;

                case (4, 0): return 1;
                case (4, 1): return 0;
                case (4, 2): return 5;
                case (4, 3): return 4;

                case (5, 0): return 3;
                case (5, 1): return 2;
                case (5, 2): return 7;
                case (5, 3): return 6;

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
    }
}
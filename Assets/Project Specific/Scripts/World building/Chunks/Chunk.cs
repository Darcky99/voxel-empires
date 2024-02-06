using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoxelUtils;
using Project.Managers;

namespace Chunks
{
    public class Chunk
    {
        private ChunksManager m_ChunksManager => ChunksManager.Instance;

        public Chunk(Vector3Int ID)
        {
            m_ChunkID = ID;
            m_VoxelMap = new VoxelMap((byte)ChunkConfiguration.ChunkSize);
            m_ChunkState = eChunkState.Active;
        }

        #region Editor
        public void OnDrawGizmos()
        {
            int chunkSize = ChunkConfiguration.ChunkSize;
            Vector3 offset = new Vector3(m_ChunkID.x, m_ChunkID.y, m_ChunkID.z) * chunkSize * .5f;

            for (int y = 0; y < chunkSize; y++)
                for (int z = 0; z < chunkSize; z++)
                    for (int x = 0; x < chunkSize; x++)
                    {
                        Vector3Int position = new Vector3Int(x, y, z);
                        byte blockID = GetVoxel(position);

                        if (blockID == 0)
                            continue;

                        position -= Vector3Int.one;
                        Vector3 voxelCenter = new Vector3(position.x * 0.5f, position.y * 0.5f, position.z * 0.5f);
                        position += Vector3Int.one;

                        Gizmos.DrawWireCube(offset + voxelCenter, Vector3.one * 0.5f);
                    }
        }
        #endregion

        public Vector3Int ChunkID => m_ChunkID;
        public eChunkState ChunkState => m_ChunkState;

        private Vector3Int m_ChunkID;
        private VoxelMap m_VoxelMap;
        private ChunkMesh m_ChunkMesh;
        private eChunkState m_ChunkState;

        public byte GetVoxel(Vector3Int voxelPosition) => m_VoxelMap.GetVoxel(voxelPosition);
        public void SetVoxel(Vector3Int voxelPosition) => m_VoxelMap.SetVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, 1);

        public void SetVoxelMap(byte[] flatVoxelMap) => m_VoxelMap.SetFlatMap(flatVoxelMap);
        public byte[] Get_Expanded_VoxelMap()
        {
            int flatChunkSizeMaxIndex = ChunkConfiguration.FlatChunkSize - 1;

            byte[] flatMap = m_VoxelMap.Expanded_FlatMap;

            if (flatMap.Length == 1)
                return flatMap;

            for (int y = 0; y <= flatChunkSizeMaxIndex; y++)
                for (int z = 0; z <= flatChunkSizeMaxIndex; z++)
                    for (int x = 0; x <= flatChunkSizeMaxIndex; x++)
                    {
                        if (!(x == 0 || x == flatChunkSizeMaxIndex || y == 0 || y == flatChunkSizeMaxIndex || z == 0 || z == flatChunkSizeMaxIndex))
                            continue;

                        Vector3Int adjacentChunkID = default, adjacentVoxel = default;

                        adjacentChunkID.x = x == 0 ? m_ChunkID.x - 1 : x == 17 ? m_ChunkID.x + 1 : m_ChunkID.x;
                        adjacentChunkID.y = y == 0 ? m_ChunkID.y - 1 : y == 17 ? m_ChunkID.y + 1 : m_ChunkID.y;
                        adjacentChunkID.z = z == 0 ? m_ChunkID.z - 1 : z == 17 ? m_ChunkID.z + 1 : m_ChunkID.z;

                        Chunk adjacentChunk = m_ChunksManager.GetChunk(adjacentChunkID);

                        if (adjacentChunk == null)
                        {
                            flatMap[Voxels.Expanted_Index(x, y, z)] = 0;
                            continue;
                        }

                        adjacentVoxel.x = x == 0 ? 15 : x == 17 ? 0 : x - 1;
                        adjacentVoxel.y = y == 0 ? 15 : y == 17 ? 0 : y - 1;
                        adjacentVoxel.z = z == 0 ? 15 : z == 17 ? 0 : z - 1;

                        flatMap[Voxels.Expanted_Index(x, y, z)] = adjacentChunk.GetVoxel(adjacentVoxel);
                    }

            return flatMap;
        }

        public void DrawMesh()
        {
            if(m_ChunkMesh == null)
                m_ChunkMesh = ChunkMeshPool.s_Instance.DeQueue();

            m_ChunkMesh.Initialize(this);
            m_ChunkState = eChunkState.Drawn;
        }
    }
}
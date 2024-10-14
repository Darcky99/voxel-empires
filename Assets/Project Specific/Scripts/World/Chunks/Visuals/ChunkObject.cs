using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using World;
using Unity.Jobs;
using Unity.Collections.NotBurstCompatible;
using System;

namespace World
{
    public class ChunkObject : MonoBehaviour
    {
        private WorldManager _WorldManager => WorldManager.Instance;
        private CameraController _CameraController => CameraController.Instance;

        private Chunk _Chunk;

        [SerializeField] private MeshFilter lods;
        [SerializeField] private MeshCollider meshCollider;

        #region Unity
        private void OnEnable()
        {
            _CameraController.Move += CameraController_OnMove;
        }
        private void OnDisable()
        {
            _CameraController.Move -= CameraController_OnMove;
        }
        #endregion

        private void CameraController_OnMove(object sender, EventArgs e)
        {
            //send the mesh to be drawn
        }

        private void SetMesh(Mesh mesh)
        {
            lods.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
        public void Set(Chunk chunk, Mesh mesh)
        {
            _Chunk = chunk;
            transform.position = ChunkUtils.ChunkIDToWorldCoordinates(_Chunk.ChunkID);
            SetMesh(mesh);
        }
    }
}
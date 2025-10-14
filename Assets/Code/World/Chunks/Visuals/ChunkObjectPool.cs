using UnityEngine;
using VE.World;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VE.VoxelUtilities.Pooling
{
    public class ChunkObjectPool : Pool<ChunkObject>
    {

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(ChunkObjectPool))]
    public class ChunkObjectPoolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ChunkObjectPool poolObject = (ChunkObjectPool)target;
            if (GUILayout.Button("PreBake"))
            {
                poolObject.PreBake();
            }
            if (GUILayout.Button("Clear Objects"))
            {
                poolObject.ClearObjects();
            }
        }
    }
#endif
}
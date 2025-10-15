using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VE.PerlinTexture
{
    public class PerlinTextureGenerator : MonoBehaviour
    {
        public event EventHandler TextureGenerated;

        [Title("General values")]
        [SerializeField] private Vector2Int _TextureSize;
        [field: SerializeField, Range(100000, 999999)] public uint Seed { get; private set; }
        [field: SerializeField] public float Scale { get; private set; }

        [Title("Noise parameters")]
        [SerializeField] private BiomeParameters[] _Biomes;
        [SerializeField] private NoiseParameters[] _NoiseParameters;

        public Vector2Int TextureSize => _TextureSize;
        public PerlinTextureJob TextureResult { get; private set; }

        private void Start()
        {
            CreateTextures();
        }

        private IEnumerator GenerateTextures()
        {
            TextureJobParameters textureJobParameters = new TextureJobParameters(this);
            PerlinTextureJob perlinTextureJob = new PerlinTextureJob(textureJobParameters, _Biomes[0].NoiseParameters);
            JobHandle jobHandle = perlinTextureJob.Schedule();
            yield return new WaitUntil(() => jobHandle.IsCompleted);
            jobHandle.Complete();
            TextureResult = perlinTextureJob;
            TextureGenerated?.Invoke(this, EventArgs.Empty);
            yield return null;
            perlinTextureJob.Dispose();
        }
        public void CreateTextures()
        {
            StartCoroutine(GenerateTextures());
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PerlinTextureGenerator))]
    public class PerlinTextureGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            PerlinTextureGenerator textureGenerator = (PerlinTextureGenerator)target;
            if (GUILayout.Button("Generate Texture"))
            {
                textureGenerator.CreateTextures();
            }
        }
    }
#endif
}
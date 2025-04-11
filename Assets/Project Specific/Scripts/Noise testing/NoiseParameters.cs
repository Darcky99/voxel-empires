using System;
using Unity.Mathematics;
using UnityEngine;

namespace VE.PerlinTexture
{
    [Serializable]
    public class NoiseParameters
    {
        [SerializeField, Range(1, 10)] internal int _Octaves;
        [SerializeField, Range(0.00000001f, 100)] internal float _Scale;
        [SerializeField, Range(0.01f, 5f)] internal float _Persistance;
        [SerializeField, Range(0.01f, 15f)] internal float _Lacunarity;
        [SerializeField] internal AnimationCurve _Curve;
    }
    public struct BurstNoiseParameters
    {
        public BurstNoiseParameters(NoiseParameters noiseParameters)
        {
            Octaves = noiseParameters._Octaves;
            Scale = noiseParameters._Scale;
            Persistance = noiseParameters._Persistance;
            Lacunarity = noiseParameters._Lacunarity;
            Curve = new BurstAnimationCurve(noiseParameters._Curve.keys);
        }
        public int Octaves { get; private set; }
        public float Scale { get; private set; }
        public float Persistance { get; private set; }
        public float Lacunarity { get; private set; }
        public BurstAnimationCurve Curve { get; private set; }

        public void Dispose()
        {
            Curve.Dispose();
        }
    }
}
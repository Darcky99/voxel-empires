using System;
using VE.CustomNoise;
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
        [SerializeField] internal eNoiseRangeMode _NoiseRange;
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
            NoiseRange = noiseParameters._NoiseRange;
            Curve = new BurstAnimationCurve(noiseParameters._Curve.keys);
        }
        public int Octaves { get; private set; }
        public float Scale { get; private set; }
        public float Persistance { get; private set; }
        public float Lacunarity { get; private set; }
        public eNoiseRangeMode NoiseRange { get; private set; }
        public BurstAnimationCurve Curve { get; private set; }

        public float GetNoise(ProceduralNoiseProject.PerlinNoise perlin, int x, int y, uint seed, float scale)
        {
            float noise = Noise.Perlin2D(perlin, x, y, Scale * scale, Octaves, Persistance, Lacunarity);
            noise = NoiseRange switch
            {
                eNoiseRangeMode.Regular => noise,
                eNoiseRangeMode.Absolute => math.abs(noise),
                _ => noise
            };
            return Curve.Evaluate(noise);
        }

        public void Dispose()
        {
            Curve.Dispose();
        }
    }
}
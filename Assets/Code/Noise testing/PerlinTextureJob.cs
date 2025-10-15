using ProceduralNoiseProject;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace VE.PerlinTexture
{
    [BurstCompile]
    public struct PerlinTextureJob : IJob
    {
        public PerlinTextureJob(TextureJobParameters jobParameters, NoiseParameters[] noiseParameters)
        {
            _JobParameters = jobParameters;
            Continentalness_TextureData = new NativeArray<Color>(jobParameters.TextureSize.x * jobParameters.TextureSize.y, Allocator.Persistent);
            Erosion_TextureData = new NativeArray<Color>(jobParameters.TextureSize.x * jobParameters.TextureSize.y, Allocator.Persistent);
            PeaksAndValleys_TextureData = new NativeArray<Color>(jobParameters.TextureSize.x * jobParameters.TextureSize.y, Allocator.Persistent);
            Result_TextureData = new NativeArray<Color>(jobParameters.TextureSize.x * jobParameters.TextureSize.y, Allocator.Persistent);
            _Continentalness = new BurstNoiseParameters(noiseParameters[0]);
            _Erosion = new BurstNoiseParameters(noiseParameters[1]);
            _PeaksAndValleys = new BurstNoiseParameters(noiseParameters[2]);

            _PerlinNoise = new PerlinNoise((int)jobParameters.Seed, 1, Allocator.Persistent);
        }

        private TextureJobParameters _JobParameters;
        private BurstNoiseParameters _Continentalness;
        private BurstNoiseParameters _Erosion;
        private BurstNoiseParameters _PeaksAndValleys;

        public NativeArray<Color> Continentalness_TextureData;
        public NativeArray<Color> Erosion_TextureData;
        public NativeArray<Color> PeaksAndValleys_TextureData;
        public NativeArray<Color> Result_TextureData;

        private PerlinNoise _PerlinNoise;

        public void Execute()
        {
            uint seed = _JobParameters.Seed;
            float scale = _JobParameters.Scale;
            for (int x = 0; x < _JobParameters.TextureSize.x; x++)
            {
                for (int y = 0; y < _JobParameters.TextureSize.y; y++)
                {
                    // I need a biome map, which on each position tells how close it is to the biome center...
                    // based on the biome, use one or another noise parameters.
                    // but it's not that simple. I need to take in account biomes around. So I can lerp between values.
                    // I might need a struct container of all posible biome parameters.

                    // So, the biome map is a pregenerated colored texuture. 

                    // 1. Generate noise
                    float c = _Continentalness.GetNoise(_PerlinNoise, x, y, seed, scale);
                    float e = _Erosion.GetNoise(_PerlinNoise, x, y, seed, scale);
                    float pv = _PeaksAndValleys.GetNoise(_PerlinNoise, x, y, seed, scale);

                    // float c = _PerlinNoise.Sample2D(x, y);
                    // float e = _Erosion.GetNoise(_PerlinNoise, x, y, seed, scale);
                    // float pv = _PeaksAndValleys.GetNoise(_PerlinNoise, x, y, seed, scale);

                    // 2. Noise merging.
                    float cepv = (c + (pv * e)) / 2f;

                    // 3. Assign to textures.
                    int arrayIndex = x + (y * _JobParameters.TextureSize.x);
                    Continentalness_TextureData[arrayIndex] = new Color(c, c, c, 1);
                    Erosion_TextureData[arrayIndex] = new Color(e, e, e, 1);
                    PeaksAndValleys_TextureData[arrayIndex] = new Color(pv, pv, pv, 1);
                    Result_TextureData[arrayIndex] = new Color(cepv, cepv, cepv, 1);
                }
            }
        }

        public void Dispose()
        {
            _Continentalness.Dispose();
            _Erosion.Dispose();
            _PeaksAndValleys.Dispose();
            Continentalness_TextureData.Dispose();
            Erosion_TextureData.Dispose();
            PeaksAndValleys_TextureData.Dispose();
            Result_TextureData.Dispose();
            _PerlinNoise.Dispose();
        }
    }
}

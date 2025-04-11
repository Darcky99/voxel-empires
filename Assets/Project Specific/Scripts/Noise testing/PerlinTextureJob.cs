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
        }

        private TextureJobParameters _JobParameters;
        private BurstNoiseParameters _Continentalness;
        private BurstNoiseParameters _Erosion;
        private BurstNoiseParameters _PeaksAndValleys;

        public NativeArray<Color> Continentalness_TextureData;
        public NativeArray<Color> Erosion_TextureData;
        public NativeArray<Color> PeaksAndValleys_TextureData;
        public NativeArray<Color> Result_TextureData;

        public void Execute()
        {
            // Here you could read _JobParameters.CombineStyle and based on that operate.
            // 

            uint seed = _JobParameters.Seed;
            float scale = _JobParameters.Scale;
            for (int x = 0; x < _JobParameters.TextureSize.x; x++)
            {
                for (int y = 0; y < _JobParameters.TextureSize.y; y++)
                {
                    // 1. Noise generation
                    int arrayIndex = x + (y * _JobParameters.TextureSize.x);
                    float c_noice = Noise.Perlin2D(x, y, seed, _Continentalness.Scale * scale, _Continentalness.Octaves, _Continentalness.Persistance, _Continentalness.Lacunarity);
                    float e_noice = Noise.Perlin2D(x, y, seed, _Erosion.Scale * scale, _Erosion.Octaves, _Erosion.Persistance, _Erosion.Lacunarity);
                    float pv_noice = Noise.Perlin2D(x, y, seed, _PeaksAndValleys.Scale * scale, _PeaksAndValleys.Octaves, _PeaksAndValleys.Persistance, _PeaksAndValleys.Lacunarity);
                    pv_noice = math.abs(pv_noice);

                    float c = _Continentalness.Curve.Evaluate(c_noice);
                    float e = _Erosion.Curve.Evaluate(e_noice);
                    float pv = _PeaksAndValleys.Curve.Evaluate(pv_noice);

                    // 2. Noise combination
                    // float cepv = ((c + pv) * e) / 2f;
                    float cepv = (c + (pv * e)) / 2f;

                    // 3. 
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
        }
    }
}

using Unity.Mathematics;

public static class Noise
{
    public static float Perlin2D(int x, int y, uint seed, float scale, int octaves, float persistance, float lacunarity)
    {
        FastNoiseLite noise = new FastNoiseLite((int)seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        if (scale <= 0)
        {
            scale = 0.0000001f;
        }
        float noiseValue = 0;
        float amplitude = 1;
        float frequensy = 1;
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequensy;
            float sampleY = y / scale * frequensy;

            float perlinNoise = noise.GetNoise(sampleX, sampleY);
            noiseValue += perlinNoise * amplitude;
            amplitude *= persistance;
            frequensy *= lacunarity;
        }
        return noiseValue;
    }
}
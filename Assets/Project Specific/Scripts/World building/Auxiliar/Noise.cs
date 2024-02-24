using Unity.Mathematics;

public static class Noise
{
    public static float Perlin2D(int x, int y, uint seed, float scale, int octaves, float persistance, float lacunarity)
    {
        Random random = new Random(seed);
        float2 seedOffset = new float2(random.NextFloat(-100000f, 100000f), random.NextFloat(-100000f, 100000f));

        if (scale <= 0)
            scale = 0.0000001f;

        float noiseValue = 0;
        float amplitude = 1;
        float frequensy = 1;

        for(int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequensy + seedOffset.x;
            float sampleY = y / scale * frequensy + seedOffset.y;

            float perlinNoise = noise.cnoise(new float2(sampleX, sampleY));
            noiseValue += perlinNoise * amplitude;

            amplitude *= persistance;
            frequensy *= lacunarity;
        }
        return noiseValue;
    }
}
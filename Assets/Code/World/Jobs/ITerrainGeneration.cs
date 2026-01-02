using VE.CustomNoise;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using VE.VoxelUtilities;

[BurstCompile]
public struct ITerrainGeneration : IJob
{
    public ITerrainGeneration(int2 chunkID, int3 chunkSize)
    {
        _chunkID = chunkID;
        _terrainMaxHeight = (uint)GameConfig.Instance.WorldConfiguration.WorldHeight;

        HeightMap = new NativeGrid<byte>(chunkSize, Allocator.Persistent);
        IsEmpty = true;

        Continentalness = new BurstAnimationCurve(GameConfig.Instance.WorldConfiguration.Continentalness.keys);
        Erosion = new BurstAnimationCurve(GameConfig.Instance.WorldConfiguration.Erosion.keys);
        PeaksAndValleys = new BurstAnimationCurve(GameConfig.Instance.WorldConfiguration.PeaksAndValleys.keys);

        _seed = GameConfig.Instance.WorldConfiguration.Seed;
        _scale = GameConfig.Instance.WorldConfiguration.Scale;
        _perlinNoise = new ProceduralNoiseProject.PerlinNoise(_seed, 1, Allocator.Persistent);
    }

    private ProceduralNoiseProject.PerlinNoise _perlinNoise;
    public NativeGrid<byte> HeightMap;
    public bool IsEmpty;

    private uint _terrainMaxHeight;
    private int _seed;
    private float _scale;

    private BurstAnimationCurve Continentalness;
    private BurstAnimationCurve Erosion;
    private BurstAnimationCurve PeaksAndValleys;

    private readonly int2 _chunkID;

    public void Execute()
    {

        int2 globalChunkPosition = new int2(_chunkID.x * HeightMap.Lenght.x, _chunkID.y * HeightMap.Lenght.z);
        float persistance = 1f;
        float lacunarity = 1f;
        for (int x = 0; x < HeightMap.Lenght.x; x++)
        {
            for (int z = 0; z < HeightMap.Lenght.z; z++)
            {
                int sampleX = globalChunkPosition.x + x;
                int sampleZ = globalChunkPosition.y + z;

                float continentalness_noice = Noise.Perlin2D(_perlinNoise, sampleX, sampleZ, 0.15f * _scale, 4, 0.55f * persistance, 1.25f * lacunarity);
                float erosion_noice = Noise.Perlin2D(_perlinNoise, sampleX, sampleZ, 1 * _scale, 4, 0.76f * persistance, 2f * lacunarity);
                float peaksandvalleys_noice = Noise.Perlin2D(_perlinNoise, sampleX, sampleZ, 0.5f * _scale, 3, 0.95f * persistance, 3.1f * lacunarity);
                peaksandvalleys_noice = math.abs(peaksandvalleys_noice);
                float c = Continentalness.Evaluate(continentalness_noice);
                float e = Erosion.Evaluate(erosion_noice);
                float pv = PeaksAndValleys.Evaluate(peaksandvalleys_noice);
                float cepv = (c + (e * pv)) / 2f;
                cepv = (cepv + 1) / 2f;
                byte terrainHeight = (byte)math.round(cepv * _terrainMaxHeight);
                HeightMap.SetValue(new int3(x, 0, z), terrainHeight);
            }
        }
        IsEmpty = false;
    }
    public void Dispose()
    {
        Continentalness.Dispose();
        Erosion.Dispose();
        PeaksAndValleys.Dispose();
    }
}
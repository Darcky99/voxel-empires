using Unity.Mathematics;
using UnityEngine;

namespace VoxelEmpires.PerlinTexture
{
    public struct TextureJobParameters
    {
        public TextureJobParameters(PerlinTextureGenerator generator)
        {
            TextureSize = new int2(generator.TextureSize.x, generator.TextureSize.y);
            Seed = generator.Seed;
            Scale = generator.Scale;
        }

        public int2 TextureSize { get; private set; }
        public uint Seed;
        public float Scale;
    }
}
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static FastNoise;

public class texturegen : MonoBehaviour
{
    [SerializeField] private Vector2Int m_TextureSize;
    [SerializeField] private SpriteRenderer[] m_SpriteRenderer;

    //[SerializeField] private AnimationCurve[] m_Curves;

    [Title("General values")]
    [SerializeField, Range(100000, 999999)] private uint seed;
    [SerializeField, Range(0.00000001f, 60)] private float scale = 1f;
    [SerializeField, Range(0.01f, 5f)] private float persistance = 1;
    [SerializeField, Range(0.01f, 15f)] private float lacunarity = 1;
    [Title("Continentalness")]
    [SerializeField, Range(0.001f, 60)] private float c_scale = 17.3f;
    [SerializeField, Range(1, 10)] private int c_octaves = 3;
    [SerializeField, Range(0.01f, 5f)] private float c_persistance = 0.16f;
    [SerializeField, Range(0.01f, 15f)] private float c_lacunarity = 3.25f;
    [Title("Erosion")]
    [SerializeField, Range(0.001f, 60)] private float e_scale = 53.9f;
    [SerializeField, Range(1, 10)] private int e_octaves = 2;
    [SerializeField, Range(0.01f, 5f)] private float e_persistance = 0.33f;
    [SerializeField, Range(0.01f, 150)] private float e_lacunarity = 120f;
    [Title("P&V")]
    [SerializeField, Range(0.001f, 60)] private float pv_scale = 15.2f;
    [SerializeField, Range(1, 10)] private int pv_octaves = 3;
    [SerializeField, Range(0.01f, 5f)] private float pv_persistance = 0.83f;
    [SerializeField, Range(0.01f, 15f)] private float pv_lacunarity = 3.2f;

    [Button]
    private void generateTexture()
    {
        Texture2D texture_a = new Texture2D(m_TextureSize.x, m_TextureSize.y);
        Texture2D texture_b = new Texture2D(m_TextureSize.x, m_TextureSize.y);
        Texture2D texture_c = new Texture2D(m_TextureSize.x, m_TextureSize.y);
        Texture2D texture_d = new Texture2D(m_TextureSize.x, m_TextureSize.y);

        //FastNoiseLite noise = new FastNoiseLite((int)seed);
        //noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        for (int x = 0; x < m_TextureSize.x; x++)
            for (int y = 0; y < m_TextureSize.y; y++)
            {
                float c_noice = Noise.Perlin2D(x, y, seed, c_scale * scale, c_octaves, c_persistance * persistance, c_lacunarity * lacunarity);
                float e_noice = Noise.Perlin2D(x, y, seed, e_scale * scale, e_octaves, e_persistance * persistance, e_lacunarity * lacunarity);
                float pv_noice = Noise.Perlin2D(x, y, seed, pv_scale * scale, pv_octaves, pv_persistance * persistance, pv_lacunarity * lacunarity);

                pv_noice = math.abs(pv_noice);


                float c = GameConfig.Instance.WorldConfiguration.Continentalness.Evaluate(c_noice);
                float e = GameConfig.Instance.WorldConfiguration.Erosion.Evaluate(e_noice);
                float pv = GameConfig.Instance.WorldConfiguration.PeaksAndValleys.Evaluate(pv_noice);

                float nv = (c_noice + (e_noice * pv_noice)) / 2f;
                float n = (c + (e * pv)) / 2f;

                texture_a.SetPixel(x, y, new Color(c, c, c, 1));
                texture_b.SetPixel(x, y, new Color(e, e, e, 1));
                texture_c.SetPixel(x, y, new Color(pv, pv, pv, 1));
                texture_d.SetPixel(x, y, new Color(n, n, n, 1));
            }
        texture_a.Apply();
        texture_b.Apply();
        texture_c.Apply();
        texture_d.Apply();
        m_SpriteRenderer[0].sprite = Sprite.Create(texture_a, new Rect(0, 0, m_TextureSize.x, m_TextureSize.y), Vector2.one * 0.5f);
        m_SpriteRenderer[1].sprite = Sprite.Create(texture_b, new Rect(0, 0, m_TextureSize.x, m_TextureSize.y), Vector2.one * 0.5f);
        m_SpriteRenderer[2].sprite = Sprite.Create(texture_c, new Rect(0, 0, m_TextureSize.x, m_TextureSize.y), Vector2.one * 0.5f);
        m_SpriteRenderer[3].sprite = Sprite.Create(texture_d, new Rect(0, 0, m_TextureSize.x, m_TextureSize.y), Vector2.one * 0.5f);
    }
}

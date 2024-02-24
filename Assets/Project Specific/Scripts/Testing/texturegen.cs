using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class texturegen : MonoBehaviour
{
    [SerializeField] private Vector2Int m_TextureSize;
    [SerializeField] private SpriteRenderer[] m_SpriteRenderer;

    //[SerializeField] private AnimationCurve[] m_Curves;

    [SerializeField, Range(100000, 999999)] private uint seed;
    [SerializeField, Range(0.00000001f, 60)] private float scale = 1f;
    [SerializeField] private bool applyCurve;

    [Title("Unused")]
    [SerializeField, Range(1, 8), OnValueChanged(nameof(generateTexture))] private int octaves = 1;
    [SerializeField, Range(0.0000001f, 1f), OnValueChanged(nameof(generateTexture))] private float persistance = 0.5f;
    [SerializeField, Range(0.0000001f, 15f), OnValueChanged(nameof(generateTexture))] private float lacunarity = 0.5f;

    [Button]
    private void generateTexture()
    {
        Texture2D texture_a = new Texture2D(m_TextureSize.x, m_TextureSize.y);
        //Texture2D texture_b = new Texture2D(m_TextureSize.x, m_TextureSize.y);

        for (int x = 0; x < m_TextureSize.x; x++)
            for (int y = 0; y < m_TextureSize.y; y++)
            {
                float c_noice = Noise.Perlin2D(x, y, seed, 17.3f * scale, 3, 0.16f, 5.6f);
                float e_noice = Noise.Perlin2D(x, y, seed, 53.9f * scale, 2, 0.33f, 4.9f);
                float pv_noice = Noise.Perlin2D(x, y, seed, 15.2f * scale, 3, 0.83f, 1.6f);

                float c = applyCurve ? GameConfig.Instance.WorldConfig.PeaksAndValleys.Evaluate(c_noice) : (c_noice + 1f) / 2f;
                float e = applyCurve ? GameConfig.Instance.WorldConfig.PeaksAndValleys.Evaluate(e_noice) : (e_noice + 1f) / 2f;
                float pv = applyCurve ? GameConfig.Instance.WorldConfig.PeaksAndValleys.Evaluate(pv_noice) : (pv_noice + 1f) / 2f;

                float n = (c + (e * pv))/3;

                texture_a.SetPixel(x, y, new Color(n, n, n, 1));
            }
        texture_a.Apply();
        //texture_b.Apply();
        m_SpriteRenderer[0].sprite = Sprite.Create(texture_a, new Rect(0,0, m_TextureSize.x, m_TextureSize.y), Vector2.one * 0.5f);
        //m_SpriteRenderer[1].sprite = Sprite.Create(texture_b, new Rect(0, 0, m_TextureSize.x, m_TextureSize.y), Vector2.one * 0.5f);
    }
}

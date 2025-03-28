using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class texturegen : MonoBehaviour
{
    [SerializeField] private Vector2Int m_TextureSize;
    [SerializeField] private SpriteRenderer[] m_SpriteRenderer;

    //[SerializeField] private AnimationCurve[] m_Curves;

    [Title("General values")]
    [SerializeField] private Vector2Int m_Offset;
    [SerializeField, Range(100000, 999999)] private uint seed;
    [SerializeField, Range(0.00000001f, 100)] private float scale;
    [SerializeField, Range(0.01f, 5f)] private float persistance;
    [SerializeField, Range(0.01f, 15f)] private float lacunarity;
    [Title("Continentalness")]
    [SerializeField, Range(0.001f, 60)] private float c_scale;
    [SerializeField, Range(1, 10)] private int c_octaves;
    [SerializeField, Range(0.01f, 5f)] private float c_persistance;
    [SerializeField, Range(0.01f, 15f)] private float c_lacunarity;
    [Title("Erosion")]
    [SerializeField, Range(0.001f, 60)] private float e_scale;
    [SerializeField, Range(1, 10)] private int e_octaves;
    [SerializeField, Range(0.01f, 5f)] private float e_persistance;
    [SerializeField, Range(0.01f, 150)] private float e_lacunarity;
    [Title("P&V")]
    [SerializeField, Range(0.001f, 60)] private float pv_scale;
    [SerializeField, Range(1, 10)] private int pv_octaves;
    [SerializeField, Range(0.01f, 5f)] private float pv_persistance;
    [SerializeField, Range(0.01f, 15f)] private float pv_lacunarity;

    private void SetSprite(SpriteRenderer spriteRenderer, Texture2D texture)
    {
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        spriteRenderer.sprite.texture.filterMode = FilterMode.Point;
        spriteRenderer.sprite.texture.Apply();
    }

    public void GenerateTexture()
    {
        Texture2D texture_a = new Texture2D(m_TextureSize.x, m_TextureSize.y);
        Texture2D texture_b = new Texture2D(m_TextureSize.x, m_TextureSize.y);
        Texture2D texture_c = new Texture2D(m_TextureSize.x, m_TextureSize.y);
        Texture2D texture_d = new Texture2D(m_TextureSize.x, m_TextureSize.y);

        float halfWidth = m_TextureSize.x / 2f;
        float halfHeight = m_TextureSize.y / 2f;
        for (int x = 0; x < m_TextureSize.x; x++)
        {
            for (int y = 0; y < m_TextureSize.y; y++)
            {
                int xPos = x; //Mathf.FloorToInt((x - halfWidth) / scale) + m_Offset.x;
                int yPos = y; //Mathf.FloorToInt((y - halfHeight) / scale) + m_Offset.y;
                float c_noice = Noise.Perlin2D(xPos, yPos, seed, c_scale * scale, c_octaves, c_persistance * persistance, c_lacunarity * lacunarity);
                float e_noice = Noise.Perlin2D(xPos, yPos, seed, e_scale * scale, e_octaves, e_persistance * persistance, e_lacunarity * lacunarity);
                float pv_noice = Noise.Perlin2D(xPos, yPos, seed, pv_scale * scale, pv_octaves, pv_persistance * persistance, pv_lacunarity * lacunarity);

                pv_noice = math.abs(pv_noice);

                float c = GameConfig.Instance.WorldConfiguration.Continentalness.Evaluate(c_noice);
                float e = GameConfig.Instance.WorldConfiguration.Erosion.Evaluate(e_noice);
                float pv = GameConfig.Instance.WorldConfiguration.PeaksAndValleys.Evaluate(pv_noice);

                float cepv = (c + (e * pv)) / 2f;

                // Normalize values from -1 to 1 into 0 to 1
                // c = (c + 1) / 2f;
                // e = (e + 1) / 2f;
                // pv = (pv + 1) / 2f;
                // cepv = (cepv + 1) / 2f;

                texture_a.SetPixel(x, y, new Color(c, c, c, 1));
                texture_b.SetPixel(x, y, new Color(e, e, e, 1));
                texture_c.SetPixel(x, y, new Color(pv, pv, pv, 1));
                texture_d.SetPixel(x, y, new Color(cepv, cepv, cepv, 1));
            }
        }
        texture_a.Apply();
        texture_b.Apply();
        texture_c.Apply();
        texture_d.Apply();
        SetSprite(m_SpriteRenderer[0], texture_a);
        SetSprite(m_SpriteRenderer[1], texture_b);
        SetSprite(m_SpriteRenderer[2], texture_c);
        SetSprite(m_SpriteRenderer[3], texture_d);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(texturegen))]
public class texturegenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        texturegen gen = (texturegen)target;
        if (GUILayout.Button("Generate Texture"))
        {
            gen.GenerateTexture();
        }
    }
}
#endif
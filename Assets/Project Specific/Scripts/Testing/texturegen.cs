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

    [SerializeField] private AnimationCurve[] m_Curves;


    [Button]
    private void generateTexture()
    {
        Texture2D texture_a = new Texture2D(m_TextureSize.x, m_TextureSize.y);
        Texture2D texture_b = new Texture2D(m_TextureSize.x, m_TextureSize.y);


        Vector2
            tp = Vector2.zero;
        float f;

        for (tp.x = 0; tp.x < m_TextureSize.x; tp.x++)
            for (tp.y = 0; tp.y < m_TextureSize.y; tp.y++)
            {
                f = 0.02f;
                float a1 = noise.cnoise(tp * f);
                float a2 = noise.cnoise(tp * f * 4) / 4;
                float a3 = noise.cnoise(tp * f * 16) / 16;
                float a4 = noise.cnoise(tp * f * 64) / 64;
                float a = a1 + a2 + a3 + a4;
                
                f = 0.01f;
                float b1 = noise.snoise(tp * f);
                float b2 = noise.snoise(tp * f * 4) / 4;
                float b = b1 + b2;
                

                float na = (a + 1) / 2;
                float nb = (b + 1) / 2;

                texture_a.SetPixel((int)tp.x, (int)tp.y, new Color(na, na, na, 1));
                texture_b.SetPixel((int)tp.x, (int)tp.y, new Color(nb, nb, nb, 1));
            }
        texture_a.Apply();
        texture_b.Apply();
        m_SpriteRenderer[0].sprite = Sprite.Create(texture_a, new Rect(0,0, m_TextureSize.x, m_TextureSize.y), Vector2.one * 0.5f);
        m_SpriteRenderer[1].sprite = Sprite.Create(texture_b, new Rect(0, 0, m_TextureSize.x, m_TextureSize.y), Vector2.one * 0.5f);
    }
}

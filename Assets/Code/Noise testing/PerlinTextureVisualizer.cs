using UnityEngine;
using VE.PerlinTexture;
using System;
using Unity.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PerlinTextureVisualizer : MonoBehaviour
{
    [SerializeField] PerlinTextureGenerator _PerlinTextureGenerator;
    [SerializeField] private SpriteRenderer[] m_SpriteRenderer;

    private void OnEnable()
    {
        _PerlinTextureGenerator.TextureGenerated += PerlinTextureGenerator_GenerateTexture;
    }
    private void OnDisable()
    {
        _PerlinTextureGenerator.TextureGenerated -= PerlinTextureGenerator_GenerateTexture;
    }

    private void PerlinTextureGenerator_GenerateTexture(object sender, EventArgs e)
    {
        PerlinTextureJob perlinTextureJob = _PerlinTextureGenerator.TextureResult;
        SetSprite(m_SpriteRenderer[0], perlinTextureJob.Continentalness_TextureData);
        SetSprite(m_SpriteRenderer[1], perlinTextureJob.Erosion_TextureData);
        SetSprite(m_SpriteRenderer[2], perlinTextureJob.PeaksAndValleys_TextureData);
        SetSprite(m_SpriteRenderer[3], perlinTextureJob.Result_TextureData);
    }

    private void SetSprite(SpriteRenderer spriteRenderer, NativeArray<Color> arrayTexture)
    {
        Texture2D texture = new Texture2D(_PerlinTextureGenerator.TextureSize.x, _PerlinTextureGenerator.TextureSize.y);
        texture.SetPixels(arrayTexture.ToArray());
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        spriteRenderer.sprite.texture.filterMode = FilterMode.Point;
        spriteRenderer.sprite.texture.Apply();
    }
}

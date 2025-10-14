using System;
using UnityEngine;

namespace VE.PerlinTexture
{
    [Serializable]
    public class BiomeParameters
    {
        [SerializeField] private Color _BiomeID;
        [SerializeField] private ePerlinCombineStyle _PerlinCombineStyle;
        [SerializeField] private NoiseParameters[] _NoiseParameters;
    }
}
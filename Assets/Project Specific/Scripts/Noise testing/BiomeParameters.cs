using UnityEngine;
using VE.PerlinTexture;

public class BiomeParameters
{
    [SerializeField] private NoiseParameters[] _NoiseParameters;

    [SerializeField] private ePerlinCombineStyle _PerlinCombineStyle;
}

using System;
using UnityEngine;

namespace VE.PerlinTexture
{
    [Serializable]
    public class BiomeParameters
    {
        public NoiseParameters[] NoiseParameters
        {
            get => _NoiseParameters;
        }

        [SerializeField] private Color _BiomeID;
        [SerializeField] private ePerlinCombineStyle _PerlinCombineStyle; // Inside another class this will be converted 
        [SerializeField] private NoiseParameters[] _NoiseParameters;
    }
}
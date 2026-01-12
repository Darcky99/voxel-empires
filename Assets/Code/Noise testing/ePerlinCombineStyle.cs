using UnityEngine;

namespace VoxelEmpires.PerlinTexture
{
    /// <summary>
    /// This selects the formula to combine noises together.
    /// </summary>
    public enum ePerlinCombineStyle
    {
        ClasicContinents
    }
    public static class PerlinCombine
    {
        public static float ClasicContinents(float continentalness, float erosion, float peaksAndValleys)
        {
            return (continentalness + (erosion * peaksAndValleys)) / 2f;
        }
    }
}
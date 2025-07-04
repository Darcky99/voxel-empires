using UnityEngine;

public enum eNoiseRangeMode
{
    /// <summary>
    /// -1 to 1 and directly processed by the curve.
    /// </summary>
    Regular,
    /// <summary>
    /// Absolute function applied before being processed by the curve.
    /// </summary>
    Absolute
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VoxelBase_SO : ScriptableObject
{
    /// <summary>
    /// Break the voxel
    /// </summary>
    public abstract void LeftClick();


    /// <summary>
    /// Custom functionality
    /// </summary>
    public abstract void RightClick();
}
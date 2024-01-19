using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Solid Voxel", menuName = "Voxels/Solid")]
public class SolidVoxel_SO : VoxelBase_SO
{
    [SerializeField] private float m_Hardness;

    //we will need texture data, sound references, item drop data, and block shape...

    public override void LeftClick()
    {
        throw new System.NotImplementedException();
        
    }

    public override void RightClick()
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        return $"Solid Voxel, Hardness: {m_Hardness}";
    }
}
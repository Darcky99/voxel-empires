using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CubeVoxel", menuName = "Voxels/Cube")]
public class CubeVoxel_SO : VoxelBaseSO
{
    public override eVoxelShape GetShape() => eVoxelShape.Cube;
}
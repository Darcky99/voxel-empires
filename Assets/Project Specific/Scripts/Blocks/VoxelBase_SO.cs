using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Collections;

public abstract class VoxelBaseSO : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public bool IsTransparent { get; private set; }
    [field: SerializeField] public int[] Faces { get; private set; }

    public abstract eVoxelShape GetShape();

    public VoxelConfig GetConfig() => new VoxelConfig(this);
}
public struct VoxelConfig
{
    public VoxelConfig(VoxelBaseSO voxel)
    {
        Shape = voxel.GetShape();
        IsTransparent = voxel.IsTransparent;

        TopFace = voxel.Faces[0];
        DownFace = voxel.Faces[1];
        RightFace = voxel.Faces[2];
        LeftFace = voxel.Faces[3];
        FrontFace = voxel.Faces[4];
        BackFace = voxel.Faces[5];
    }

    public eVoxelShape Shape { get; private set; }
    public bool IsTransparent { get; private set; }

    public int TopFace { get; private set; }
    public int DownFace { get; private set; }
    public int RightFace { get; private set; }
    public int LeftFace { get; private set; }
    public int FrontFace { get; private set; }
    public int BackFace { get; private set; }
    //Sometimes I have internal faces... 7th value?

    public int TextureIndex(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0: return TopFace;
            case 1: return DownFace;
            case 2: return RightFace;
            case 3: return LeftFace;
            case 4: return FrontFace;
            case 5: return BackFace;
            default:
                Debug.LogError($"Invalid faceIndex {faceIndex}");
                return -1;
        }
    }
}
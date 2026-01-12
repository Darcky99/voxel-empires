using UnityEngine;

namespace VoxelEmpires.World
{
    public struct FacesDrawn
    {
        public FacesDrawn(bool top = false, bool bottom = false, bool right = false, bool left = false, bool front = false, bool back = false)
        {
            Top = top;
            Bottom = bottom;
            Right = right;
            Left = left;
            Front = front;
            Back = back;
        }

        public bool Top;
        public bool Bottom;
        public bool Right;
        public bool Left;
        public bool Front;
        public bool Back;

        public bool IsFaceDrawn(int faceIndex)
        {
            switch (faceIndex)
            {
                case 0: return Top;
                case 1: return Bottom;
                case 2: return Right;
                case 3: return Left;
                case 4: return Front;
                case 5: return Back;
                default:
                    Debug.LogError("Error");
                    return false;
            }
        }
        public void SetByIndex(int faceIndex)
        {
            switch (faceIndex)
            {
                case 0:
                    Top = true;
                    break;
                case 1:
                    Bottom = true;
                    break;
                case 2:
                    Right = true;
                    break;
                case 3:
                    Left = true;
                    break;
                case 4:
                    Front = true;
                    break;
                case 5:
                    Back = true;
                    break;
                default:
                    Debug.LogError("Error");
                    break;
            }
        }
    }
}
using UnityEngine;

namespace Unity.Mathematics
{
    public static class MathematicsExtensions
    {
        public static int3 Move(this int3 _, int x, int y, int z) => new int3(_.x + x, _.y + y, _.z + z);
        public static int2 Move(this int2 _, int x, int z) => new int2(_.x + x, _.y + z);
        
        public static Vector3Int ToVector3Int(this int3 _) => new Vector3Int(_.x, _.y, _.z);
        public static Vector3 ToVector3(this int3 _) => new Vector3(_.x, _.y, _.z);
        
        public static Vector3 ToVector3(this float3 _) => new Vector3(_.x, _.y, _.z);
    }
}
using UnityEngine;

namespace RTS {
    public static class ResourceManager {
        public static readonly float ScrollSpeed = 25;
        public static readonly float RotateSpeed = 100;
        public static readonly float RotateAmount = 10;
        public static readonly float ScrollWidth = 15;
        public static readonly float MinCameraHeight = 10;
        public static readonly float MaxCameraHeight = 40;
        public static readonly Vector3 InvalidPosition = new(-99999, -99999, -99999);
    }
}

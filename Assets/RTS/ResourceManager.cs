using System.Collections.Generic;
using UnityEngine;

namespace RTS {
    public static class ResourceManager {
        public static readonly float ScrollSpeed = 25;
        public static readonly float RotateSpeed = 100;
        public static readonly float RotateAmount = 10;
        public static readonly float ScrollWidth = 15;
        public static readonly float MinCameraHeight = 10;
        public static readonly float MaxCameraHeight = 40;
        public static readonly int BuildSpeed = 2;
        public static readonly Vector3 InvalidPosition = new(-99999, -99999, -99999);
        public static readonly Bounds InvalidBounds = new(new(-99999, -99999, -99999), new(0,0,0));
        public static bool MenuOpen { get; set; }

        public static GUISkin SelectBoxSkin { get; private set; }
        public static Texture2D HealthyTexture { get; private set; }
        public static Texture2D DamagedTexture { get; private set; }
        public static Texture2D CriticalTexture { get; private set; }
        public static void StoreSelectBoxItems(GUISkin skin, Texture2D healthy, Texture2D damaged, Texture2D critical) {
            SelectBoxSkin = skin;
            HealthyTexture = healthy;
            DamagedTexture = damaged;
            CriticalTexture = critical;
        }
        private static Dictionary<ResourceType, Texture2D> resourceHealthBarTextures;
        public static Texture2D GetResourceHealthBar(ResourceType resourceType) {
            if (resourceHealthBarTextures != null && resourceHealthBarTextures.ContainsKey(resourceType)) return resourceHealthBarTextures[resourceType];
            return null;
        }
        public static void SetResourceHealthBarTextures(Dictionary<ResourceType, Texture2D> images) {
            resourceHealthBarTextures = images;
        }

        private static GameObjectList gameObjectList;
        public static void SetGameObjectList(GameObjectList objectList) => gameObjectList = objectList;
        public static GameObject GetBuilding(string name) => gameObjectList.GetBuilding(name);
        public static GameObject GetUnit(string name) => gameObjectList.GetUnit(name);
        public static GameObject GetWorldObject(string name) => gameObjectList.GetWorldObject(name);
        public static GameObject GetPlayerObject() => gameObjectList.GetPlayerObject();
        public static Texture2D GetBuildImage(string name) => gameObjectList.GetBuildImage(name);

        public static readonly float ButtonHeight = 40;
        public static readonly float HeaderHeight = 32, HeaderWidth = 256;
        public static readonly float TextHeight = 25, Padding = 10;
        public static float PauseMenuHeight => HeaderHeight + 2 * ButtonHeight + 4 * Padding;
        public static float MenuWidth => HeaderWidth + 2 * Padding;
        public static float ButtonWidth => (MenuWidth - 3 * Padding) / 2;

        public static string LevelName { get; set; }

        public static int GetNewObjectId() {
            LevelLoader loader = (LevelLoader)Object.FindObjectOfType(typeof(LevelLoader));
            if (loader) return loader.GetNewObjectId();
            return -1;
        }
    }
}

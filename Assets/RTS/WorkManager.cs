using System.Collections.Generic;
using UnityEngine;

namespace RTS {
    public static class WorkManager {
        public static Rect CalculateSelectionBox(Bounds selectionBounds, Rect playingArea) {
            //shorthand for the coordinates of the centre of the selection bounds
            float cx = selectionBounds.center.x;
            float cy = selectionBounds.center.y;
            float cz = selectionBounds.center.z;
            //shorthand for the coordinates of the extents of the selection bounds
            float ex = selectionBounds.extents.x;
            float ey = selectionBounds.extents.y;
            float ez = selectionBounds.extents.z;

            //Determine the screen coordinates for the corners of the selection bounds
            List<Vector3> corners = new() {
                Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz + ez)),
                Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz - ez)),
                Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz + ez)),
                Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz + ez)),
                Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz - ez)),
                Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz + ez)),
                Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz - ez)),
                Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz - ez))
            };

            //Determine the bounds on screen for the selection bounds
            Bounds screenBounds = new(corners[0], Vector3.zero);
            for (int i = 1; i < corners.Count; i++) {
                screenBounds.Encapsulate(corners[i]);
            }

            //Screen coordinates start in the bottom left corner, rather than the top left corner
            //this correction is needed to make sure the selection box is drawn in the correct place
            float selectBoxTop = playingArea.height - (screenBounds.center.y + screenBounds.extents.y);
            float selectBoxLeft = screenBounds.center.x - screenBounds.extents.x;
            float selectBoxWidth = 2 * screenBounds.extents.x;
            float selectBoxHeight = 2 * screenBounds.extents.y;

            return new(selectBoxLeft, selectBoxTop, selectBoxWidth, selectBoxHeight);
        }

        public static GameObject FindHitObject(Vector3 origin) {
            Ray ray = Camera.main.ScreenPointToRay(origin);
            if (Physics.Raycast(ray, out RaycastHit hit)) return hit.collider.gameObject;
            return null;
        }

        public static Vector3 FindHitPoint(Vector3 origin) {
            Ray ray = Camera.main.ScreenPointToRay(origin);
            if (Physics.Raycast(ray, out RaycastHit hit)) return hit.point;
            return ResourceManager.InvalidPosition;
        }

        public static ResourceType GetResourceType(string type) => type switch {
            "Money" => ResourceType.Money,
            "Power" => ResourceType.Power,
            "Ore" => ResourceType.Ore,
            _ => ResourceType.Unknown,
        };

        public static bool ObjectIsGround(GameObject obj) => obj.name == "Ground" || obj.name == "Ground(Clone)";

        public static List<WorldObject> FindNearbyObjects(Vector3 position, float range) {
            Collider[] hitColliders = Physics.OverlapSphere(position, range);
            HashSet<int> nearbyObjectIds = new();
            List<WorldObject> nearbyObjects = new();
            for (int i = 0; i < hitColliders.Length; i++) {
                Transform parent = hitColliders[i].transform.parent;
                if (parent) {
                    WorldObject parentObject = parent.GetComponent<WorldObject>();
                    if (parentObject && !nearbyObjectIds.Contains(parentObject.ObjectId)) {
                        nearbyObjectIds.Add(parentObject.ObjectId);
                        nearbyObjects.Add(parentObject);
                    }
                }
            }
            return nearbyObjects;
        }

        public static WorldObject FindNearestWorldObjectInListToPosition(List<WorldObject> objects, Vector3 position) {
            if (objects == null || objects.Count == 0) return null;
            WorldObject nearestObject = objects[0];
            float distanceToNearestObject = Vector3.Distance(position, nearestObject.transform.position);
            for (int i = 1; i < objects.Count; i++) {
                float distanceToObject = Vector3.Distance(position, objects[i].transform.position);
                if (distanceToObject < distanceToNearestObject) {
                    distanceToNearestObject = distanceToObject;
                    nearestObject = objects[i];
                }
            }
            return nearestObject;
        }
        
    }
}

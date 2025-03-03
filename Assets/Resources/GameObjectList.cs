using RTS;
using UnityEngine;

public class GameObjectList : MonoBehaviour {
    public GameObject[] buildings;
    public GameObject[] units;
    public GameObject[] worldObjects;
    public GameObject player;
    private static bool created = false;

    void Awake() {
        if (!created) {
            DontDestroyOnLoad(transform.gameObject);
            ResourceManager.SetGameObjectList(this);
            created = true;
        } else {
            Destroy(gameObject);
        }
    }

    public GameObject GetBuilding(string name) {
        for (int i = 0; i < buildings.Length; i++) {
            Building building = buildings[i].GetComponent<Building>();
            if (building && building.name == name) return buildings[i];
        }
        Debug.LogWarning($"[GetBuilding] `{name}` not found!");
        return null;
    }

    public GameObject GetUnit(string name) {
        for (int i = 0; i < units.Length; i++) {
            Unit unit = units[i].GetComponent<Unit>();
            if (unit != null && unit.name == name) {
                return units[i];
            }
        }
        Debug.LogWarning($"[GetUnit] `{name}` not found!");
        return null;
    }

    public GameObject GetWorldObject(string name) {
        foreach (GameObject worldObject in worldObjects) {
            if (worldObject.name == name) return worldObject;
        }
        Debug.LogWarning($"[GetWorldObject] `{name}` not found!");
        return null;
    }

    public GameObject GetPlayerObject() => player;

    public Texture2D GetBuildImage(string name) {
        for (int i = 0; i < buildings.Length; i++) {
            Building building = buildings[i].GetComponent<Building>();
            if (building && building.name == name) return building.buildImage;
        }
        for (int i = 0; i < units.Length; i++) {
            Unit unit = units[i].GetComponent<Unit>();
            if (unit && unit.name == name) return unit.buildImage;
        }
        Debug.LogWarning($"[GetBuildImage] `{name}` not found!");
        return null;
    }

}

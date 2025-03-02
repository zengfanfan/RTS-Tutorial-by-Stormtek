using System.Collections.Generic;
using Newtonsoft.Json;
using RTS;
using UnityEngine;

public class Player : MonoBehaviour {
    public string username;
    public bool human;
    public HUD hud;
    public WorldObject SelectedObject { get; set; }
    public int startMoney, startMoneyLimit, startPower, startPowerLimit;
    public Material notAllowedMaterial, allowedMaterial;
    public Color teamColor;

    private Dictionary<ResourceType, int> resources, resourceLimits;
    private Building tempBuilding;
    private Unit tempCreator;
    private bool findingPlacement = false;

    void Awake() {
        resources = InitResourceList();
        resourceLimits = InitResourceList();
    }

    private Dictionary<ResourceType, int> InitResourceList() {
        Dictionary<ResourceType, int> list = new() {
            { ResourceType.Money, 0 },
            { ResourceType.Power, 0 }
        };
        return list;
    }

    void Start() {
        hud = GetComponentInChildren<HUD>();
        AddStartResourceLimits();
        AddStartResources();
    }

    void Update() {
        if (human) {
            hud.SetResourceValues(resources, resourceLimits);
        }
        if (findingPlacement) {
            tempBuilding.CalculateBounds();
            if (CanPlaceBuilding()) tempBuilding.SetTransparentMaterial(allowedMaterial, false);
            else tempBuilding.SetTransparentMaterial(notAllowedMaterial, false);
        }
    }

    private void AddStartResourceLimits() {
        IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
        IncrementResourceLimit(ResourceType.Power, startPowerLimit);
    }

    private void AddStartResources() {
        AddResource(ResourceType.Money, startMoney);
        AddResource(ResourceType.Power, startPower);
    }

    public void AddResource(ResourceType type, int amount) => resources[type] += amount;
    public void IncrementResourceLimit(ResourceType type, int amount) => resourceLimits[type] += amount;

    public void AddUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation, Building creator) {
        var units = GetComponentInChildren<Units>();
        var newUnit = Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation, units.transform);
        var unitObject = newUnit.GetComponent<Unit>();
        if (unitObject && spawnPoint != rallyPoint) unitObject.StartMove(rallyPoint);
        if (unitObject) {
            unitObject.SetBuilding(creator);
            unitObject.ObjectId = ResourceManager.GetNewObjectId();
            if (spawnPoint != rallyPoint) unitObject.StartMove(rallyPoint);
        }
    }

    public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea) {
        GameObject newBuilding = Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
        tempBuilding = newBuilding.GetComponent<Building>();
        if (tempBuilding) {
            tempBuilding.ObjectId = ResourceManager.GetNewObjectId();
            tempCreator = creator;
            findingPlacement = true;
            tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
            tempBuilding.SetColliders(false);
            tempBuilding.SetPlayingArea(playingArea);
        } else Destroy(newBuilding);
    }

    public bool IsFindingBuildingLocation() => findingPlacement;

    public void FindBuildingLocation() {
        Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
        newLocation.y = 0;
        tempBuilding.transform.position = newLocation;
    }

    public bool CanPlaceBuilding() {
        bool canPlace = true;

        Bounds placeBounds = tempBuilding.GetSelectionBounds();
        //shorthand for the coordinates of the center of the selection bounds
        float cx = placeBounds.center.x;
        float cy = placeBounds.center.y;
        float cz = placeBounds.center.z;
        //shorthand for the coordinates of the extents of the selection box
        float ex = placeBounds.extents.x;
        float ey = placeBounds.extents.y;
        float ez = placeBounds.extents.z;

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

        foreach (Vector3 corner in corners) {
            GameObject hitObject = WorkManager.FindHitObject(corner);
            if (hitObject && hitObject.name != "Ground") {
                WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
                if (worldObject && placeBounds.Intersects(worldObject.GetSelectionBounds())) canPlace = false;
            }
        }
        return canPlace;
    }

    public void StartConstruction() {
        findingPlacement = false;
        Buildings buildings = GetComponentInChildren<Buildings>();
        if (buildings) tempBuilding.transform.parent = buildings.transform;
        tempBuilding.SetPlayer();
        tempBuilding.SetColliders(true);
        tempCreator.SetBuilding(tempBuilding);
        tempBuilding.StartConstruction();
    }

    public void CancelBuildingPlacement() {
        findingPlacement = false;
        Destroy(tempBuilding.gameObject);
        tempBuilding = null;
        tempCreator = null;
    }

    public virtual void SaveDetails(JsonWriter writer) {
        SaveManager.WriteString(writer, "Username", username);
        SaveManager.WriteBoolean(writer, "Human", human);
        SaveManager.WriteColor(writer, "TeamColor", teamColor);
        SaveManager.SavePlayerResources(writer, resources);
        SaveManager.SavePlayerBuildings(writer, GetComponentsInChildren<Building>());
        SaveManager.SavePlayerUnits(writer, GetComponentsInChildren<Unit>());
    }

}

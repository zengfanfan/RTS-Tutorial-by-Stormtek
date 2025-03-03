using System.Collections.Generic;
using Newtonsoft.Json;
using RTS;
using UnityEngine;

public class Building : WorldObject {
    public float maxBuildProgress = 10;
    protected Queue<string> buildQueue = new();
    private float currentBuildProgress = 0.0f;
    private Vector3 spawnPoint;
    protected Vector3 rallyPoint;
    public Texture2D rallyPointImage;
    public Texture2D sellImage;
    private bool needsBuilding = false;

    protected override void Awake() {
        base.Awake();
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
        rallyPoint = spawnPoint = new(spawnX, 0.0f, spawnZ);
    }

    protected override void Start() => base.Start();
    protected override void Update() {
        base.Update();
        ProcessBuildQueue();
    }

    protected override void OnGUI() {
        base.OnGUI();
        if (needsBuilding) DrawBuildProgress();
    }

    private void DrawBuildProgress() {
        GUI.skin = ResourceManager.SelectBoxSkin;
        Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
        //Draw the selection box around the currently selected object, within the bounds of the main draw area
        GUI.BeginGroup(playingArea);
        CalculateCurrentHealth(0.5f, 0.99f);
        DrawHealthBar(selectBox, "Building ...");
        GUI.EndGroup();
    }

    protected void CreateUnit(string unitName) => buildQueue.Enqueue(unitName);

    protected void ProcessBuildQueue() {
        if (buildQueue.Count > 0) {
            currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
            if (currentBuildProgress > maxBuildProgress) {
                if (player) player.AddUnit(buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, this);
                currentBuildProgress = 0.0f;
            }
        }
    }

    public string[] GetBuildQueueValues() {
        string[] values = new string[buildQueue.Count];
        int pos = 0;
        foreach (string unit in buildQueue) values[pos++] = unit;
        return values;
    }

    public float GetBuildPercentage() => currentBuildProgress / maxBuildProgress;

    public override void SetSelection(bool selected, Rect playingArea) {
        base.SetSelection(selected, playingArea);
        if (player) {
            RallyPoint flag = player.GetComponentInChildren<RallyPoint>(true);
            if (selected) {
                if (flag && player.human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition) {
                    flag.transform.localPosition = rallyPoint;
                    flag.transform.forward = transform.forward;
                    flag.gameObject.SetActive(true);
                }
            } else {
                if (flag && player.human) flag.gameObject.SetActive(false);
            }
        }
    }

    public bool HasSpawnPoint() => spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;

    public override void SetHoverState(GameObject hoverObject) {
        base.SetHoverState(hoverObject);
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected) {
            if (WorkManager.ObjectIsGround(hoverObject)) {
                if (player.hud.GetPreviousCursorState() == CursorState.RallyPoint) player.hud.SetCursorState(CursorState.RallyPoint);
            }
        }
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        base.MouseClick(hitObject, hitPoint, controller);
        //only handle iput if owned by a human player and currently selected
        if (player && player.human && currentlySelected) {
            if (WorkManager.ObjectIsGround(hitObject)) {
                if ((player.hud.GetCursorState() == CursorState.RallyPoint || player.hud.GetPreviousCursorState() == CursorState.RallyPoint) && hitPoint != ResourceManager.InvalidPosition) {
                    SetRallyPoint(hitPoint);
                }
            }
        }
    }

    public void SetRallyPoint(Vector3 position) {
        rallyPoint = position;
        if (player && player.human && currentlySelected) {
            RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
            if (flag) flag.transform.localPosition = rallyPoint;
        }
    }

    public void Sell() {
        if (player) player.AddResource(ResourceType.Money, sellValue);
        if (currentlySelected) SetSelection(false, playingArea);
        Destroy(gameObject);
    }

    public void StartConstruction() {
        CalculateBounds();
        needsBuilding = true;
        hitPoints = 0;
    }

    public bool UnderConstruction() => needsBuilding;

    public void Construct(int amount) {
        hitPoints += amount;
        if (hitPoints >= maxHitPoints) {
            hitPoints = maxHitPoints;
            needsBuilding = false;
            RestoreMaterials();
        }
    }

    public override void SaveDetails(JsonWriter writer) {
        base.SaveDetails(writer);
        SaveManager.WriteBoolean(writer, "NeedsBuilding", needsBuilding);
        SaveManager.WriteVector(writer, "SpawnPoint", spawnPoint);
        SaveManager.WriteVector(writer, "RallyPoint", rallyPoint);
        SaveManager.WriteFloat(writer, "BuildProgress", currentBuildProgress);
        SaveManager.WriteStringArray(writer, "BuildQueue", buildQueue.ToArray());
        if (needsBuilding) SaveManager.WriteRect(writer, "PlayingArea", playingArea);
    }

    protected override void HandleLoadedProperty(JsonTextReader reader, string propertyName, object readValue) {
        base.HandleLoadedProperty(reader, propertyName, readValue);
        switch (propertyName) {
        case "NeedsBuilding": needsBuilding = (bool)readValue; break;
        case "SpawnPoint": spawnPoint = LoadManager.LoadVector(reader); break;
        case "RallyPoint": rallyPoint = LoadManager.LoadVector(reader); break;
        case "BuildProgress": currentBuildProgress = (float)(double)readValue; break;
        case "BuildQueue": buildQueue = new Queue<string>(LoadManager.LoadStringArray(reader)); break;
        case "PlayingArea": playingArea = LoadManager.LoadRect(reader); break;
        default: break;
        }
    }

}

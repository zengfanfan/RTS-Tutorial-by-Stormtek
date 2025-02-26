using System.Collections.Generic;
using RTS;
using UnityEngine;

public class Building : WorldObject {
    public float maxBuildProgress = 10;
    protected Queue<string> buildQueue = new();
    private float currentBuildProgress = 0.0f;
    private Vector3 spawnPoint;

    protected override void Awake() {
        base.Awake();
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new(spawnX, 0.0f, spawnZ);
    }

    protected override void Start() => base.Start();
    protected override void Update() {
        base.Update();
        ProcessBuildQueue();
    }

    protected override void OnGUI() => base.OnGUI();

    protected void CreateUnit(string unitName) => buildQueue.Enqueue(unitName);

    protected void ProcessBuildQueue() {
        if (buildQueue.Count > 0) {
            currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
            if (currentBuildProgress > maxBuildProgress) {
                if (player) player.AddUnit(buildQueue.Dequeue(), spawnPoint, transform.rotation);
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

    public float GetBuildPercentage() {
        return currentBuildProgress / maxBuildProgress;
    }

}

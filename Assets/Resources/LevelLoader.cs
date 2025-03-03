using System;
using RTS;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Singleton that handles loading level details. This includes making sure
 * that all world objects have an objectId set.
 */

public class LevelLoader : MonoBehaviour {

    private static int nextObjectId = 0;
    private static bool created = false;
    private bool initialised = false;

    void Awake() {
        if (!created) {
            DontDestroyOnLoad(transform.gameObject);
            created = true;
            initialised = true;
        } else {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += (a,b) => OnLevelWasLoadedZ();
    }

    void OnLevelWasLoadedZ() {
        if (initialised) {
            if (ResourceManager.LevelName != null && ResourceManager.LevelName != "") {
                LoadManager.LoadGame(ResourceManager.LevelName);
            } else {
                WorldObject[] worldObjects = FindObjectsOfType(typeof(WorldObject)) as WorldObject[];
                foreach (WorldObject worldObject in worldObjects) {
                    worldObject.ObjectId = nextObjectId++;
                    if (nextObjectId >= int.MaxValue) nextObjectId = 0;
                }
            }
        }
    }

    public int GetNewObjectId() {
        nextObjectId++;
        if (nextObjectId >= int.MaxValue) nextObjectId = 0;
        return nextObjectId;
    }
}

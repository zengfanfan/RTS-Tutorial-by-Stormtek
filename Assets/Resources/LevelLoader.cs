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
            SceneManager.sceneLoaded += (a,b) => OnLevelWasLoadedZ();
        } else {
            Destroy(gameObject);
        }

        if (initialised) {
            SelectPlayerMenu menu = FindObjectOfType(typeof(SelectPlayerMenu)) as SelectPlayerMenu;
            if (!menu) {
                //we have started from inside a map, rather than the main menu
                //this happens if we launch Unity from inside a map file for testing
                Player[] players = FindObjectsOfType(typeof(Player)) as Player[];
                foreach (Player player in players) {
                    if (player.human) {
                        PlayerManager.SelectPlayer(player.username, 0);
                    }
                }
                SetObjectIds();
            }
        }
    }

    void OnLevelWasLoadedZ() {
        if (initialised) {
            if (ResourceManager.LevelName != null && ResourceManager.LevelName != "") {
                LoadManager.LoadGame(ResourceManager.LevelName);
            } else {
                SetObjectIds();
            }
            Time.timeScale = 1.0f;
            ResourceManager.MenuOpen = false;
        }
    }

    private void SetObjectIds() {
        WorldObject[] worldObjects = FindObjectsOfType(typeof(WorldObject)) as WorldObject[];
        foreach (WorldObject worldObject in worldObjects) {
            worldObject.ObjectId = nextObjectId++;
            if (nextObjectId >= int.MaxValue) nextObjectId = 0;
        }
    }

    public int GetNewObjectId() {
        nextObjectId++;
        if (nextObjectId >= int.MaxValue) nextObjectId = 0;
        return nextObjectId;
    }
}

using UnityEngine;
using RTS;
using UnityEngine.SceneManagement;

/**
 * Singleton that handles the management of game state. This includes
 * detecting when a game has been finished and what to do from there.
 */

public class GameManager : MonoBehaviour {
    private static bool created = false;
    private bool initialised = false;
    private VictoryCondition[] victoryConditions;
    private HUD hud;

    void Awake() {
        if (!created) {
            DontDestroyOnLoad(transform.gameObject);
            created = true;
            initialised = true;
            SceneManager.sceneLoaded += (a, b) => OnLevelWasLoadedZ();
        } else {
            Destroy(gameObject);
        }
        if (initialised) {
            LoadDetails();
        }
    }

    void OnLevelWasLoadedZ() {
        if (initialised) {
            LoadDetails();
        }
    }

    private void LoadDetails() {
        Player[] players = FindObjectsOfType(typeof(Player)) as Player[];
        foreach (Player player in players) {
            if (player.human) hud = player.GetComponentInChildren<HUD>();
        }
        victoryConditions = FindObjectsOfType(typeof(VictoryCondition)) as VictoryCondition[];
        if (victoryConditions != null) {
            foreach (VictoryCondition victoryCondition in victoryConditions) {
                victoryCondition.SetPlayers(players);
            }
        }
    }

    void Update() {
        if (victoryConditions != null) {
            foreach (VictoryCondition victoryCondition in victoryConditions) {
                if (victoryCondition.GameFinished()) {
                    var resultsScreen = hud.GetComponent<ResultsScreen>();
                    resultsScreen.SetMetVictoryCondition(victoryCondition);
                    resultsScreen.enabled = true;
                    Time.timeScale = 0.0f;
                    Cursor.visible = true;
                    ResourceManager.MenuOpen = true;
                    hud.enabled = false;
                }
            }
        }
    }

}

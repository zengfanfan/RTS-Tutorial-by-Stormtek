using UnityEngine;
using System.Collections.Generic;
using RTS;
using UnityEngine.SceneManagement;

public class ResultsScreen : MonoBehaviour {

    public GUISkin skin;
    public AudioClip clickSound;
    public float clickVolume = 1.0f;

    private AudioElement audioElement;
    private Player winner;
    private VictoryCondition metVictoryCondition;

    void Start() {
        List<AudioClip> sounds = new();
        List<float> volumes = new();
        sounds.Add(clickSound);
        volumes.Add(clickVolume);
        audioElement = new(sounds, volumes, "ResultsScreen", null);
    }

    void OnGUI() {
        GUI.skin = skin;

        GUI.BeginGroup(new(0, 0, Screen.width, Screen.height));

        //display
        float padding = ResourceManager.Padding;
        float itemHeight = ResourceManager.ButtonHeight;
        float buttonWidth = ResourceManager.ButtonWidth;
        float leftPos = padding;
        float topPos = padding;
        GUI.Box(new(0, 0, Screen.width, Screen.height), "");
        string message = "Game Over";
        if (winner) message = "Congratulations " + winner.username + "! You have won by " + metVictoryCondition.GetDescription();
        GUI.Label(new(leftPos, topPos, Screen.width - 2 * padding, itemHeight), message);
        leftPos = Screen.width / 2 - padding / 2 - buttonWidth;
        topPos += itemHeight + padding;
        if (GUI.Button(new(leftPos, topPos, buttonWidth, itemHeight), "New Game")) {
            PlayClick();
            //makes sure that the loaded level runs at normal speed
            Time.timeScale = 1.0f;
            ResourceManager.MenuOpen = false;
            SceneManager.LoadScene("Map");
        }
        leftPos += padding + buttonWidth;
        if (GUI.Button(new(leftPos, topPos, buttonWidth, itemHeight), "Main Menu")) {
            ResourceManager.LevelName = "";
            SceneManager.LoadScene("MainMenu");
            Cursor.visible = true;
        }

        GUI.EndGroup();
    }

    private void PlayClick() => audioElement?.Play(clickSound);

    public void SetMetVictoryCondition(VictoryCondition victoryCondition) {
        if (!victoryCondition) return;
        metVictoryCondition = victoryCondition;
        winner = metVictoryCondition.GetWinner();
    }

}

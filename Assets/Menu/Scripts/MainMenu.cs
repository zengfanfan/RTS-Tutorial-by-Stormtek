using UnityEngine;
using RTS;
using UnityEngine.SceneManagement;

public class MainMenu : Menu {

    void Awake() => OnLevelWasLoadedZ();

    protected override void SetButtons() => buttons = new string[] { "New Game", "Change Player", "Quit Game" };

    protected override void HandleButton(string text) {
        switch (text) {
        case "New Game": NewGame(); break;
        case "Change Player": ChangePlayer(); break;
        case "Quit Game": ExitGame(); break;
        default: break;
        }
    }

    private void NewGame() {
        ResourceManager.MenuOpen = false;
        SceneManager.LoadScene("Map");
        //makes sure that the loaded level runs at normal speed
        Time.timeScale = 1.0f;
    }

    void OnLevelWasLoadedZ() {
        Cursor.visible = true;
        if (PlayerManager.GetPlayerName() == "Unknown") {
            //no player yet selected so enable SetPlayerMenu
            GetComponent<MainMenu>().enabled = false;
            GetComponent<SelectPlayerMenu>().enabled = true;
        } else {
            //player selected so enable MainMenu
            GetComponent<MainMenu>().enabled = true;
            GetComponent<SelectPlayerMenu>().enabled = false;
        }
    }

    private void ChangePlayer() {
        GetComponent<MainMenu>().enabled = false;
        GetComponent<SelectPlayerMenu>().enabled = true;
        SelectionList.LoadEntries(PlayerManager.GetPlayerNames());
    }
}

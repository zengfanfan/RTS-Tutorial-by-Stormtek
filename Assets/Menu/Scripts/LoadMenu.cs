using UnityEngine;
using RTS;
using UnityEngine.SceneManagement;

public class LoadMenu : MonoBehaviour {

    public GUISkin mainSkin, selectionSkin;

    void Start() => Activate();

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) CancelLoad();
    }

    void OnGUI() {
        if (SelectionList.MouseDoubleClick()) StartLoad();

        GUI.skin = mainSkin;
        float menuHeight = GetMenuHeight();
        float groupLeft = Screen.width / 2 - ResourceManager.MenuWidth / 2;
        float groupTop = Screen.height / 2 - menuHeight / 2;
        Rect groupRect = new(groupLeft, groupTop, ResourceManager.MenuWidth, menuHeight);
        GUI.BeginGroup(groupRect);
        //background box
        GUI.Box(new(0, 0, ResourceManager.MenuWidth, menuHeight), "");
        //menu buttons
        float leftPos = ResourceManager.Padding;
        float topPos = menuHeight - ResourceManager.Padding - ResourceManager.ButtonHeight;
        if (GUI.Button(new(leftPos, topPos, ResourceManager.ButtonWidth, ResourceManager.ButtonHeight), "Load Game")) {
            StartLoad();
        }
        leftPos += ResourceManager.ButtonWidth + ResourceManager.Padding;
        if (GUI.Button(new(leftPos, topPos, ResourceManager.ButtonWidth, ResourceManager.ButtonHeight), "Cancel")) {
            CancelLoad();
        }
        GUI.EndGroup();

        //selection list, needs to be called outside of the group for the menu
        float selectionLeft = groupRect.x + ResourceManager.Padding;
        float selectionTop = groupRect.y + ResourceManager.Padding;
        float selectionWidth = groupRect.width - 2 * ResourceManager.Padding;
        float selectionHeight = groupRect.height - GetMenuItemsHeight() - ResourceManager.Padding;
        SelectionList.Draw(selectionLeft, selectionTop, selectionWidth, selectionHeight, selectionSkin);
    }

    private float GetMenuHeight() => 250 + GetMenuItemsHeight();
    private float GetMenuItemsHeight() => ResourceManager.ButtonHeight + 2 * ResourceManager.Padding;

    public void Activate() => SelectionList.LoadEntries(PlayerManager.GetSavedGames());

    private void StartLoad() {
        string newLevel = SelectionList.GetCurrentEntry();
        if (newLevel != "") {
            ResourceManager.LevelName = newLevel;
            var scenceName = SceneManager.GetActiveScene().name;
            if (scenceName != "BlankMap1") SceneManager.LoadScene("BlankMap1");
            else if (scenceName != "BlankMap2") SceneManager.LoadScene("BlankMap2");
            //makes sure that the loaded level runs at normal speed
            Time.timeScale = 1.0f;
        }
    }

    private void CancelLoad() {
        GetComponent<LoadMenu>().enabled = false;
        PauseMenu pause = GetComponent<PauseMenu>();
        if (pause) pause.enabled = true;
        else {
            MainMenu main = GetComponent<MainMenu>();
            if (main) main.enabled = true;
        }
    }

}

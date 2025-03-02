using System.Collections;
using System.Collections.Generic;
using RTS;
using UnityEngine;

public class HUD : MonoBehaviour {
    const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
    const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
    const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
    const int BUTTON_SPACING = 7;
    const int SCROLL_BAR_WIDTH = 22;
    const int BUILD_IMAGE_PADDING = 8;

    public GUISkin mouseCursorSkin;
    public GUISkin resourceSkin, ordersSkin, selectBoxSkin;
    public Texture2D activeCursor;
    public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;
    public Texture2D[] resources;
    public Texture2D buttonHover, buttonClick;
    public Texture2D buildFrame, buildMask;
    public Texture2D smallButtonHover, smallButtonClick;
    public Texture2D rallyPointCursor;
    public Texture2D healthy, damaged, critical;
    public Texture2D[] resourceHealthBars;
    public GUISkin playerDetailsSkin;

    private readonly Dictionary<ResourceType, Texture2D> resourceImages = new();
    private Dictionary<ResourceType, int> resourceValues = new(), resourceLimits = new();
    private CursorState activeCursorState;
    private int mouseFrame = 0;
    private WorldObject lastSelection;
    private float sliderValue;
    private Player player;
    private int buildAreaHeight = 0;
    private CursorState previousCursorState;

    void Start() {
        buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;

        for (int i = 0; i < resources.Length; i++) {
            switch (resources[i].name) {
            case "Money":
                resourceImages.Add(ResourceType.Money, resources[i]);
                resourceValues.Add(ResourceType.Money, 0);
                resourceLimits.Add(ResourceType.Money, 0);
                break;
            case "Power":
                resourceImages.Add(ResourceType.Power, resources[i]);
                resourceValues.Add(ResourceType.Power, 0);
                resourceLimits.Add(ResourceType.Power, 0);
                break;
            default: break;
            }
        }

        player = transform.root.GetComponent<Player>();
        ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);

        Dictionary<ResourceType, Texture2D> resourceHealthBarTextures = new Dictionary<ResourceType, Texture2D>();
        for (int i = 0; i < resourceHealthBars.Length; i++) {
            switch (resourceHealthBars[i].name) {
            case "ore":
                resourceHealthBarTextures.Add(ResourceType.Ore, resourceHealthBars[i]);
                break;
            default: break;
            }
        }
        ResourceManager.SetResourceHealthBarTextures(resourceHealthBarTextures);

        SetCursorState(CursorState.Select);
        StartCoroutine(MouseAnimationTimer());
    }

    private IEnumerator MouseAnimationTimer() {
        for (; ; ) {
            yield return new WaitForSecondsRealtime(1f / 3);
            mouseFrame++;
        }
    }

    void OnGUI() {
        if (player && player.human) {
            DrawOrdersBar();
            DrawResourceBar();
            DrawMouseCursor();
            DrawPlayerDetails();
        }
    }

    private void DrawPlayerDetails() {
        GUI.skin = playerDetailsSkin;
        GUI.BeginGroup(new(0, 0, Screen.width, Screen.height));
        float height = ResourceManager.TextHeight;
        float leftPos = ResourceManager.Padding;
        float topPos = Screen.height - height - ResourceManager.Padding;
        Texture2D avatar = PlayerManager.GetPlayerAvatar();
        if (avatar) {
            //we want the texture to be drawn square at all times
            GUI.DrawTexture(new(leftPos, topPos, height, height), avatar);
            leftPos += height + ResourceManager.Padding;
        }
        float minWidth = 0, maxWidth = 0;
        string playerName = PlayerManager.GetPlayerName();
        playerDetailsSkin.GetStyle("label").CalcMinMaxWidth(new GUIContent(playerName), out minWidth, out maxWidth);
        GUI.Label(new(leftPos, topPos, maxWidth, height), playerName);
        GUI.EndGroup();
    }

    private void DrawResourceBar() {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT));

        GUI.Box(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
        int topPos = 4, iconLeft = 4, textLeft = 20;
        DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
        iconLeft += TEXT_WIDTH;
        textLeft += TEXT_WIDTH;
        DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);

        GUI.EndGroup();
    }

    private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
        Texture2D icon = resourceImages[type];
        string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
        GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
        GUI.Label(new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
    }

    private const int SELECTION_NAME_HEIGHT = 15;
    private void DrawOrdersBar() {
        GUI.skin = ordersSkin;
        GUI.BeginGroup(new(Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));

        GUI.Box(new(BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");

        string selectionName = "";
        if (player.SelectedObject) {
            selectionName = player.SelectedObject.objectName;
            if (player.SelectedObject.IsOwnedBy(player)) {
                //reset slider value if the selected object has changed
                if (lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
                DrawActions(player.SelectedObject.GetActions());
                //store the current selection
                lastSelection = player.SelectedObject;

                Building selectedBuilding = lastSelection.GetComponent<Building>();
                if (selectedBuilding) {
                    DrawBuildQueue(selectedBuilding.GetBuildQueueValues(), selectedBuilding.GetBuildPercentage());
                    DrawStandardBuildingOptions(selectedBuilding);
                }
            }
        }
        if (!selectionName.Equals("")) {
            int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
            int topPos = buildAreaHeight + BUTTON_SPACING;
            GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
        }

        GUI.EndGroup();
    }

    private void DrawStandardBuildingOptions(Building building) {
        GUIStyle buttons = new();
        buttons.hover.background = smallButtonHover;
        buttons.active.background = smallButtonClick;
        GUI.skin.button = buttons;
        int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
        int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
        int width = BUILD_IMAGE_WIDTH / 2;
        int height = BUILD_IMAGE_HEIGHT / 2;
        if (building.HasSpawnPoint()) {
            if (GUI.Button(new(leftPos, topPos, width, height), building.sellImage)) {
                building.Sell();
            }
            leftPos += width + BUTTON_SPACING;
            if (GUI.Button(new(leftPos, topPos, width, height), building.rallyPointImage)) {
                if (activeCursorState != CursorState.RallyPoint && previousCursorState != CursorState.RallyPoint) {
                    SetCursorState(CursorState.RallyPoint);
                } else {
                    //dirty hack to ensure toggle between RallyPoint and not works ...
                    SetCursorState(CursorState.PanRight);
                    SetCursorState(CursorState.Select);
                }
            }
        }
    }

    private void DrawBuildQueue(string[] buildQueue, float buildPercentage) {
        for (int i = 0; i < buildQueue.Length; i++) {
            float topPos = i * BUILD_IMAGE_HEIGHT - (i + 1) * BUILD_IMAGE_PADDING;
            Rect buildPos = new(BUILD_IMAGE_PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
            GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
            GUI.DrawTexture(buildPos, buildFrame);
            topPos += BUILD_IMAGE_PADDING;
            float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
            float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
            if (i == 0) {
                //shrink the build mask on the item currently being built to give an idea of progress
                topPos += height * buildPercentage;
                height *= 1 - buildPercentage;
            }
            GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING, topPos, width, height), buildMask);
        }
    }

    private void DrawActions(string[] actions) {
        GUIStyle buttons = new();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        GUI.skin.button = buttons;
        int numActions = actions.Length;
        //define the area to draw the actions inside
        GUI.BeginGroup(new(BUILD_IMAGE_WIDTH, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
        //draw scroll bar for the list of actions if need be
        if (numActions >= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
        //display possible actions as buttons and handle the button click for each
        for (int i = 0; i < numActions; i++) {
            int column = i % 2;
            int row = i / 2;
            Rect pos = GetButtonPos(row, column);
            Texture2D action = ResourceManager.GetBuildImage(actions[i]);
            if (action) {
                //create the button and handle the click of that button
                if (GUI.Button(pos, action)) {
                    if (player.SelectedObject) player.SelectedObject.PerformAction(actions[i]);
                }
            }
        }
        GUI.EndGroup();
    }

    private int MaxNumRows(int areaHeight) => areaHeight / BUILD_IMAGE_HEIGHT;

    private Rect GetButtonPos(int row, int column) {
        int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
        float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
        return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
    }

    private void DrawSlider(int groupHeight, float numRows) {
        //slider goes from 0 to the number of rows that do not fit on screen
        sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0.0f, numRows - MaxNumRows(groupHeight));
    }

    private Rect GetScrollPos(int groupHeight) => new(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);

    public bool MouseInBounds() {
        //Screen coordinates start in the lower-left corner of the screen
        //not the top-left of the screen like the drawing coordinates do
        Vector3 mousePos = Input.mousePosition;
        bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
        bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
        return insideWidth && insideHeight;
    }

    public Rect GetPlayingArea() => new(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);

    private void DrawMouseCursor() {
        bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
        if (mouseOverHud) {
            Cursor.visible = true;
        } else {
            Cursor.visible = false;
            if (!player.IsFindingBuildingLocation()) {
                GUI.skin = mouseCursorSkin;
                GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
                UpdateCursorAnimation();
                Rect cursorPosition = GetCursorDrawPosition();
                GUI.Label(cursorPosition, activeCursor);
                GUI.EndGroup();
            }
        }
    }

    private void UpdateCursorAnimation() {
        //sequence animation for cursor (based on more than one image for the cursor)
        //change once per second, loops through array of images
        if (activeCursorState == CursorState.Move) {
            activeCursor = moveCursors[mouseFrame % moveCursors.Length];
        } else if (activeCursorState == CursorState.Attack) {
            activeCursor = attackCursors[mouseFrame % attackCursors.Length];
        } else if (activeCursorState == CursorState.Harvest) {
            activeCursor = harvestCursors[mouseFrame % harvestCursors.Length];
        }
    }

    private Rect GetCursorDrawPosition() {
        //set base position for custom cursor image
        float leftPos = Input.mousePosition.x;
        float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
                                                              //adjust position base on the type of cursor being shown
        if (activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
        else if (activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
        else if (activeCursorState == CursorState.RallyPoint) topPos -= activeCursor.height;
        else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
            topPos -= activeCursor.height / 2;
            leftPos -= activeCursor.width / 2;
        }
        return new(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    public void SetCursorState(CursorState newState) {
        if (activeCursorState != newState) previousCursorState = activeCursorState;
        activeCursorState = newState;
        switch (newState) {
        case CursorState.Select:
            activeCursor = selectCursor;
            break;
        case CursorState.Attack:
            activeCursor = attackCursors[mouseFrame % attackCursors.Length];
            break;
        case CursorState.Harvest:
            activeCursor = harvestCursors[mouseFrame % harvestCursors.Length];
            break;
        case CursorState.Move:
            activeCursor = moveCursors[mouseFrame % moveCursors.Length];
            break;
        case CursorState.PanLeft:
            activeCursor = leftCursor;
            break;
        case CursorState.PanRight:
            activeCursor = rightCursor;
            break;
        case CursorState.PanUp:
            activeCursor = upCursor;
            break;
        case CursorState.PanDown:
            activeCursor = downCursor;
            break;
        case CursorState.RallyPoint:
            activeCursor = rallyPointCursor;
            break;
        default: break;
        }
    }

    public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits) {
        this.resourceValues = resourceValues;
        this.resourceLimits = resourceLimits;
    }

    public CursorState GetPreviousCursorState() => previousCursorState;
    public CursorState GetCursorState() => activeCursorState;

}

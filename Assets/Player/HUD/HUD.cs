using System.Collections;
using System.Collections.Generic;
using RTS;
using UnityEngine;

public class HUD : MonoBehaviour {
    const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
    const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;

    public GUISkin mouseCursorSkin;
    public GUISkin resourceSkin, ordersSkin, selectBoxSkin;
    public Texture2D activeCursor;
    public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;
    public Texture2D[] resources;

    private readonly Dictionary<ResourceType, Texture2D> resourceImages = new();
    private Dictionary<ResourceType, int> resourceValues = new(), resourceLimits = new();
    private CursorState activeCursorState;
    private int mouseFrame = 0;
    Player player;

    void Start() {
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
        ResourceManager.StoreSelectBoxItems(selectBoxSkin);
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
        }
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
        GUI.BeginGroup(new(Screen.width - ORDERS_BAR_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));

        GUI.Box(new Rect(0, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
        string selectionName = "";
        if (player.SelectedObject) {
            selectionName = player.SelectedObject.objectName;
        }
        if (!selectionName.Equals("")) {
            GUI.Label(new Rect(0, 10, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
        }

        GUI.EndGroup();
    }

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
            GUI.skin = mouseCursorSkin;
            GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
            UpdateCursorAnimation();
            Rect cursorPosition = GetCursorDrawPosition();
            GUI.Label(cursorPosition, activeCursor);
            GUI.EndGroup();
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
        else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
            topPos -= activeCursor.height / 2;
            leftPos -= activeCursor.width / 2;
        }
        return new(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    public void SetCursorState(CursorState newState) {
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
        default: break;
        }
    }

    public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits) {
        this.resourceValues = resourceValues;
        this.resourceLimits = resourceLimits;
    }

}

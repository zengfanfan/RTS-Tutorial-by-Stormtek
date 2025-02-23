using System;
using UnityEditor;
using UnityEngine;

public class QuickMenu {
    [MenuItem("Zeng/QuickMenu")]
    public static void OnQuickMenu() {
        // Debug.Log($"[QuickMenu]! {EditorWindow.focusedWindow.GetType()}");
        var aw = EditorWindow.focusedWindow;
        if (aw == null) return;
        if (aw.GetType().ToString() == "UnityEditor.ProjectBrowser") {
            OnQuickMenu_Asset();
        }
    }

    static void OnQuickMenu_Asset() {
        if (Selection.objects.Length != 1) return;
        var s = Selection.objects[0];
        if (s is DefaultAsset da) {
            OnQuickMenu_Asset_Folder(da.name, AssetDatabase.GetAssetPath(da));
        } else if (s is MonoScript) {
            DoMenu("Assets/Create/C# Script");
        } else if (s is SceneAsset) {
            DoMenu("Assets/Create/Scene");
        } else if (s is Material) {
            DoMenu("Assets/Create/Material");
        } else if (s is PhysicMaterial) {
            DoMenu("Assets/Create/PhysicMaterial");
        } else {
            DoMenu("Assets/Create/Folder");
        }
    }

    static void OnQuickMenu_Asset_Folder(string name, string path) {
        // Debug.Log($"name={name}, path={path}");
        if (name == "Assets") {
            DoMenu("Assets/Create/Folder");
        } else if (name == "StreamingAssets") {
            DoMenu("Assets/Create/Folder");
        } else if (name == "Scripts") {
            DoMenu("Assets/Create/C# Script");
        } else if (name == "Scenes") {
            DoMenu("Assets/Create/Scene");
        } else if (name == "Materials") {
            OnQuickMenu_Asset_Material();
        } else if (path.Contains("Resources/Units")) {
            DoMenu("Assets/Create/ScriptableObjects/Unit");
        } else if (path.Contains("Resources/Items")) {
            DoMenu("Assets/Create/ScriptableObjects/Item");
        } else if (path.Contains("Resources/Abilities")) {
            DoMenu("Assets/Create/ScriptableObjects/Ability");
        } else if (path.Contains("Resources/Objects")) {
            DoMenu("Assets/Create/ScriptableObjects/Object");
        } else if (path.Contains("Script")) {
            DoMenu("Assets/Create/C# Script");
        } else if (path.Contains("Scene")) {
            DoMenu("Assets/Create/Scene");
        } else if (path.Contains("Material")) {
            OnQuickMenu_Asset_Material();
        } else {
            DoMenu("Assets/Create/Folder");
        }
    }

    static void DoMenu(string path) => EditorApplication.ExecuteMenuItem(path);

    static GenericMenu menu = null;
    static void OnQuickMenu_Asset_Material() {
        if (menu == null) {
            menu = new();
            menu.AddItem(new GUIContent("Folder"), false, () => DoMenu("Assets/Create/Folder"));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Material"), false, () => DoMenu("Assets/Create/Material"));
            menu.AddItem(new GUIContent("Physic Material"), false, () => DoMenu("Assets/Create/Physic Material"));
        }
        menu.ShowAsContext();
    }
}

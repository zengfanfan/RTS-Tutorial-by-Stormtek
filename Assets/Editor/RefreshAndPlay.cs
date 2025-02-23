using UnityEditor;

[InitializeOnLoad]
public class RefreshAndPlay {
    static RefreshAndPlay() { // 每次 refresh assets 都会执行
        EditorApplication.update += OnUpdate;
        Menu.SetChecked("Zeng/Enable", Enbaled);
    }

    static bool Active {// 避免刚打开项目进入Play模式, 只有通过自定义菜单才可以
        get => SessionState.GetBool("RefreshAndPlay.Active", false);
        set => SessionState.SetBool("RefreshAndPlay.Active", value);
    }
    static bool Enbaled {
        get => SessionState.GetBool("RefreshAndPlay.Enbaled", true);
        set => SessionState.SetBool("RefreshAndPlay.Enbaled", value);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsCompiled() {
        if (Enbaled && Active) OnMenuPlay();
        Active = false;
    }

    [MenuItem("Zeng/Enable")]
    public static void OnMenuEnable() {
        Enbaled = !Enbaled;
        Menu.SetChecked("Zeng/Enable", Enbaled);
    }

    [MenuItem("Zeng/Play", true)]
    public static bool ValidateOnMenuPlay() => Enbaled;
    [MenuItem("Zeng/Play")]
    public static void OnMenuPlay() {
        if (EditorApplication.isPlaying) {
            EditorApplication.ExitPlaymode();
        } else {
            EditorApplication.EnterPlaymode();
        }
    }

    [MenuItem("Zeng/Refresh And Play", true)]
    public static bool ValidateOnMenuRefreshPlay() => Enbaled;
    [MenuItem("Zeng/Refresh And Play")]
    public static void OnMenuRefreshPlay() {
        if (EditorApplication.isPlaying) {
            EditorApplication.ExitPlaymode();
        } else {
            Active = true;
            lastRefreshTime = EditorApplication.timeSinceStartup;
            AssetDatabase.Refresh();
        }
    }

    static double lastRefreshTime = 0;
    static void OnUpdate() {
        if (lastRefreshTime > 0) {
            if (EditorApplication.timeSinceStartup - lastRefreshTime < 1) {
                // 正在刷新, 刷新完成后在 OnScriptsCompiled 中会进入Play模式
                if (EditorApplication.isCompiling) {
                    lastRefreshTime = 0;// do nothing
                }
            } else {
                // 没发生刷新, 直接执行 OnScriptsCompiled (刷新完成的回调函数)
                lastRefreshTime = 0;
                OnScriptsCompiled();
            }
        }
    }
}

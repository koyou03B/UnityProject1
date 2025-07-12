using UnityEditor;
using UnityEngine;

public class TooltipPopupWindow : EditorWindow
{
    private string tooltipText;
    private Vector2 scrollPos;

    public static void ShowWindow(string text)
    {
        var window = ScriptableObject.CreateInstance<TooltipPopupWindow>();
        window.tooltipText = text;
        window.titleContent = new GUIContent("説明");
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 200);
        window.ShowUtility();  // ポップアップ風に小さく表示
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.LabelField(tooltipText, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("閉じる"))
        {
            Close();
        }
    }
}

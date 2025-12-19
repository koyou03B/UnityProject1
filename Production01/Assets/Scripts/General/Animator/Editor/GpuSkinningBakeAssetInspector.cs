#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GpuSkinningBakeAsset))]
public class GpuSkinningBakeAssetInspector : Editor
{
    private SkinnedMeshRenderer _SMRender;
    private AnimationClip _Clip;
    private int _SampleRate = 60;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Bake (Editor Only)", EditorStyles.boldLabel);

        _SMRender = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Source SMR", _SMRender, typeof(SkinnedMeshRenderer), true);
        _Clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", _Clip, typeof(AnimationClip), false);
        _SampleRate = EditorGUILayout.IntPopup("Sample Rate", _SampleRate, new[] { "30", "60" }, new[] { 30, 60 });

        using (new EditorGUI.DisabledScope(_SMRender == null || _Clip == null))
        {
            if (GUILayout.Button("Bake Into This Asset"))
            {
                var asset = (GpuSkinningBakeAsset)target;
                GpuSkinningBakeUtility.BakeIntoAsset(asset, _SMRender, _Clip, _SampleRate);
                Debug.Log("Bake Completed.");
            }
        }
    }
}
#endif

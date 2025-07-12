using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "MouseCursorData", menuName = "Scriptable Objects/MouseCursorData")]
public class MouseCursorData : ScriptableObject
{
    [System.Serializable]
    public class MouseCursorInfo
    {
        [SerializeField]
        private eCursorMode _CursorMode;
        [SerializeField]
        private Sprite _CursorSprite;
        [SerializeField]
        private Vector2 _Pivot;

        public bool CheckCursorMode(eCursorMode mode) => mode == _CursorMode;
        public Sprite CursorSprite => _CursorSprite;
        public Vector2 Pivot => _Pivot;
    }

    [SerializeField]
    private MouseCursorInfo[] _MouseCursorInfoArray;

    public MouseCursorInfo GetMouseCursorInfo(eCursorMode mode)
    {
        foreach(var info in _MouseCursorInfoArray)
        {
            if(info.CheckCursorMode(mode))
            {
                return info;
            }
        }

        Debug.Log(mode.ToString() + "is not found in " + _MouseCursorInfoArray.ToString());
        return null;
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SpriteListData", menuName = "Scriptable Objects/SpriteListData")]
public class SpriteListData : ScriptableObject
{
    [SerializeField]
    private Sprite[] _SpriteArray;

    public Sprite GetSprite(int index)
    {
        if(index > _SpriteArray.Length)
        {
            Debug.Log("index is Over List in " + _SpriteArray.ToString());
            return null;
        }

        return _SpriteArray[index];
    }
}

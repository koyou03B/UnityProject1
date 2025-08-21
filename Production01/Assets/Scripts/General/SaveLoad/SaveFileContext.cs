using System.Text;
using UnityEngine;

/// <summary>
/// 各プラットフォームごとに設定できる
/// </summary>
public class SaveFileContext
{
    private string _MountName;
    private string _SaveDataName;
    private string _SystemName;
    private string[] _SlotName;


    public string MountName { get { return _MountName; } }
    public string SystemName { get { return _SystemName; } }
    public string SaveFileName(int index)
    {
        StringBuilder sbStr = new StringBuilder();
        sbStr.Append(_SaveDataName);
        if(index < 0 && index >= _SlotName.Length)
        {
            return null;
        }

         sbStr.Append(_SlotName[index]);
        return sbStr.ToString();
    }

    

    public SaveFileContext(string mountName, string saveDataName, string systemName, string[] slotName)
    {
        _MountName = mountName;
        _SaveDataName = saveDataName;
        _SystemName = systemName;
        _SlotName = slotName;
    }
}

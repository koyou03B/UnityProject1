using UnityEngine;

public class SaveFileContext
{
    private string _MountName;
    private string _SaveDataName;
    private string _SystemName;
    private string[] _SlotName;

    public string MountName { get { return _MountName; } }
    public string SaveDataName { get { return _SaveDataName; } }
    public string SystemName { get { return _SystemName; } }
    public string FindSlotName(int index)
    {
        if(index < 0 && index >= _SlotName.Length)
        {
            return null;
        }

        return _SlotName[index];
    }

    public SaveFileContext(string mountName, string saveDataName, string systemName, string[] slotName)
    {
        _MountName = mountName;
        _SaveDataName = saveDataName;
        _SystemName = systemName;
        _SlotName = slotName;
    }
}

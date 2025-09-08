using UnityEditor.SceneManagement;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SaveDataSlotManager : SingletonMonoBehavior<SaveDataSlotManager>
{
    private SaveDataController _SaveDataCtrl;
    private SaveLoadTags.eTopTag _eSaveSlot;
    public bool EndSaveLoadAction => !_SaveDataCtrl.IsSaveLoadAction;

    private void Awake()
    {
        _SaveDataCtrl = gameObject.GetComponent<SaveDataController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
    //    _SaveDataCtrl.Setup();
    //    LoadAllDataFromFile();
    //}
    public void Setup()
    {
        _SaveDataCtrl.Setup();
        LoadAllDataFromFile();
    }
    // Update is called once per frame
    void Update()
    {
        _SaveDataCtrl.Tick();
    }

    public void SaveSystemData()
    {
        _SaveDataCtrl.SetupSave(_eSaveSlot, true);
    }

    public void SaveSlotData()
    {
        _SaveDataCtrl.SetupSave(_eSaveSlot, true);
    }
    public void SaveSlotData(SaveLoadTags.eTopTag slot)
    {
        if(slot != _eSaveSlot)
        {
            _eSaveSlot = slot;
        }
        _SaveDataCtrl.SetupSave(_eSaveSlot, true);
    }

    public void DeleteSaveFiile(SaveLoadTags.eTopTag slot,bool systemFile)
    {
        _SaveDataCtrl.SetupDelete(slot, systemFile);
    }

    /// <summary>
    /// すべてのファイルからデータを取得する
    /// </summary>
    private void LoadAllDataFromFile()
    {
        _SaveDataCtrl.SetupLoad();
    }

}

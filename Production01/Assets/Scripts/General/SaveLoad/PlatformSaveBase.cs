using UnityEngine;

public abstract class PlatformSaveBase : MonoBehaviour
{
    protected readonly string PathSetting = "PathSetting!";
    protected readonly string EmptySaveFile = "EmptySaveFile";

    protected SaveFileContext _SaveFileContext;
    protected SaveLoadBuffer _SaveLoadBuffer;
    protected Observable<SaveLoadEnum.eSaveErrorType> _ErrorObservable;

    protected byte[] _SystemData;
    protected byte[][] _GameData;

    protected bool _IsWritingSaveData;
    protected bool _IsLoadingSaveData;
    
    public void SettingSaveFileContext(string mountName, string saveDataName, string systemName, string[] slotName)
    {
        _SaveFileContext = new SaveFileContext(mountName,saveDataName, systemName, slotName);
        _SystemData = null;
        _GameData = new byte[slotName.Length][];

        _IsWritingSaveData = false;
        _IsLoadingSaveData = false;
    }

    public bool IsWritingSaveData()
    {
        return _IsWritingSaveData;
    }

    public bool IsLoadingSaveData()
    {
        return _IsWritingSaveData;
    }

    
    public virtual void WriteSaveProcess(int slot, bool systemFile = false) { }
    public virtual void ReadSaveProcess(int slot, bool systemFile = false) { }
    public virtual void DeleatSaveProcess(int slot,bool systemFile = false) {}
    public virtual bool IsExistSaveData() { return false; }

    /// <summary>
    /// セーブフォルダのパスを取得
    /// </summary>
    /// <returns></returns>
    protected virtual string GetSaveDataPath() { return Application.persistentDataPath.Replace("/LocalLow/", "/Local/").Replace(Application.productName, _SaveFileContext.MountName) + "/EditorSave"; }
}

using System.Collections.Generic;
using NUnit.Framework;
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
    protected bool _IsDeleatingSaveData;
    
    public void SettingSaveFileContext(string mountName, string saveDataName, string systemName, string[] slotName)
    {
        _SaveFileContext = new SaveFileContext(mountName,saveDataName, systemName, slotName);
        _SystemData = null;
        _GameData = new byte[slotName.Length][];

        _IsWritingSaveData = false;
        _IsLoadingSaveData = false;
    }

    public bool IsWritingSaveData => _IsWritingSaveData;
    public bool IsLoadingSaveData => _IsLoadingSaveData;
    public bool IsDeleatingSaveData => _IsDeleatingSaveData;

    public void SendToSaveBuffer()
    {
        //システムは初め必ず作られるからこういう場合は無視でいい
        if (_SystemData == null || _SystemData.Length == 0)
        {
            return;
        }
        List<byte> loadData = new List<byte>(_SystemData.Length + 1024);
        loadData.AddRange(_SystemData);
        for (int i = 0; i < _GameData.Length; i++)
        {
            if (_GameData[i] != null && _GameData[i].Length != 0)
            {
                loadData.AddRange(_GameData[i]);
            }
        }

        _SaveLoadBuffer.SetLoadData(loadData.ToArray());
    }


    public virtual void Setup() { }
    public virtual void WriteSaveProcess(SaveLoadTags.eTopTag slotType, bool systemFile = false) { }
    public virtual void ReadSaveProcess(SaveLoadTags.eTopTag slotType, bool systemFile = false) { }
    public virtual void DeleatSaveProcess(SaveLoadTags.eTopTag slotType, bool systemFile = false) {}
    public virtual void DetermineRecoveryStrategy(SaveLoadEnum.eSaveLoadAction eSaveLoadAction, ref bool isRetry,ref bool isStop) {}
    /// <summary>
    /// セーブフォルダのパスを取得
    /// </summary>
    /// <returns></returns>
    protected virtual string GetSaveDataPath() { return Application.persistentDataPath.Replace("/LocalLow/", "/Local/").Replace(Application.productName, _SaveFileContext.MountName) + "/EditorSave"; }
}

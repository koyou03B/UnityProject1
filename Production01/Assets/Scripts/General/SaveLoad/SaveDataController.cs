using System;
using UnityEngine;

public class SaveDataController : MonoBehaviour,IObserver<SaveLoadEnum.eSaveErrorType>
{
    private WindowsSaveService _SaveService;
    private SaveLoadMapper _SaveLoadMapper;
    private SaveLoadBuffer _SaveLoadBuffer;

    private SaveLoadTags.eTopTag _eSaveSlot;
    private SaveLoadEnum.eSaveErrorType _eSaveErrorType;
    private SaveLoadEnum.eSaveLoadAction _eSaveLoadAction;

    private bool _IsSaveLoadAction;
    private bool _IsSystemData;
    private bool _EndSlotLoad;

    /// <summary>
    /// セーブ/ロードが終わったかどうか
    /// </summary>
    public bool IsSaveLoadAction => _IsSaveLoadAction;
    
    void Awake()
    {
        _SaveService = gameObject.GetComponent<WindowsSaveService>();
        _SaveLoadBuffer = GetComponent<SaveLoadBuffer>();
        _SaveLoadMapper = GetComponent<SaveLoadMapper>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup()
    {
        _eSaveSlot = SaveLoadTags.eTopTag.Slot1;
        _eSaveLoadAction = SaveLoadEnum.eSaveLoadAction.None;
        _IsSaveLoadAction = false;
        _IsSystemData = false;
        _EndSlotLoad = false;

        _SaveService.Setup();
        _SaveLoadBuffer.Setup();
        _SaveLoadMapper.Setup();
    }

    private void Reset()
    {
        _eSaveLoadAction = SaveLoadEnum.eSaveLoadAction.None;
        _IsSaveLoadAction = false;
        _IsSystemData = false;
        _EndSlotLoad = false;
    }

    public void Tick()
    {
        switch(_eSaveLoadAction)
        {
            case SaveLoadEnum.eSaveLoadAction.Save:
                if(!_SaveService.IsWritingSaveData)
                {
                    //アプデ内容のリセット
                    GlobalRawSaveData.Instance.ResetUpdateSaveType();
                    //セーブデータをゲームに反映
                    ApplySaveToGameState();
                    Reset();
                }
                break;
            case SaveLoadEnum.eSaveLoadAction.Load:
                //残りのスロット分をセーブする
                SlotDataLoad();
                break;
            case SaveLoadEnum.eSaveLoadAction.Delete:
                if(!_SaveService.IsDeleatingSaveData)
                {
                    Reset();
                }
                break;
            case SaveLoadEnum.eSaveLoadAction.Error:

                break;

        }
    }

    public void LoadSlotDataToGameState(SaveLoadTags.eTopTag slotType)
    {
        if (slotType != _eSaveSlot) return;

        _eSaveSlot = slotType;
        ApplySlotDataToGameData();
    }

    /// <summary>
    /// 選択したSlotを反映させる
    /// </summary>
    private void ApplySlotDataToGameData()
    {
        var innerTypes = Enum.GetValues(typeof(SaveLoadTags.eInnerTypeTag));
        bool containsSlot = _SaveLoadBuffer.ContainsTopBlock(_eSaveSlot);
        foreach (SaveLoadTags.eInnerTypeTag inner in innerTypes)
        {
            if (inner == SaveLoadTags.eInnerTypeTag.Input ||
                inner == SaveLoadTags.eInnerTypeTag.Option ||
                inner == SaveLoadTags.eInnerTypeTag.GameSystem)
            {
                continue;
            }

            byte[] data = null;
            if (containsSlot)
            {
                _SaveLoadBuffer.GetInnerBlock(_eSaveSlot, inner, ref data);
                _SaveLoadMapper.DeserializeSaveLoadData(data, inner);
            }
            else
            {
                data = BytePacker.Pack((byte)inner, 0, Array.Empty<byte>());
                _SaveLoadMapper.DeserializeSaveLoadData(data, inner);
            }

        }
    }


    /// <summary>
    /// セーブ処理準備
    /// </summary>
    /// <param name="slotType"></param>
    /// <param name="systemSave"></param>
    public void SetupSave(SaveLoadTags.eTopTag slotType, bool systemSave = true)
    {
        _eSaveSlot = slotType;
        _eSaveLoadAction = SaveLoadEnum.eSaveLoadAction.Save;
        _IsSystemData = systemSave;
        _IsSaveLoadAction = true;

        //セーブするペイロードを整える
        PrepareSavePayload(slotType);
        //書き込み処理を行う
        _SaveService.WriteSaveProcess(slotType, systemSave);
    }

    /// <summary>
    /// セーブするためのペイロードを整える
    /// </summary>
    /// <param name="slotType"></param>
    /// <param name="systemSave"></param>
    private void PrepareSavePayload(SaveLoadTags.eTopTag slotType)
    {
        var saveList = GlobalRawSaveData.Instance.UpdateSaveTypeList;
        //部分的にキャプチャ(初めての時は全部キャプチャになる
        _SaveLoadBuffer.Capture(slotType, saveList);
    }

    /// <summary>
    /// セーブした内容をゲーム側に反映させる
    /// </summary>
    private void ApplySaveToGameState()
    {
        var affectedSaveTypes = _SaveLoadBuffer.AffectedSaveTypes;
        
        foreach(var inner in affectedSaveTypes)
        {
            byte[] data = null;
            if (inner == SaveLoadTags.eInnerTypeTag.Input ||
                inner == SaveLoadTags.eInnerTypeTag.Option ||
                inner == SaveLoadTags.eInnerTypeTag.GameSystem)
            {
                _SaveLoadBuffer.GetInnerBlock(SaveLoadTags.eTopTag.General,inner,ref data);
            }
            else
            {
                _SaveLoadBuffer.GetInnerBlock(_eSaveSlot,inner, ref data);
            }
            _SaveLoadMapper.DeserializeSaveLoadData(data, inner);
        }
    }

    /// <summary>
    /// ロードの準備をする
    /// </summary>
    public void SetupLoad()
    {
        _eSaveLoadAction = SaveLoadEnum.eSaveLoadAction.Load;
        _IsSaveLoadAction = true;

        _eSaveSlot = SaveLoadTags.eTopTag.General;
    }

    /// <summary>
    /// スロットのロードを始める
    /// </summary>
    private void SlotDataLoad()
    {
        if (_SaveService.IsLoadingSaveData) return;

        //最後まで終わっているのなら
        if (_EndSlotLoad)
        {
            _EndSlotLoad = false;
            //ロードデータを送る
            _SaveService.SendToSaveBuffer();
            Reset();
        }

        switch (_eSaveSlot)
        {
            case SaveLoadTags.eTopTag.General:
                //Systemの読み込みを始める
                _SaveService.ReadSaveProcess(_eSaveSlot, true);
                _eSaveSlot = SaveLoadTags.eTopTag.Slot1;

                break;
            case SaveLoadTags.eTopTag.Slot1:
                _SaveService.ReadSaveProcess(_eSaveSlot, false);
                _eSaveSlot = SaveLoadTags.eTopTag.Slot2;
                break;
            case SaveLoadTags.eTopTag.Slot2:
                _SaveService.ReadSaveProcess(_eSaveSlot, false);
                _eSaveSlot = SaveLoadTags.eTopTag.Slot3;
                break;
            case SaveLoadTags.eTopTag.Slot3:
                _SaveService.ReadSaveProcess(_eSaveSlot, false);
                _EndSlotLoad = true;
                break;
        }
    }


    /// <summary>
    /// 削除の準備
    /// </summary>
    /// <param name="slotType"></param>
    /// <param name="systemSave"></param>
    public void SetupDelete(SaveLoadTags.eTopTag slotType, bool systemSave = true)
    {
        _eSaveSlot = slotType;
        _eSaveLoadAction = SaveLoadEnum.eSaveLoadAction.Delete;
        _IsSystemData = systemSave;
        _IsSaveLoadAction = true;

        _SaveService.DeleatSaveProcess(slotType, systemSave);
    }

    /// <summary>
    /// エラー通知
    /// </summary>
    /// <param name="state"></param>
    public void OnNotify(SaveLoadEnum.eSaveErrorType state)
    {
       _eSaveErrorType = state;
        //設定し忘れはログ見て気づいて
        if(_eSaveErrorType == SaveLoadEnum.eSaveErrorType.ShortageFs)
        {
            if(_eSaveLoadAction == SaveLoadEnum.eSaveLoadAction.Save)
            {
                //アプデ内容のリセット
                GlobalRawSaveData.Instance.ResetUpdateSaveType();
                //セーブデータをゲームに反映
                ApplySaveToGameState();
            }

            Reset();
            return;
        }

        bool isRetry =false;
        bool isStop = false;
        //次の行動を決める
        _SaveService.DetermineRecoveryStrategy(_eSaveLoadAction, ref isRetry, ref isStop);
        switch (_eSaveLoadAction)
        {
            case SaveLoadEnum.eSaveLoadAction.Save:
                if (isRetry)
                {
                    //再度書き込み処理を行う
                    _SaveService.WriteSaveProcess(_eSaveSlot, _IsSystemData);
                }
                else if (isStop)
                {
                    //アプデ内容のリセット
                    GlobalRawSaveData.Instance.ResetUpdateSaveType();
                    //セーブデータをゲームに反映
                    ApplySaveToGameState();
                    Reset();
                }

                break;
            case SaveLoadEnum.eSaveLoadAction.Load:
                if (isRetry)
                {
                    //再度読み込み処理を行う
                    RetryLoadAction();
                }
                else if (isStop)
                {
                    _EndSlotLoad = false;
                    //ロードデータを送る
                    _SaveService.SendToSaveBuffer();
                    Reset();
                }
                break;
            case SaveLoadEnum.eSaveLoadAction.Delete:
                if (isRetry)
                {
                    //再度削除処理を行う
                    SetupDelete(_eSaveSlot, _IsSystemData);
                }
                else if (isStop)
                {
                    Reset();
                }
                break;

        }

    }

    /// <summary>
    /// 事前のロードをもう一度行う
    /// </summary>
    private void RetryLoadAction()
    {
        switch (_eSaveSlot)
        {
            case SaveLoadTags.eTopTag.Slot1://System中
                _SaveService.ReadSaveProcess(SaveLoadTags.eTopTag.General, true);
                break;
            case SaveLoadTags.eTopTag.Slot2://Slot1中
                _SaveService.ReadSaveProcess(SaveLoadTags.eTopTag.Slot1, false);
                break;
            case SaveLoadTags.eTopTag.Slot3://Slot2中もしくはSlot3中
                if (!_EndSlotLoad)
                {
                    _SaveService.ReadSaveProcess(SaveLoadTags.eTopTag.Slot2, false);
                }
                else
                {
                    _SaveService.ReadSaveProcess(SaveLoadTags.eTopTag.Slot3, false);
                }
                break;
        }
    }

    /// <summary>
    /// オブザーバーエラー時
    /// </summary>
    /// <param name="error"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    public void OnError(System.Exception error)
    {
        //あんまり入ってこないとは思う
    }
}

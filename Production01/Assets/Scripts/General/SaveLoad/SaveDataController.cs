using UnityEngine;

public class SaveDataController : MonoBehaviour,IObserver<SaveLoadEnum.eSaveErrorType>
{
    private WindowsSaveService _SaveService;
    private SaveLoadMapper _SaveLoadMapper;
    private SaveLoadBuffer _SaveLoadBuffer;

    private SaveLoadEnum.eSaveErrorType _eSaveErrorType;
    private SaveLoadEnum.eSaveType _eSaveSlot;
    private SaveLoadEnum.eSaveLoadAction _eSaveLoadAction;

    private bool _IsSaveLoadAction;
    private bool _IsSystemData;
    private bool _EndSlotLoad;

    /// <summary>
    /// セーブ/ロードが終わったかどうか
    /// </summary>
    public bool isSaveLoadAction => _IsSaveLoadAction;
    
    void Awake()
    {
        _SaveService = GetComponent<WindowsSaveService>();
        _SaveLoadBuffer = GetComponent<SaveLoadBuffer>();
        _SaveLoadMapper = GetComponent<SaveLoadMapper>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _eSaveSlot = SaveLoadEnum.eSaveType.Slot1;
        _eSaveLoadAction = SaveLoadEnum.eSaveLoadAction.None;
        _IsSaveLoadAction = false;
        _IsSystemData = false;
        _EndSlotLoad = false;
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
            case SaveLoadEnum.eSaveLoadAction.Deleate:
                if(!_SaveService.IsDeleatingSaveData)
                {
                    Reset();
                }
                break;
            case SaveLoadEnum.eSaveLoadAction.Error:

                break;

        }
    }

    /// <summary>
    /// セーブ処理準備
    /// </summary>
    /// <param name="slotType"></param>
    /// <param name="systemSave"></param>
    public void SetupSave(SaveLoadEnum.eSaveType slotType, bool systemSave = true)
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
    private void PrepareSavePayload(SaveLoadEnum.eSaveType slotType)
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
        
        foreach(var saveType in affectedSaveTypes)
        {
            byte[] data = null;
            if (saveType == SaveLoadEnum.eSaveType.System || saveType == SaveLoadEnum.eSaveType.Input)
            {
                _SaveLoadBuffer.GetSaveLoadDataBlock(saveType,ref data);
            }
            else
            {
                _SaveLoadBuffer.GetSaveLoadDataInnerBlock(_eSaveSlot,saveType, ref data);
            }
            _SaveLoadMapper.DeserializeSaveLoadData(data, saveType);
        }
    }

    /// <summary>
    /// ロードの準備をする
    /// </summary>
    public void SetupLoad()
    {
        _eSaveLoadAction = SaveLoadEnum.eSaveLoadAction.Load;
        _IsSaveLoadAction = true;

        _eSaveSlot = SaveLoadEnum.eSaveType.System;
    }

    /// <summary>
    /// スロットのロードを始める
    /// </summary>
    private void SlotDataLoad()
    {
        if (!_SaveService.IsLoadingSaveData)
        {
            //最後まで終わっているのなら
            if(_EndSlotLoad)
            {
                _EndSlotLoad = false;
                //ロードデータを送る
                _SaveService.SendToSaveBuffer();
                Reset();
            }
        }

        switch(_eSaveSlot)
        {
            case SaveLoadEnum.eSaveType.System:
                //Systemの読み込みを始める
                _SaveService.ReadSaveProcess(_eSaveSlot, true);
                _eSaveSlot = SaveLoadEnum.eSaveType.Slot1;

                break;
            case SaveLoadEnum.eSaveType.Slot1:
                _SaveService.ReadSaveProcess(_eSaveSlot, false);
                _eSaveSlot = SaveLoadEnum.eSaveType.Slot2;
                break;
            case SaveLoadEnum.eSaveType.Slot2:
                _SaveService.ReadSaveProcess(_eSaveSlot, false);
                _eSaveSlot = SaveLoadEnum.eSaveType.Slot3;
                break;
            case SaveLoadEnum.eSaveType.Slot3:
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
    public void SetupDeleate(SaveLoadEnum.eSaveType slotType, bool systemSave = true)
    {
        _eSaveSlot = slotType;
        _eSaveLoadAction = SaveLoadEnum.eSaveLoadAction.Deleate;
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
            case SaveLoadEnum.eSaveLoadAction.Deleate:
                if (isRetry)
                {
                    //再度削除処理を行う
                    SetupDeleate(_eSaveSlot, _IsSystemData);
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
            case SaveLoadEnum.eSaveType.Slot1://System中
                _SaveService.ReadSaveProcess(SaveLoadEnum.eSaveType.System, true);
                break;
            case SaveLoadEnum.eSaveType.Slot2://Slot1中
                _SaveService.ReadSaveProcess(SaveLoadEnum.eSaveType.Slot1, false);
                break;
            case SaveLoadEnum.eSaveType.Slot3://Slot2中もしくはSlot3中
                if (!_EndSlotLoad)
                {
                    _SaveService.ReadSaveProcess(SaveLoadEnum.eSaveType.Slot2, false);
                }
                else
                {
                    _SaveService.ReadSaveProcess(SaveLoadEnum.eSaveType.Slot3, false);
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

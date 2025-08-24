using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public class WindowsSaveService :PlatformSaveBase
{
    private PrefixLogger _Logger;

    private void Awake()
    {
        _SaveLoadBuffer = GetComponent<SaveLoadBuffer>();
        var saveDataCtrl = GetComponent<SaveDataController>();

        _ErrorObservable = new Observable<SaveLoadEnum.eSaveErrorType>();
        _ErrorObservable.RegistObserver(saveDataCtrl, saveDataCtrl.GetInstanceID());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _Logger = new PrefixLogger(new UnityLogger(), "[WindowsSaveService]");


        //名前決め次第
        //SettingSaveFileContext();
    }

    /// <summary>
    /// 読み取り
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="systemFile"></param>
    public override void ReadSaveProcess(SaveLoadEnum.eSaveType slotType, bool systemFile = false)
    {
        //取り出すセーブファイル名の作成
        string fileName = FindDirectoryProcess(slotType, systemFile);
        if (string.IsNullOrEmpty(fileName))
        {
            _Logger.Log("fileName not Found");
            return;
        }
        
        _IsLoadingSaveData = true;
        StartCoroutine(ReadSave());
       
        IEnumerator ReadSave()
        {
            byte[] data = null;
            SaveLoadEnum.eSaveType saveType;
            if (systemFile)
            {
                saveType = SaveLoadEnum.eSaveType.System;
            }
            else
            {
                saveType = slotType;
            }

            //ロード処理
            using (FileStream fs = new System.IO.FileStream(fileName.ToString(), FileMode.Open))
            {
                long len = fs.Length;
                data = new byte[(int)len];
                fs.Read(data, 0, data.Length);
            }
            yield return null;

            //暗号化解析
            data = GlobalCrypt.AES.Decrypt(data);
            if (data != null && data.Length != 0)
            {
                //暗号化の影響で余分な配列データを含むのでトリミング
                bool success = _SaveLoadBuffer.TrimToSingleBlock(ref data, saveType);
                if (!success)
                {
                    //Errorの処理
                    _ErrorObservable.SendNotify(SaveLoadEnum.eSaveErrorType.Coprupt);
                    _IsLoadingSaveData = false;
                    yield break;
                }
            }
            else
            {
                //Errorの処理
                _ErrorObservable.SendNotify(SaveLoadEnum.eSaveErrorType.UunkownError);
                _IsLoadingSaveData = false;
                yield break;
            }

            //対応する箱に入れる
            if (systemFile)
            {
                _SystemData = data;
            }
            else
            {
                int slot = slotType switch
                {
                    SaveLoadEnum.eSaveType.Slot1 => 0,
                    SaveLoadEnum.eSaveType.Slot2 => 1,
                    SaveLoadEnum.eSaveType.Slot3 => 2,
                    _ => -1
                };
                //ここにきてる=-1は絶対にない
                _GameData[slot] = data;
            }

            _IsLoadingSaveData = false;
        }
    }

    /// <summary>
    /// 書き込み
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="systemSave"></param>
    public override void WriteSaveProcess(SaveLoadEnum.eSaveType slotType, bool systemSave = false)
    {
        byte[] data = null;
        if (systemSave)
        {
            //システムデータのみ取ってくる
            _SaveLoadBuffer.GetSaveLoadDataBlock(SaveLoadEnum.eSaveType.System, ref data);
        }
        else
        {
            //slot1～slot3を取得
            SaveLoadEnum.eSaveType saveType = slotType;
            _SaveLoadBuffer.GetSaveLoadDataBlock(saveType, ref data);
        }
        //セーブファイル名の作成
        string fileName = FindDirectoryProcess(slotType, systemSave);
        if (string.IsNullOrEmpty(fileName))
        {
            _Logger.Log("fileName not Found");
            _ErrorObservable.SendNotify(SaveLoadEnum.eSaveErrorType.ShortageFs);
            return;
        }

        _IsWritingSaveData = true;
        StartCoroutine(Save());

        IEnumerator Save()
        {
            //AES128暗号化処理
            byte[] _data = GlobalCrypt.AES.Encrypt(data);

            yield return _data;

            //セーブ処理
            using (FileStream fs = new System.IO.FileStream(fileName, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(_data);
                }
            }

            _IsWritingSaveData = false;
        }
    }

    /// <summary>
    /// セーブファイルの削除
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="systemFile"></param>
    public override void DeleatSaveProcess(SaveLoadEnum.eSaveType slotType, bool systemFile = false)
    {
        //セーブファイルのパス作成
        string fileName = FindDirectoryProcess(slotType, systemFile);
        if(string.IsNullOrEmpty(fileName))
        {
            _Logger.Log("saveFile not Found");
            return;
        }


        if (File.Exists(fileName))
        {
            File.Delete(fileName);
            _Logger.Log($"Deleate Success! {fileName}");
        }
    }

    /// <summary>
    /// エラー時にどう動くか
    /// Windows版では問答無用でストップ
    /// </summary>
    /// <param name="eSaveLoadAction"></param>
    /// <param name="isRetry"></param>
    /// <param name="isStop"></param>
    public override void DetermineRecoveryStrategy(SaveLoadEnum.eSaveLoadAction eSaveLoadAction, ref bool isRetry, ref bool isStop)
    {
        isStop = true;
        isRetry = false;
    }

    /// <summary>
    /// ディレクトリがあるか。ないなら作成
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="systemFile"></param>
    /// <returns></returns>
    private string FindDirectoryProcess(SaveLoadEnum.eSaveType slotType, bool systemFile)
    {
        int slot = slotType switch
        {
            SaveLoadEnum.eSaveType.Slot1 => 0,
            SaveLoadEnum.eSaveType.Slot2 => 1,
            SaveLoadEnum.eSaveType.Slot3 => 2,
            _ => -1
        };

        if(slot == -1 && systemFile == false)
        {
            _Logger.LogError($"slotType {slotType} : Slot1,2,3 Only");
            return null;
        }

        string saveFile = CreateSaveFileName(slot, systemFile);
        if (saveFile.Contains(PathSetting) || saveFile.Contains(EmptySaveFile))
        {
            //パスもしくはファイル設定のミス
            _ErrorObservable.SendNotify(SaveLoadEnum.eSaveErrorType.ShortageFs);
            return null;
        }
        CheckDirectory(saveFile);

        return saveFile;
    }

    /// <summary>
    /// ディレクトリの確認
    /// </summary>
    /// <param name="path"></param>
    private void CheckDirectory(string path)
    {
        //パスに拡張子が付いているかを確認
        if (path.LastIndexOf('.') != -1)
        {
            string gameSavePath = path.Substring(0, path.LastIndexOf('/'));
            //pathが既存のディレクトリを参照しているかどうか
            if (Directory.Exists(gameSavePath) == false)
            {
                //ディレクトリの作成(フォルダの作成)
                Directory.CreateDirectory(gameSavePath);
            }

            return;
        }


        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
        }
    }


    /// <summary>
    /// セーブファイル名の作成
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="systemFile"></param>
    /// <returns></returns>
    private string CreateSaveFileName(int slot, bool systemFile)
    {
        //セーブデータパスの有無をチェック
        StringBuilder sbStr = new StringBuilder();
        sbStr.Append(base.GetSaveDataPath());

        string folderPath = sbStr.ToString();
        //空で送られた来た場合
        if (string.IsNullOrEmpty(folderPath))
        {
            _Logger.Log($"PathSetting:GetSaveDataPath Function");
            return PathSetting;
        }

        sbStr.Append("/");
        if (systemFile)
        {
            sbStr.Append(_SaveFileContext.SystemName);
            sbStr.Append(".bin");
        }
        else
        {
            string saveFileName = _SaveFileContext.SaveFileName(slot);
            if (string.IsNullOrEmpty(saveFileName))
            {
                _Logger.Log($"{saveFileName} empty! setting SettingSaveFileContext Function");
                return EmptySaveFile;//何かしらで失敗成功かの通知は送るDelegateまたはオブザーバー
            }
            sbStr.Append(saveFileName);
        }
        return sbStr.ToString();
    }
}

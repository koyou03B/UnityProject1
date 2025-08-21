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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _Logger = new PrefixLogger(new UnityLogger(), "[WindowsSaveService]");
        _ErrorObservable = new Observable<SaveLoadEnum.eSaveErrorType>();
        //送る先ができたら追加しよう

        //名前決め次第
        //SettingSaveFileContext();
    }

    public override void ReadSaveProcess(int slot, bool systemFile = false)
    {
        int targetSlot = slot;
        bool readSystemFile = systemFile;
        string fileName = FindDirectoryProcess(slot, systemFile);
        _IsLoadingSaveData = true;

        StartCoroutine(ReadSave());
        IEnumerator ReadSave()
        {
            byte[] data = null;
            SaveLoadEnum.eSaveType saveType;
            if (readSystemFile)
            {
                saveType = SaveLoadEnum.eSaveType.System;
            }
            else
            {
                saveType = SaveLoadEnum.eSaveType.Slot1 + targetSlot;
            }

            //ロード処理
            using (FileStream fs = new System.IO.FileStream(fileName.ToString(), FileMode.Open))
            {
                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
            }
            yield return null;

            data = GlobalCrypt.AES.Decrypt(data);
            if(data != null || data.Length != 0)
            {
                bool success = _SaveLoadBuffer.TrimToSingleBlock(ref data, saveType);
                if(!success)
                {
                    //Errorの処理
                    _ErrorObservable.SendNotify(SaveLoadEnum.eSaveErrorType.Coprupt);
                    _IsLoadingSaveData = false;
                }
            }

            _IsLoadingSaveData = false;
            if (readSystemFile)
            {
                _SystemData = data;
            }
            else
            {
                _GameData[targetSlot] = data;
            }
        }
    }

    public override void WriteSaveProcess(int slot, bool systemFile = false)
    {
        FindDirectoryProcess(slot, systemFile);

        byte[] data = null;
        if (systemFile)
        {
            _SaveLoadBuffer.FindSaveLoadDataArray(SaveLoadEnum.eSaveType.System, ref data);
        }
        else
        {
            SaveLoadEnum.eSaveType saveType = SaveLoadEnum.eSaveType.System + slot;
            _SaveLoadBuffer.FindSaveLoadDataArray(saveType, ref data);
        }

        int targetSlot = slot;
        bool readSystemFile = systemFile;
        string fileName = FindDirectoryProcess(slot, systemFile);
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

    public override void DeleatSaveProcess(int slot, bool systemFile = false)
    {
        string saveFile = FindDirectoryProcess(slot,systemFile);

        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            _Logger.Log($"Deleate Success! {saveFile}");
        }
    }

    public override bool IsExistSaveData()
    {
        return File.Exists(base.GetSaveDataPath());
    }


    private string FindDirectoryProcess(int slot ,bool systemFile)
    {
        string saveFile = CreateSaveFileName(slot, systemFile);
        if (saveFile.Contains(PathSetting) || saveFile.Contains(EmptySaveFile))
        {
            _ErrorObservable.SendNotify(SaveLoadEnum.eSaveErrorType.NoSpaceFs);
        }
        CheckDirectory(saveFile);

        return saveFile;
    }

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
            CreateDirectory(path);
        }
    }

    private void CreateDirectory(string path) => Directory.CreateDirectory(path);

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

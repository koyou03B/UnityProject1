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
        //SettingSaveFileContext();
    }

    public override void ReadSaveProcess(int slot, bool systemFile = false)
    {
        CheckDirectoryProcess(slot, systemFile);
       IEnumerator ReadSave()
        {

            yield return null;
        }
    }

    public override void WriteSaveProcess(int slot, bool systemFile = false)
    {
        CheckDirectoryProcess(slot, systemFile);

        byte[] data = null;
        if (systemFile)
        {
            _SaveLoadBuffer.FindSaveLoadDataArray(SaveLoadEnum.eSaveType.System, ref data);
        }
        else
        {
            
        }
    }

    public override void DeleatSaveProcess(int slot, bool systemFile = false)
    {
        string saveFile = CreateSaveFileName(slot,systemFile);
        if(saveFile.Contains(PathSetting))
        {
            //何かしらで失敗成功かの通知は送るDelegateまたはオブザーバー

        }
        else if(saveFile.Contains(EmptySaveFile))
        {
            //何かしらで失敗成功かの通知は送るDelegateまたはオブザーバー

        }


        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            _Logger.Log($"Deleate Success! {saveFile}");
        }

        //何かしらで失敗成功かの通知は送るDelegateまたはオブザーバー
    }

    public override bool IsExistSaveData()
    {
        return File.Exists(base.GetSaveDataPath());
    }


    private void CheckDirectoryProcess(int slot ,bool systemFile)
    {
        string saveFile = CreateSaveFileName(slot, systemFile);
        if (saveFile.Contains(PathSetting))
        {
            //何かしらで失敗成功かの通知は送るDelegateまたはオブザーバー

        }
        else if (saveFile.Contains(EmptySaveFile))
        {
            //何かしらで失敗成功かの通知は送るDelegateまたはオブザーバー

        }
        CheckDirectory(saveFile);
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

using UnityEngine;

public interface ISaveFileHandler
{
    /// <summary>
    /// 非同期で行うSaveDataの書き込み
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="data"></param>
    void WriteSaveDataAsync(string fileName, byte[] data);

    /// <summary>
    /// 非同期で行うSaveDataの読み込み
    /// </summary>
    /// <param name="fileName"></param>
    void ReadSaveDataAsync(string fileName);

    /// <summary>
    /// SaveDataの削除
    /// </summary>
    /// <param name="fileName"></param>
    void DeleteSaveData(string fileName);

    /// <summary>
    /// SaveDataが存在するかどうか
    /// </summary>
    /// <param name="fileName"></param>
    bool IsExistSaveData(string fileName);

    /// <summary>
    /// 書き込み中
    /// </summary>
    /// <returns></returns>
    bool IsWritingSaveData();
    /// <summary>
    /// 読み込み中
    /// </summary>
    /// <returns></returns>
    bool IsLoadingSaveData();
}

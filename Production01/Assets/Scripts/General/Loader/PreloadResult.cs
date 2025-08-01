using UnityEngine;
using System.Collections.Generic;

public class PreloadResult
{
    //IReadOnlyList 読み取り専用のList
    //注意するのはforeachを使うとGCを発生する
    //https://logicalbeat.jp/blog/18903/
    public IReadOnlyList<string> SuccessKeys { get; }
    public IReadOnlyList<string> FailedKeys { get; }

    public bool HasError => FailedKeys.Count > 0;

    public PreloadResult(List<string> successKeys, List<string> failedKeys)
    {
        SuccessKeys = successKeys ?? new List<string>();
        FailedKeys = failedKeys ?? new List<string>();
    }
}

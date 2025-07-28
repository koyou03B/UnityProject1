using System;
using System.Collections.Generic;
using System.Threading;

public class CancelTokenCollector
{
    private readonly List<CancellationTokenSource> _CancelTokenList = new List<CancellationTokenSource>();

    /// <summary>
    /// トークンの作成
    /// </summary>
    /// <returns></returns>
    public CancellationTokenSource CreateToken()
    {
        Cleanup();
        var cts = new CancellationTokenSource();
        _CancelTokenList.Add(cts);

        return cts;
    }

    /// <summary>
    /// すべてのトークンを削除
    /// </summary>
    public void CancelAll()
    {
        foreach(var cts in _CancelTokenList)
        {
            if(cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
        _CancelTokenList.Clear();
    }

    /// <summary>
    /// 必要のなくなったトークンのみ削除
    /// </summary>
    private void Cleanup()
    {
        for (int i = _CancelTokenList.Count - 1; i >= 0; i--)
        {
            var cts = _CancelTokenList[i];
            if (cts == null || cts.IsCancellationRequested)
            {
                cts?.Dispose();
                _CancelTokenList.RemoveAt(i);
            }
        }
    }
}

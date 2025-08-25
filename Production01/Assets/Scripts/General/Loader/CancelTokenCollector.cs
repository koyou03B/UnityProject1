using System;
using System.Collections.Generic;
using System.Threading;

public sealed class CancelTokenCollector : IDisposable
{
    private readonly List<CancellationTokenSource> _Sources = new();
    private readonly object _Lock = new();

    public CancellationToken CreateToken()
    {
        var cts = new CancellationTokenSource();
        lock (_Lock)
        {
            _Sources.Add(cts);
        }
        return cts.Token;
    }

    public void CancelAll()
    {
        lock (_Lock)
        {
            foreach (var cts in _Sources)
            {
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                catch { }
            }
            _Sources.Clear();
        }
    }

    public void Dispose()
    {
        CancelAll(); // 破棄時には Cancel + Dispose 両方
    }
}

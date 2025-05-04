using System;
using UnityEngine;

public interface IObserver<TState>
{
    //通知を受け取る
    void OnNotify(TState state);

    void OnError(Exception error);
}

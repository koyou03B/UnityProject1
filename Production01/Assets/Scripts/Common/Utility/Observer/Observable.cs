using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Observerに通知を送る役割
/// </summary>
/// <typeparam name="TState">送る通知の状態</typeparam>
public class Observable <TState>
{
    //登録されるオブザーバーのリスト
    private Dictionary<int, IObserver<TState>> _dResistObserver;
  
    public Observable()
    {
        _dResistObserver = new Dictionary<int, IObserver<TState>>();
    }

    /// <summary>
    /// オブザーバーの登録
    /// </summary>
    /// <param name="observer"></param>
    public void RegistObserver(IObserver<TState> observer,int hashCode)
    {
        if (_dResistObserver.ContainsKey(hashCode)) return;
        _dResistObserver.Add(hashCode, observer);
    }

    /// <summary>
    /// オブザーバーの解除
    /// </summary>
    /// <param name="observer"></param>
    public void UnregistObserver(int hashCode)
    {
        if (_dResistObserver.Count == 0) return;
        _dResistObserver.Remove(hashCode);
    }

    /// <summary>
    /// 登録されたすべてのオブザーバーに通知を送る
    /// </summary>
    /// <param name="state"></param>
   public void SendNotify(TState state)
    {
        foreach(var observer in _dResistObserver.Values)
        {
            try
            {
                observer.OnNotify(state);
            }
            catch(Exception error)
            {
                observer.OnError(error);
            }
        }
    }

    /// <summary>
    /// 特定のオブザーバーに送る通知
    /// </summary>
    /// <param name="state"></param>
    /// <param name="hashCode"></param>
    public void SecretSendNotify(TState state, int hashCode)
    {
        if(_dResistObserver.ContainsKey(hashCode))
        {
            _dResistObserver[hashCode].OnNotify(state);
        }
    }
}

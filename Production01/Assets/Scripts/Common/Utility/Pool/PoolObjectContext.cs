using System;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class PoolObjectContext<TObject> where TObject : class
{
    [SerializeField]
    //二重Release検出時に例外を出すかどうか
    public bool _CollectionCheck = true;
    [SerializeField]
    //初期容量
    public int _DefaultCapacity = 1;
    [SerializeField]
    //最大サイズ
    public int _MaxSize = 10;

    public IObjectPool<TObject> _ObjectPool;

    public void CreatePool(Func<TObject> createFunc, Action<TObject> onGetFunc,
        Action<TObject> onReleaseFunc,Action<TObject> OnDestroyFunc)
    {
        _ObjectPool = new ObjectPool<TObject>(createFunc,
            onGetFunc, onReleaseFunc, OnDestroyFunc,
            _CollectionCheck, _DefaultCapacity, _MaxSize);
    }
}

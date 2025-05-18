using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public abstract class Factory<TObject> where TObject : UnityEngine.Object 
{
    [SerializeField]
    //Addressables用のパス
    protected string _ProductsPath = "";
    [SerializeField]
    protected PoolObjectContext<TObject> _ProductPool;

    //GameObject.Instantiateする際に使うObject
    protected TObject _ProductObject;
    //継承先でnewする
    protected ILogger _Logger;

    public Factory(ILogger logger)
    {
        _Logger = logger;
    }

    public virtual async Task Initialize() { CreateObjectPool(); }
    private void CreateObjectPool()
    {
        if (_ProductPool != null)
        {
            _ProductPool.CreatePool(CreateProduct, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject);
        }
        else
        {
            _Logger.LogError("_ProductPool is null");
        }
    }
    /// <summary>
    /// オブジェクトプールに入力する項目を作成するときに呼び出される
    /// </summary>
    /// <returns></returns>
    protected abstract TObject CreateProduct();

    /// <summary>
    /// オブジェクトプールから次の項目を取得するときに呼び出される
    /// </summary>
    /// <param name="pooledObject"></param>
    protected abstract void OnGetFromPool(TObject pooledObject);

    /// <summary>
    /// オブジェクトプールから次の項目を取得するときに呼び出される
    /// </summary>
    /// <param name="pooledObject"></param>
    protected abstract void OnReleaseToPool(TObject pooledObject);

    /// <summary>
    /// プールされた項目の最大数を超える(プールされたオブジェクトを破壊する)と呼び出される
    /// </summary>
    /// <param name="pooledObject"></param>
    protected abstract void OnDestroyPooledObject(TObject pooledObject);
}

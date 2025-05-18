using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class SampleBoxFactory : Factory<GameObject>
{
    public IObjectPool<GameObject> ObjectPool => _ProductPool._ObjectPool;

    public SampleBoxFactory(ILogger logger) : base(logger) { }

    public override async Task Initialize()
    {
        var assetLoader = AssetLoaderService.Instance.AssetLoder;
        var loadAsset = await assetLoader.LoadAssetAsync<GameObject>(_ProductsPath);
        if (loadAsset.IsSuccess)
        {
            _ProductObject = loadAsset.Asset;
        }

        base.Initialize();
    }

    protected override GameObject CreateProduct()
    {
        GameObject obj = GameObject.Instantiate(_ProductObject);

        return obj;
    }

    protected override void OnGetFromPool(GameObject pooledObject)
    {
        pooledObject.gameObject.SetActive(true);
    }

    protected override void OnReleaseToPool(GameObject pooledObject)
    {
        pooledObject.gameObject.SetActive(false);
    }

    protected override void OnDestroyPooledObject(GameObject pooledObject)
    {
        GameObject.Destroy(pooledObject.gameObject);
    }
}

using UnityEngine;

/// <summary>
/// Loder系をまとめているクラス
/// </summary>
public class AssetLoaderService : SingletonMonoBehavior<AssetLoaderService>
{
    //アセットのロードを行う
    private AddressablesAssetLoader _AssetLoder;
    //シーンのロードを行う
    private AddressablesSceneLoader _SceneLoder;
    //アセットのプリロードを行う
    private AddressablesAssetPreloader _AssetPreloder;

    public AddressablesAssetLoader AssetLoder => _AssetLoder;
    public AddressablesSceneLoader SceneLoder => _SceneLoder;
    public AddressablesAssetPreloader AssetPreloder => _AssetPreloder;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitOnBoot()
    {
        var servise = AssetLoaderService.Instance;
        servise._AssetLoder = new AddressablesAssetLoader(new PrefixLogger(new UnityLogger(), "[AssetLoader]"));
        servise._AssetPreloder = new AddressablesAssetPreloader(new PrefixLogger(new UnityLogger(), "[AssetPreLoader]"));
        servise._SceneLoder = new AddressablesSceneLoader(new PrefixLogger(new UnityLogger(), "[SceneLoader]"));
    }
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    protected  override void OnDestroy()
    {
         _AssetLoder.ReleaseAllImmediate();
        _AssetPreloder.ReleaseAllImmediate();

        base.OnDestroy();
    }
}

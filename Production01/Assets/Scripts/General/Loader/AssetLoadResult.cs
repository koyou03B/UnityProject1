using UnityEngine;
public enum AssetLoadErrorType
{
    None,               //成功時限定。エラーがない場合。
    NotFound,       // アセットが見つからなかった
    InUse,              //現在そのアセットは解放中で触れない
    Canceled,        //CancellationToken によってキャンセルされた
    Failed,             //Addressables自体のロード失敗
    Exception,      //上記以外で try-catch に引っかかった例外
}
/// <summary>
/// 呼び出し側にエラー処理をさせてみようかなって思って
/// nullで返すよりはかはこの形で返してあげるのがやりやすいかも
/// って思って作ってみた
/// </summary>
/// <typeparam name="T"></typeparam>
public class AssetLoadResult<T> where T : UnityEngine.Object
{
    public T Asset { get; }
    public AssetLoadErrorType ErrorType { get; }
    public string ErrorMessage { get; }
    public bool IsSuccess => ErrorType == AssetLoadErrorType.None;

    public AssetLoadResult(T asset)
    {
        Asset = asset;
        ErrorType = AssetLoadErrorType.None;
    }

    public AssetLoadResult(AssetLoadErrorType type, string message)
    {
        ErrorType = type;
        ErrorMessage = message;
    }
}

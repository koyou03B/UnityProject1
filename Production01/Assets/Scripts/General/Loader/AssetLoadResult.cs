using UnityEngine;

/// <summary>
/// 呼び出し側にエラー処理をさせてみようかなって思って
/// nullで返すよりはかはこの形で返してあげるのがやりやすいかも
/// って思って作ってみた
/// </summary>
/// <typeparam name="T"></typeparam>
public class AssetLoadResult<T> where T : UnityEngine.Object
{
    public T Asset { get; }
    public string ErrorMessage { get; }
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

    public AssetLoadResult(T asset) => Asset = asset;
    public AssetLoadResult(string error) => ErrorMessage = error;
}

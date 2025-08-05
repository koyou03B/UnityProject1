using UnityEngine;

/// <summary>
/// 読み取り専用のクラス
/// 他クラスはここを介して見たい変数にアクセスすること
/// 継承不可
/// </summary>
public sealed partial class GlobalReadOnlySaveData : SingletonMonoBehavior<GlobalReadOnlySaveData>
{
    private PrefixLogger _Logger = new PrefixLogger(new UnityLogger, "[GlobalReadOnlySaveData]");
}

using UnityEngine;

/// <summary>
/// 生のデータ配列を保持する
/// BytePacker.Packしたデータ配列を必ずここにSet関数を作ること
/// また該当するデータにのみget関数を許可します
/// 継承不可
/// </summary>
public sealed partial  class GlobalRawSaveData : SingletonMonoBehavior<GlobalRawSaveData>
{

}

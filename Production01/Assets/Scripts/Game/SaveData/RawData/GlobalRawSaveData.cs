using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 生のデータ配列を保持する
/// BytePacker.Packしたデータ配列を必ずここにSet関数を作ること
/// また該当するデータにのみget関数を許可します
/// 継承不可
/// </summary>
public sealed partial  class GlobalRawSaveData : SingletonMonoBehavior<GlobalRawSaveData>
{
    public delegate byte[] RawDataMapping();

    private List<SaveLoadTags.eInnerTypeTag> _UpdateSaveTypeList;

    /// <summary>
    /// 読み取りとして扱いなさい
    /// このデータを外でいじくることを禁じる
    /// </summary>
    public List<SaveLoadTags.eInnerTypeTag> UpdateSaveTypeList => _UpdateSaveTypeList;


    public void ResetUpdateSaveType()
    {
        UpdateSaveTypeList.Clear();
    }


    /// <summary>
    /// 何かあれば入れてください
    /// </summary>
    public void Setup()
    {
        _UpdateSaveTypeList = new List<SaveLoadTags.eInnerTypeTag>();

        //重くなってそうならStartに移動
        SetupInputData();
        SetupRawSystemData();
        SetupSkillData();
    }
}

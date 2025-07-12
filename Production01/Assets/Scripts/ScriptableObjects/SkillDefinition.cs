using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkillDefinition", menuName = "Scriptable Objects/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    [System.Serializable]
    public class SkillData
    {
        [HideInInspector]
        public uint SkillTier;//スキルのティアランク
        [HideInInspector]
        public SkillEnums.SkillCategory eMainSkillCategory;//メインのカテゴリ
        public SkillEnums.SkillCategory eSubSkillCategory;//サブのカテゴリ複数あり
        public SkillEnums.SkillTriggerTiming eSkillTriggerTiming;//発動タイミング
        //将来的に追加はある(アイコン番号とか,説明テキストIDとか？肥大化はマジで注意今危ないよ)

        //触られるとややこしいからprivate(全部触んなとはおもうが)
        [SerializeField]
        private uint _ID;//スキルのID明確にする
        private string _AddressPass;//アドレスパス

        public SkillData(uint id,string addressPass)
        {
            this._ID = id;
            this._AddressPass = addressPass;
        }
        public uint ID => _ID;
        public string AddressPass => _AddressPass;
    }

    [System.Serializable]
    public class SkillTierPackage
    {    
        public  uint SkillTier;
        public  SkillData[] SkillDataList;

        /// <summary>
        /// エラー判定は呼び出し側でしてくれ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillData FindSkillDataByID(uint id)
        {
            foreach(SkillData data in SkillDataList)
            {
                if (data.ID == id)
                {
                    return data;
                }
            }
            return null;
        }
    }

    [System.Serializable]
    public class SkillGroup
    {
        public SkillEnums.SkillCategory eMainSkillCategory;//メインのカテゴリ
        public SkillTierPackage[] SkillTierPackageArray;

        /// <summary>
        /// SkillTierPackageの取得
        /// エラー判定は呼び出し側でしてくれ
        /// </summary>
        /// <param name="tier"></param>
        /// <returns></returns>
        public SkillTierPackage FindSkillTierPackage(uint tier)
        {
            foreach (var group in SkillTierPackageArray)
            {
                if (tier == group.SkillTier)
                {
                    return group;
                }
            }
            return null;
        }

        /// <summary>
        /// SkillDataの取得
        /// エラー判定は呼び出し側でしてくれ
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillData FindSkillData(uint tier, uint id)
        {
            SkillTierPackage skillPackage = FindSkillTierPackage(tier);
            if (skillPackage != null)
            {
                return skillPackage.FindSkillDataByID(id);
            }

            return null;
        }
    }

    [SerializeField]
    private SkillGroup _SkillGroup;


    /// <summary>
    /// SkillTierPackageの取得
    /// エラー判定は呼び出し側でしてくれ
    /// </summary>
    /// <param name="tier"></param>
    /// <returns></returns>
    public SkillTierPackage FindTargetSkillTierPackage(uint tier)
    {
        return _SkillGroup.FindSkillTierPackage(tier);
    }

    /// <summary>
    /// SkillDataの取得
    /// エラー判定は呼び出し側でしてくれ
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public SkillData FindTargetSkillData(uint tier, uint id)
    {
        return _SkillGroup.FindSkillData(tier, id);
    }
}

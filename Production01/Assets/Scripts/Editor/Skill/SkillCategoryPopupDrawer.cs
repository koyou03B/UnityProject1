using UnityEditor;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Reflection;

[CustomPropertyDrawer(typeof(SkillEnums.SkillCategory))]
public class SkillCategoryPopupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SkillEnums.SkillCategory currentValue = (SkillEnums.SkillCategory)property.intValue;

        // EnumFlagsFieldを使って複数選択UIを表示
        SkillEnums.SkillCategory newValue = (SkillEnums.SkillCategory)EditorGUI.EnumFlagsField(position, label, currentValue);

        //新しくなってたら変更
        if (newValue != currentValue)
        {
            property.intValue = (int)newValue;
        }

        // ポップアップボタンを描画
        Rect buttonRect = new Rect(position.xMax - 20, position.y, 20, position.height);
        if (GUI.Button(buttonRect, "?"))
        {
            //別Windowに説明分を乗せる
            TooltipPopupWindow.ShowWindow(GetTooltipText(newValue));
        }
    }

    private string GetTooltipText(SkillEnums.SkillCategory value)
    {
        if (value == SkillEnums.SkillCategory.None)
            return "なし";

        //as をつけてキャストすることで安全 null扱いになる
        var categories = Enum.GetValues(typeof(SkillEnums.SkillCategory)) as SkillEnums.SkillCategory[];
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var cat in categories)
        {
            if (cat == SkillEnums.SkillCategory.None) continue;
            //catを含んでいたら
            if (value.HasFlag(cat))
            {
                string desc = GetDescription(cat);
                sb.AppendLine($"・{desc}");
            }
        }
        return sb.ToString();
    }

    public static string GetDescription(Enum value)
    {
        if (value == null) return null;

        //型の取得
        Type type = value.GetType();
        string name = value.ToString();

        // Flags enumの場合、複数の名前がカンマ区切りで返るので分解する
        var names = name.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var n in names)
        {
            //名前からフィールドの情報を取得
            //例:"Offensive" → SkillCategory.Offensive
            FieldInfo field = type.GetField(n);
            if (field != null)
            {
                //Descriptionの取得
                var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                    sb.AppendLine(attr.Description);//説明分を追加
                else
                    sb.AppendLine(n);
            }
            else
            {
                sb.AppendLine(n);
            }
        }

        return sb.ToString().TrimEnd();
    }

}

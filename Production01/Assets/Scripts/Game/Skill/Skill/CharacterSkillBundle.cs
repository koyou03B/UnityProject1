using UnityEngine;

/// <summary>
/// 責任をもつわけではなく
/// ただまとめて保持しておくだけ
/// </summary>
public class CharacterSkillBundle
{
    public CharacterSkillContext Context { get; }
    public SkillSlotHandler Handler { get; }
    public SkillSaveLoader Loader { get; }

    public CharacterSkillBundle()
    {
        Context = new CharacterSkillContext();
        Handler = new SkillSlotHandler(Context);
        Loader = new SkillSaveLoader(Context);
    }

}

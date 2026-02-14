using UnityEditor;
using UnityEngine;
using Nodebreaker.Data;

public class SkillDataCreator
{
    [MenuItem("Tools/Nodebreaker/Create Skill Data Assets")]
    static void CreateSkillAssets()
    {
        string folder = "Assets/Data/Skills";
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Data", "Skills");

        // 공격력 강화
        var atkDmg = ScriptableObject.CreateInstance<SkillNodeData>();
        atkDmg.skillId = "atk_dmg";
        atkDmg.skillName = "공격력 강화";
        atkDmg.description = "모든 타워의 데미지가 레벨당 10% 증가합니다.";
        atkDmg.effectType = SkillEffectType.AttackDamage;
        atkDmg.valuePerLevel = 0.10f;
        atkDmg.maxLevel = 20;
        atkDmg.baseCost = 50;
        atkDmg.growthRate = 1.3f;
        atkDmg.position = new Vector2(-1, 0);
        atkDmg.prerequisiteIds = new string[0];
        AssetDatabase.CreateAsset(atkDmg, $"{folder}/Skill_AttackDamage.asset");

        // 공격속도 강화
        var atkSpd = ScriptableObject.CreateInstance<SkillNodeData>();
        atkSpd.skillId = "atk_spd";
        atkSpd.skillName = "공격속도 강화";
        atkSpd.description = "모든 타워의 공격속도가 레벨당 5% 증가합니다.";
        atkSpd.effectType = SkillEffectType.AttackSpeed;
        atkSpd.valuePerLevel = 0.05f;
        atkSpd.maxLevel = 15;
        atkSpd.baseCost = 40;
        atkSpd.growthRate = 1.25f;
        atkSpd.position = new Vector2(1, 0);
        atkSpd.prerequisiteIds = new string[0];
        AssetDatabase.CreateAsset(atkSpd, $"{folder}/Skill_AttackSpeed.asset");

        // 기지 HP 강화
        var baseHp = ScriptableObject.CreateInstance<SkillNodeData>();
        baseHp.skillId = "base_hp";
        baseHp.skillName = "기지 HP 강화";
        baseHp.description = "기지의 최대 HP가 레벨당 5 증가합니다.";
        baseHp.effectType = SkillEffectType.BaseHp;
        baseHp.valuePerLevel = 5f;
        baseHp.maxLevel = 20;
        baseHp.baseCost = 30;
        baseHp.growthRate = 1.2f;
        baseHp.position = new Vector2(0, -1);
        baseHp.prerequisiteIds = new string[0];
        AssetDatabase.CreateAsset(baseHp, $"{folder}/Skill_BaseHp.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Nodebreaker] 스킬 데이터 에셋 3개 생성 완료!");
    }
}

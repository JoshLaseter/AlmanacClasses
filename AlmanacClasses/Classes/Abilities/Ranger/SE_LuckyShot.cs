﻿using UnityEngine;

namespace AlmanacClasses.Classes.Abilities.Ranger;

public class SE_LuckyShot : StatusEffect
{
    private readonly string m_key = "LuckyShot";
    public override void Setup(Character character)
    {
        if (!TalentManager.m_talents.TryGetValue(m_key, out Talent talent)) return;
        m_ttl = talent.GetLength();
        m_startEffects = talent.GetEffectList();
        
        base.Setup(character);
    }

    public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
    {
        if (!TalentManager.m_talents.TryGetValue(m_key, out Talent talent)) return;
        if (m_character is not Player player) return;
        if (player.GetCurrentWeapon().m_shared.m_skillType is not Skills.SkillType.Bows or Skills.SkillType.Crossbows) return;
        ItemDrop.ItemData ammo = player.GetAmmoItem();
        if (ammo == null) return;
        int random = Random.Range(0, 101);
        if (talent.GetChance(talent.GetLevel()) < random) return;
        ItemDrop.ItemData clone = ammo.Clone();
        clone.m_stack = 1;
        player.GetInventory().AddItem(clone);
    }
}
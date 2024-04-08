﻿using AlmanacClasses.Data;
using BepInEx.Configuration;

namespace AlmanacClasses.Classes.Abilities;

public class SE_MonkeyWrench : StatusEffect
{
    public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
    {
        if (!PlayerManager.m_playerTalents.TryGetValue("MonkeyWrench", out Talent talent)) return;
        if (!talent.m_modifiers.TryGetValue(StatusEffectData.Modifier.Attack, out ConfigEntry<float> config)) return;
        hitData.ApplyModifier(config.Value);
    }
}
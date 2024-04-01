﻿using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlmanacClasses.LoadAssets;

public static class LoadedAssets
{
    private static GameObject VFX_BardNotes = null!;
    private static GameObject VFX_MagicRunes = null!;
    private static GameObject VFX_BardNotesBurn = null!;

    public static GameObject lightning_AOE = null!;
    public static GameObject GoblinBeam = null!;
    public static GameObject Meteor = null!;
    public static GameObject TrollStone = null!;
    public static GameObject GDKingRoots = null!;

    public static GameObject SkeletonFriendly = null!;
    public static GameObject CustomTrap = null!;
    
    public static EffectList StaffPreSpawnEffects = null!;
    public static EffectList FX_SummonSkeleton = null!;
    public static EffectList SoothEffects = null!;
    public static EffectList UnSummonEffects = null!;
    public static EffectList TrapArmedEffects = null!;
    public static EffectList FX_MusicNotes = null!;
    
    public static EffectList ShieldHitEffects = null!;
    public static EffectList ShieldBreakEffects = null!;
    public static EffectList BleedEffects = null!;
    public static EffectList VFX_SongOfSpirit = null!;
    
    public static EffectList FX_DvergerPower = null!;
    public static EffectList FX_HealthPotion = null!;
    // public static EffectList FX_EikthyrStomp = null!;

    // public static GameObject VFX_DragonBreath = null!;
    public static EffectList DragonBreath = null!;

    public static EffectList DragonBreathHit = null!;
    
    public static SE_Finder? SE_Finder;
    public static StatusEffect GP_Moder = null!;
    
    public static void InitVFX()
    {
        ZNetScene instance = ZNetScene.instance;
        
        StatusEffect SE_PotionHealthMajor = ObjectDB.instance.GetStatusEffect("Potion_health_major".GetStableHashCode());
        FX_HealthPotion = SE_PotionHealthMajor.m_startEffects;

        SE_Finder = ObjectDB.instance.GetStatusEffect("Wishbone".GetStableHashCode()) as SE_Finder;
        GP_Moder = ObjectDB.instance.GetStatusEffect("GP_Moder".GetStableHashCode());

        SE_Shield? SE_Shield = ObjectDB.instance.GetStatusEffect("Staff_shield".GetStableHashCode()) as SE_Shield;
        if (SE_Shield != null)
        {
            ShieldHitEffects = new EffectList()
            {
                m_effectPrefabs = SE_Shield.m_hitEffects.m_effectPrefabs
            };
            ShieldBreakEffects = new EffectList()
            {
                m_effectPrefabs = SE_Shield.m_breakEffects.m_effectPrefabs
            };
        }

        TrapArmedEffects = new()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = instance.GetPrefab("fx_trap_arm"),
                    m_enabled = true,
                }
            }
        };

        GameObject bleed = Object.Instantiate(instance.GetPrefab("vfx_Wet"), AlmanacClassesPlugin._Root.transform, false);
        bleed.name = "fx_bleed";
        if (bleed.transform.GetChild(0).TryGetComponent(out ParticleSystem particleSystem))
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient()
            {
                mode = ParticleSystemGradientMode.Color,
                color = Color.red,
                colorMin = Color.red,
                colorMax = Color.red
            };
        }
        RegisterToZNetScene(bleed);

        BleedEffects = new()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = bleed,
                    m_enabled = true,
                    m_attach = true,
                    m_scale = true,
                    m_inheritParentScale = true
                }
            }
        };

        GameObject VFX_UndeadBurn = Object.Instantiate(instance.GetPrefab("vfx_UndeadBurn"), AlmanacClassesPlugin._Root.transform, false);
        Transform trails = VFX_UndeadBurn.transform.Find("trails");
        Transform flames = VFX_UndeadBurn.transform.Find("flames");
        Object.Destroy(trails.gameObject);
        Object.Destroy(flames.gameObject);
        VFX_UndeadBurn.name = "vfx_SongOfSpirit";
        RegisterToZNetScene(VFX_UndeadBurn);

        VFX_SongOfSpirit = new()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = VFX_UndeadBurn,
                    m_enabled = true,
                    m_attach = true,
                    m_scale = true,
                },
                new EffectList.EffectData()
                {
                    m_prefab = instance.GetPrefab("fx_DvergerMage_Nova_ring"),
                    m_enabled = true,
                }
            }
        };
        
        VFX_BardNotes = AlmanacClassesPlugin._AssetBundle.LoadAsset<GameObject>("FX_BardNotes");
        VFX_MagicRunes = AlmanacClassesPlugin._AssetBundle.LoadAsset<GameObject>("FX_MagicRunes");
        VFX_BardNotesBurn = AlmanacClassesPlugin._AssetBundle.LoadAsset<GameObject>("vfx_bardNotesBurn");

        RegisterToZNetScene(VFX_BardNotes);
        RegisterToZNetScene(VFX_MagicRunes);
        RegisterToZNetScene(VFX_BardNotesBurn);

        FX_MusicNotes = new()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = VFX_BardNotesBurn,
                    m_enabled = true,
                    m_attach = true
                }
            }
        };
        DragonBreath = new EffectList()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = instance.GetPrefab("vfx_dragon_coldbreath"),
                    m_enabled = true,
                    m_attach = true
                },
                new EffectList.EffectData()
                {
                    m_prefab = instance.GetPrefab("sfx_dragon_coldball_start"),
                    m_enabled = true
                }
            }
        };

        DragonBreathHit = new()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = instance.GetPrefab("vfx_iceblocker_destroyed"),
                    m_enabled = true,
                    m_attach = true,
                }
            }
        };
        
        GameObject customLightning = Object.Instantiate(ZNetScene.instance.GetPrefab("lightningAOE"), AlmanacClassesPlugin._Root.transform, false);
        customLightning.name = "lightning_strike";
        if (customLightning.transform.Find("AOE_ROD").TryGetComponent(out Aoe aoe))
        {
            aoe.m_useTriggers = true;
            aoe.m_triggerEnterOnly = true;
            aoe.m_blockable = true;
            aoe.m_dodgeable = true;
            aoe.m_hitProps = false;
            aoe.m_hitOwner = false;
            aoe.m_hitParent = false;
            aoe.m_hitFriendly = false;
        }
        if (customLightning.transform.Find("AOE_AREA").TryGetComponent(out Aoe component))
        {
            component.m_useTriggers = true;
            component.m_triggerEnterOnly = true;
            component.m_blockable = true;
            component.m_dodgeable = true;
            component.m_hitProps = false;
            component.m_hitOwner = false;
            component.m_hitParent = false;
            component.m_hitFriendly = false;
        }
        RegisterToZNetScene(customLightning);

        lightning_AOE = customLightning;
        GoblinBeam = instance.GetPrefab("projectile_beam");
        Meteor = instance.GetPrefab("projectile_meteor");
        // FX_EikthyrStomp = new EffectList()
        // {
        //     m_effectPrefabs = new[]
        //     {
        //         new EffectList.EffectData()
        //         {
        //             m_prefab = ZNetScene.instance.GetPrefab("fx_eikthyr_stomp"),
        //             m_enabled = true,
        //         }
        //     }
        // };
        TrollStone = instance.GetPrefab("troll_throw_projectile");
        GDKingRoots = instance.GetPrefab("gdking_root_projectile");
        GameObject customTrap = Object.Instantiate(instance.GetPrefab("piece_trap_troll"), AlmanacClassesPlugin._Root.transform, false);
        customTrap.name = "RangerTrap";

        if (customTrap.TryGetComponent(out Piece piece))
        {
            piece.m_resources = null;
            piece.m_destroyedLootPrefab = null;
        }

        if (customTrap.TryGetComponent(out ZNetView zNetView))
        {
            zNetView.m_persistent = false;
        }
        
        if (customTrap.TryGetComponent(out Trap trapComponent))
        {
            trapComponent.m_startsArmed = true;
            trapComponent.m_triggeredByPlayers = false;
        }
        RegisterToZNetScene(customTrap);
        
        CustomTrap = customTrap;
        
        SkeletonFriendly = instance.GetPrefab("Skeleton_Friendly");
        if (SkeletonFriendly.TryGetComponent(out Tameable tameable))
        {
            SoothEffects = tameable.m_sootheEffect;
            UnSummonEffects = tameable.m_unSummonEffect;
        }

        if (instance.GetPrefab("StaffSkeleton").TryGetComponent(out ItemDrop staffSkeletonItemDrop))
        {
            GameObject projectile = staffSkeletonItemDrop.m_itemData.m_shared.m_attack.m_attackProjectile;
            if (projectile.TryGetComponent(out SpawnAbility spawnAbility))
            {
                StaffPreSpawnEffects = spawnAbility.m_preSpawnEffects;
            }
        }

        FX_SummonSkeleton = new EffectList()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = instance.GetPrefab("fx_summon_skeleton"),
                    m_enabled = true,
                }
            }
        };
        
        GameObject FX_TalentPower = Object.Instantiate(instance.GetPrefab("fx_DvergerMage_Support_start"), AlmanacClassesPlugin._Root.transform, false);
        FX_TalentPower.name = "fx_TalentPower";
        ParticleSystem spark = FX_TalentPower.transform.Find("sparks").GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = spark.main;
        mainModule.loop = true;
        Object.Destroy(FX_TalentPower.GetComponent<TimedDestruction>());
        RegisterToZNetScene(FX_TalentPower);

        if (!FX_TalentPower) return;
        FX_DvergerPower = new EffectList()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = FX_TalentPower,
                    m_enabled = true,
                    m_attach = true,
                    m_inheritParentScale = true,
                    m_inheritParentRotation = true
                }
            }
        };
        
    }

    private static void RegisterToZNetScene(GameObject prefab)
    {
        if (!ZNetScene.instance.m_prefabs.Contains(prefab))
        {
            ZNetScene.instance.m_prefabs.Add(prefab);
        }

        if (!ZNetScene.instance.m_namedPrefabs.ContainsKey(prefab.name.GetStableHashCode()))
        {
            ZNetScene.instance.m_namedPrefabs[prefab.name.GetStableHashCode()] = prefab;
        }
    }
    
    public static EffectList AddBardFX(Color color, string name, bool AddMagicRunes = false)
    {
        EffectList output = new();
        List<EffectList.EffectData> effects = new();
        
        if (!VFX_BardNotes) return output;
        GameObject VFX = Object.Instantiate(VFX_BardNotes, AlmanacClassesPlugin._Root.transform, false);
        if (!VFX) return output;
        VFX.name = name;
        RegisterToZNetScene(VFX);

        ParticleSystem[] FX_PS = VFX.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in FX_PS)
        {
            ParticleSystem.MainModule mainModule = ps.main;
            mainModule.startColor = color;
        }
        
        effects.Add(new EffectList.EffectData()
        {
            m_prefab = VFX,
            m_attach = true,
            m_enabled = true,
            m_inheritParentScale = true,
            m_inheritParentRotation = true,
        });

        if (AddMagicRunes)
        {
            GameObject Runes = Object.Instantiate(VFX_MagicRunes, AlmanacClassesPlugin._Root.transform, false);
            Runes.name = name + "_Runes";
            RegisterToZNetScene(Runes);

            ParticleSystem[] Runes_PS = Runes.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in Runes_PS)
            {
                ParticleSystem.MainModule mainModule = ps.main;
                mainModule.startColor = color;
                mainModule.loop = false;
            }
            
            effects.Add(new ()
            {
                m_prefab = Runes,
                m_enabled = true,
            });
        }

        effects.Add(new()
        {
            m_prefab = ZNetScene.instance.GetPrefab("sfx_dverger_fireball_rain_shot"),
            m_enabled = true
        });
        
        output = new EffectList()
        {
            m_effectPrefabs = effects.ToArray()
        };

        return output;
    }
}
﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AlmanacClasses.Classes;
using AlmanacClasses.Classes.Abilities;
using AlmanacClasses.Classes.Abilities.Warrior;
using AlmanacClasses.FileSystem;
using AlmanacClasses.LoadAssets;
using AlmanacClasses.Managers;
using AlmanacClasses.UI;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;

namespace AlmanacClasses
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class AlmanacClassesPlugin : BaseUnityPlugin
    {
        internal const string ModName = "AlmanacClasses";
        internal const string ModVersion = "0.4.13";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static readonly string ConfigFileName = ModGUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource AlmanacClassesLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public enum Toggle { On = 1, Off = 0 }

        public static readonly AssetBundle _AssetBundle = GetAssetBundle("classesbundle");
        public static AlmanacClassesPlugin _Plugin = null!;
        public static GameObject _Root = null!;
        public void Awake()
        {
            Localizer.Load(); 

            _Plugin = this;
            
            _Root = new GameObject("almanac_classes_root");
            DontDestroyOnLoad(_Root);
            _Root.SetActive(false);
            
            InitConfigs();
            FilePaths.CreateFolders();
            LoadPieces.LoadClassAltar();
            
            AnimationManager.LoadCustomAnimations();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            AddAttackSpeedModifiers();
            
            Watcher.InitWatcher();
        }
        public void Update()
        {
            float dt = Time.deltaTime;
            
            TalentBook.UpdateTalentBookUI();
            AbilityManager.CheckInput();
            
            SpellElementChange.UpdateSpellMouseElement();
            ExperienceBarMove.UpdateElement();
            SpellBarMove.UpdateElement();
            
            PlayerManager.UpdatePassiveEffects(dt);

            if (Input.GetKeyDown(_ShowUIKey.Value)) TalentBook.ShowUI();
        }

        private void AddAttackSpeedModifiers()
        {
            AnimationSpeedManager.Add((character, speed) =>
            {
                if (character is not Player player || !player.InAttack() || player.m_currentAttack is null) return speed;
                return speed * CharacteristicManager.GetDexterityModifier();
            });
            
            AnimationSpeedManager.Add((character, speed) =>
            {
                if (character is not Player player || !player.InAttack() || player.m_currentAttack is null) return speed;
                if (!PlayerManager.m_playerTalents.TryGetValue("MonkeyWrench", out Talent talent)) return speed;
                if (player.GetCurrentWeapon() == null) return speed;
                if (!MonkeyWrench.IsMonkeyWrenchItem(player.GetCurrentWeapon().m_shared.m_name)) return speed;
                return speed * talent.GetAttackSpeedReduction(talent.GetLevel());
            });
        }

        #region Utils
        private void OnDestroy() => Config.Save();
        
        private static AssetBundle GetAssetBundle(string fileName)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
            return AssetBundle.LoadFromStream(stream);
        }
        
        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                AlmanacClassesLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                AlmanacClassesLogger.LogError($"There was an issue loading your {ConfigFileName}");
                AlmanacClassesLogger.LogError("Please check your config entries for spelling and format!");
            }
        }
        #endregion

        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        #region Settings
        public static ConfigEntry<int> _PrestigeThreshold = null!;
        public static ConfigEntry<int> _ResetCost = null!;
        public static ConfigEntry<Toggle> _DisplayExperience = null!;
        public static ConfigEntry<Vector2> _ExperienceBarPos = null!;
        public static ConfigEntry<float> _ExperienceBarScale = null!;
        public static ConfigEntry<Toggle> _HudVisible = null!;
        public static ConfigEntry<Vector2> _SpellBookPos = null!;
        private static ConfigEntry<Toggle> _PanelBackground = null!;
        public static ConfigEntry<float> _ExperienceMultiplier = null!;
        public static ConfigEntry<int> _TalentPointPerLevel = null!;
        public static ConfigEntry<int> _TalentPointsPerTenLevel = null!;
        public static ConfigEntry<Vector2> _MenuTooltipPosition = null!;
        public static ConfigEntry<int> _ChanceForOrb = null!;
        public static ConfigEntry<int> _MaxLevel = null!;
        private static ConfigEntry<KeyCode> _ShowUIKey = null!;
        public static ConfigEntry<Toggle> _EnableRaven = null!;
        #endregion
        public static ConfigEntry<float> _EitrRatio = null!;
        public static ConfigEntry<float> _HealthRatio = null!;
        public static ConfigEntry<float> _StaminaRatio = null!;
        public static ConfigEntry<float> _PhysicalRatio = null!;
        public static ConfigEntry<float> _ElementalRatio = null!;
        public static ConfigEntry<float> _SpeedRatio = null!;
        public static ConfigEntry<float> _CarryWeightRatio = null!;
        public static ConfigEntry<int> _StatsCost = null!;
        #region Key Codes
        public static ConfigEntry<KeyCode> _SpellAlt = null!;
        public static ConfigEntry<KeyCode> _Spell1 = null!;
        public static ConfigEntry<KeyCode> _Spell2 = null!;
        public static ConfigEntry<KeyCode> _Spell3 = null!;
        public static ConfigEntry<KeyCode> _Spell4 = null!;
        public static ConfigEntry<KeyCode> _Spell5 = null!;
        public static ConfigEntry<KeyCode> _Spell6 = null!;
        public static ConfigEntry<KeyCode> _Spell7 = null!;
        public static ConfigEntry<KeyCode> _Spell8 = null!;
        #endregion
        private void InitConfigs()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            InitSettingsConfigs();
            InitKeyCodeConfigs();

            _EitrRatio = config("4 - Characteristics", "1. Eitr Ratio", 2f, new ConfigDescription("Set the amount of wisdom points required for 1 eitr", new AcceptableValueRange<float>(1f, 10f)));
            _HealthRatio = config("4 - Characteristics", "3. Health Ratio", 1f, new ConfigDescription("Set the amount of constitution points for 1 health", new AcceptableValueRange<float>(1f, 10f)));
            _StaminaRatio = config("4 - Characteristics", "5. Stamina Ratio", 3f, new ConfigDescription("Set the amount of dexterity points for 1 stamina", new AcceptableValueRange<float>(1f, 10f)));
            _PhysicalRatio = config("4 - Characteristics", "6. Physical Ratio", 10f, new ConfigDescription("Set the ratio of strength to physical damage", new AcceptableValueRange<float>(1f, 10f)));
            _ElementalRatio = config("4 - Characteristics", "7. Elemental Ratio", 10f, new ConfigDescription("Set the ratio of intelligence to elemental damage", new AcceptableValueRange<float>(1f, 10f)));
            _SpeedRatio = config("4 - Characteristics", "7. Attack Speed Ratio", 3f, new ConfigDescription("Set the ratio of dexterity to increase attack speed", new AcceptableValueRange<float>(1f, 10f)));
            _CarryWeightRatio = config("4 - Characteristics", "8. Carry Weight Ratio", 1f, new ConfigDescription("Set the ratio of strength to increased carry weight", new AcceptableValueRange<float>(1, 10f)));
            _StatsCost = config("4 - Characteristics", "9. Purchase Cost", 3, new ConfigDescription("Set the cost to unlock characteristic talents", new AcceptableValueRange<int>(1, 10)));

            _EnableRaven = config("2 - Settings", "Raven", Toggle.On,
                "If on, plugin adds Munin raven to altar prefab");
        }
        private void InitSettingsConfigs()
        {
            _PrestigeThreshold = config("2 - Settings", "Prestige Threshold", 30,
                new ConfigDescription("Minimum amount of talent points needed to spend to access prestige",
                    new AcceptableValueRange<int>(1, 101)));
            _ResetCost = config("2 - Settings", "Reset Cost", 999,
                new ConfigDescription("Set the cost to reset talents", new AcceptableValueRange<int>(0, 999)));

            _DisplayExperience = config("2 - Settings", "Show Creature Experience", Toggle.Off,
                "If on, creature hover names will display the amount of experience they give");

            _ExperienceBarPos = config("2 - Settings", "XP Bar Position", new Vector2(300f, 25f),
                "Set the position of the experience bar", false);
            _ExperienceBarPos.SettingChanged += LoadUI.OnChangeExperienceBarPosition;
            _ExperienceBarScale = config("2 - Settings", "XP Bar Scale", 100f,
                new ConfigDescription("Set the scale of the experience bar", new AcceptableValueRange<float>(0, 100)),
                false);
            _ExperienceBarScale.SettingChanged += LoadUI.OnExperienceBarScaleChange;

            _HudVisible = config("2 - Settings", "XP Bar Visible", Toggle.On, "If on, experience bar is visible on HUD",
                false);
            _HudVisible.SettingChanged += LoadUI.OnChangeExperienceBarVisibility;

            _SpellBookPos = config("2 - Settings", "Spell Bar Position", new Vector2(1500f, 100f),
                "Set the location of the spellbar", false);
            _SpellBookPos.SettingChanged += SpellBook.OnSpellBarPosChange;
            
            _PanelBackground = config("2 - Settings", "UI Background", Toggle.On,
                "If on, panel is visible, else transparent", false);
            _PanelBackground.SettingChanged += (sender, args) => LoadUI.PanelBackground.color = _PanelBackground.Value is Toggle.On ? Color.white : Color.clear;

            _ExperienceMultiplier = config("2 - Settings", "Experience Multiplier", 1f,
                new ConfigDescription("Set experience multiplier to easily increase experience gains",
                    new AcceptableValueRange<float>(-1f, 10f)));
            _TalentPointPerLevel = config("2 - Settings", "Talent Points Per Level", 3,
                new ConfigDescription("Set amount of talent points rewarded per level",
                    new AcceptableValueRange<int>(1, 10)));
            _TalentPointsPerTenLevel = config("2 - Settings", "Talent Points Per Ten Levels", 7,
                new ConfigDescription("Set extra talent points rewarded per 10 levels",
                    new AcceptableValueRange<int>(0, 10)));
            _MenuTooltipPosition = config("2 - Settings", "Menu Tooltip Position", new Vector2(0f, 150f),
                "Set position of spell bar tooltip, always attached to spell bar position", false);
            _MenuTooltipPosition.SettingChanged += LoadUI.OnMenuInfoPanelConfigChange;

            _ChanceForOrb = config("2 - Settings", "Experience Orb Drop Rate", 1,
                new ConfigDescription("Set the drop chance to drop experience orbs",
                    new AcceptableValueRange<int>(0, 100)));
            _MaxLevel = config("2 - Settings", "Max Level", 100, "Set the max level a player can attain");
            _ShowUIKey = config("2 - Settings", "Show UI Key", KeyCode.None, "Set the key to open menu remotely");
        }

        private void InitKeyCodeConfigs()
        {
            _SpellAlt = config("3 - Spell Keys", "Alt Key", KeyCode.LeftAlt, "Set the alt key code, If None, then it ignores", false);
            _SpellAlt.SettingChanged += (sender, args) =>
            {
                if (Localization.instance == null) return;
                LoadUI.SpellBarHotKeyTooltip.text = Localization.instance.Localize($"$info_spellbook_key: <color=orange>{_SpellAlt.Value}</color>");
            };
            _Spell1 = config("3 - Spell Keys", "Spell 1", KeyCode.Alpha1, "Set the key code for spell 1", false);
            _Spell2 = config("3 - Spell Keys", "Spell 2", KeyCode.Alpha2, "Set the key code for spell 2", false);
            _Spell3 = config("3 - Spell Keys", "Spell 3", KeyCode.Alpha3, "Set the key code for spell 3", false);
            _Spell4 = config("3 - Spell Keys", "Spell 4", KeyCode.Alpha4, "Set the key code for spell 4", false);
            _Spell5 = config("3 - Spell Keys", "Spell 5", KeyCode.Alpha5, "Set the key code for spell 5", false);
            _Spell6 = config("3 - Spell Keys", "Spell 6", KeyCode.Alpha6, "Set the key code for spell 6", false);
            _Spell7 = config("3 - Spell Keys", "Spell 7", KeyCode.Alpha7, "Set the key code for spell 7", false);
            _Spell8 = config("3 - Spell Keys", "Spell 8", KeyCode.Alpha8, "Set the key code for spell 8", false);
        }

        #region Config Utils
        public ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        public ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }
        #endregion
        #endregion
    }

    public static class KeyboardExtensions
    {
        public static bool IsKeyDown(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) &&
                   shortcut.Modifiers.All(Input.GetKey);
        }

        public static bool IsKeyHeld(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) &&
                   shortcut.Modifiers.All(Input.GetKey);
        }
    }
}
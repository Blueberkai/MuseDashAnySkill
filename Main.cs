﻿using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModHelper;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using FormulaBase;
using HarmonyLib;

namespace AnySkill
{
    public class Menu : MonoBehaviour
    {
        public static Menu Instance;
        public static string CurrentDirectory = string.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location) ? Environment.CurrentDirectory : Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        KeyCode MenuKey = KeyCode.F10;
        public static bool ShowMenu = true;
        void OnGUI()
        {
            if (ShowMenu && Main.FUCK)
            {
                GUI.Box(new Rect(Screen.width - 460, 0, 460, Screen.height),"Any Skill");
                GUI.Label(new Rect(Screen.width - 200, 30, 200, 20), "Character Skill: "+Main.CharacterSkill.ToString());
                if (GUI.Button(new Rect(Screen.width-200,60,200,20),"Default"))
                {
                    Main.CharacterSkill = -1;
                }
                for (int i = 0; i < Singleton<ConfigManager>.instance["character"].Count; i++)
                {
                    if (GUI.Button(new Rect(Screen.width - 200, i*30+90, 200, 20), Singleton<ConfigManager>.instance.GetJson("character", true)[i]["cosName"].ToObject<string>()))
                    {
                        Main.CharacterSkill = i;
                    }
                }
                GUI.Label(new Rect(Screen.width - 450, 30, 200, 20), "Elfin Skill: "+Main.ElfinSkill.ToString());
                if (GUI.Button(new Rect(Screen.width - 450, 60, 200, 20), "Default"))
                {
                    Main.ElfinSkill = -1;
                }
                for (int i = 0; i < Singleton<ConfigManager>.instance["elfin"].Count; i++)
                {
                    if (GUI.Button(new Rect(Screen.width - 450, i * 30 + 90, 200, 20), Singleton<ConfigManager>.instance.GetJson("elfin", true)[i]["name"].ToObject<string>()))
                    {
                        Main.ElfinSkill = i;
                    }
                }
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(MenuKey))
            {
                ShowMenu = !ShowMenu;
            }
        }
        void Start()
        {
            Instance = this;
            if (!File.Exists(Path.Combine(CurrentDirectory, "AnySkillkey.txt")))
            {
                File.WriteAllText(Path.Combine(CurrentDirectory, "AnySkillkey.txt"), "F10");
            }
            MenuKey = (KeyCode)Enum.Parse(typeof(KeyCode), File.ReadAllText(Path.Combine(CurrentDirectory, "AnySkillkey.txt")));
        }
    }
    public class Main : IMod
    {
        public static bool FUCK = true;
        public string Name => "AnySkill";

        public string Description => "Allow Any Skill for any Character";

        public string Author => "BustR75";

        public string HomePage => "";
        public static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(Main).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }
        public static int CharacterSkill = -1;
        static int LastCharacter;
        public static int ElfinSkill = -1;
        static int LastElfin;
        public void DoPatching()
        {
            Harmony harmony = new Harmony("moe.bustr75.skill");
            harmony.Patch(typeof(MainManager).GetMethod("InitLanguage", BindingFlags.NonPublic | BindingFlags.Instance), null, GetPatch(nameof(OnStart)));
            harmony.Patch(typeof(SkillManager).GetMethod("Apply"), GetPatch(nameof(SkillManagerApplyPrefix)), GetPatch(nameof(SkillManagerApplyPostfix)));
            //harmony.Patch(typeof(StatisticsManager).GetMethod("OnBattleEnd"), GetPatch(nameof(OnBattleEndPrefix)), GetPatch(nameof(OnBattleEndPostfix)));
            harmony.Patch(typeof(StageBattleComponent).GetMethod("End"), GetPatch(nameof(OnBattleEndPrefix)), GetPatch(nameof(SkillManagerApplyPostfix)));
            harmony.Patch(typeof(StageBattleComponent).GetMethod("Dead"), GetPatch(nameof(OnBattleEndPrefix)), GetPatch(nameof(SkillManagerApplyPostfix)));
        }

        private static void OnBattleEndPrefix()
        {
            if (CharacterSkill != -1)
            {
                Singleton<DataManager>.instance["Account"]["SelectedRoleIndex"].SetResult(CharacterSkill);
            }
            if (ElfinSkill != -1)
            {
                Singleton<DataManager>.instance["Account"]["SelectedElfinIndex"].SetResult(CharacterSkill);
            }
            ModLogger.AddLog("", "", Singleton<DataManager>.instance["Account"]["SelectedElfinIndex"].GetResult<int>());
        }
        /*[HarmonyBefore("moe.mdmc.headquarters")]
        private static bool UploadScore(string musicUid, int musicDifficulty, ref string characterUid, ref string elfinUid, int hp, int score, float acc, int combo, string evaluate, int miss, Newtonsoft.Json.Linq.JArray beats, string bmsVersion, Action<int> callback)
        {
            if (CharacterSkill == 2 || Singleton<BattleProperty>.instance.isAutoPlay) return false;
            characterUid = CharacterSkill.ToString();
            elfinUid = ElfinSkill.ToString();
            return true;
        }*/
        private static void OnStart()
        {
            GameObject gameObject = new GameObject("AnySkill");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Menu>();
        }
        static IEnumerator SUNOFA()
        {
            while(!Singleton<StageBattleComponent>.instance.isInGame) yield return new WaitForEndOfFrame();
            while(Singleton<StageBattleComponent>.instance.isInGame || Singleton<StageBattleComponent>.instance.isPause) yield return new WaitForEndOfFrame();
            FUCK = true;
        }
        private static void SkillManagerApplyPrefix()
        {
            LastCharacter = Singleton<DataManager>.instance["Account"]["SelectedRoleIndex"].GetResult<int>();
            if (CharacterSkill != -1)
            {
                Singleton<DataManager>.instance["Account"]["SelectedRoleIndex"].SetResult(CharacterSkill);
            }
            LastElfin = Singleton<DataManager>.instance["Account"]["SelectedElfinIndex"].GetResult<int>();
            if (ElfinSkill != -1)
            {
                Singleton<DataManager>.instance["Account"]["SelectedElfinIndex"].SetResult(ElfinSkill);
            }
            FUCK = false;
            Menu.Instance.StartCoroutine(SUNOFA());
        }
        private static void SkillManagerApplyPostfix()
        {
            if (CharacterSkill != -1)
            {
                Singleton<DataManager>.instance["Account"]["SelectedRoleIndex"].SetResult(LastCharacter);
            }
            if (ElfinSkill != -1)
            {
                Singleton<DataManager>.instance["Account"]["SelectedElfinIndex"].SetResult(LastElfin);
            }
        }
    }
}

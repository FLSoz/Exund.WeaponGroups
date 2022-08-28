using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Exund.WeaponGroups
{
    public class WeaponGroupsMod : ModBase
    {
        public static BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        internal static string asm_path = Assembly.GetExecutingAssembly().Location.Replace("Exund.WeaponGroups.dll", "");

        internal const string HarmonyID = "exund.weapongroups";
        internal static Harmony harmony = new Harmony(HarmonyID);

        private static void Load()
        {
            var holder = new GameObject();
            holder.AddComponent<GroupControllerEditor>();

            GameObject.DontDestroyOnLoad(holder);
        }

        private static bool Inited = false;
        private static ModContainer ThisContainer;

        public override void EarlyInit()
        {
            if (!Inited)
            {
                Dictionary<string, ModContainer> mods = (Dictionary<string, ModContainer>)AccessTools.Field(typeof(ManMods), "m_Mods").GetValue(Singleton.Manager<ManMods>.inst);
                if (mods.TryGetValue("Weapon Groups", out ModContainer thisContainer))
                {
                    ThisContainer = thisContainer;
                    GroupControllerEditor.icon_cancel = ThisContainer.Contents.FindAsset("cancel.png") as Texture2D;
                    GroupControllerEditor.icon_remove = ThisContainer.Contents.FindAsset("remove.png") as Texture2D;
                    GroupControllerEditor.icon_rename = ThisContainer.Contents.FindAsset("rename.png") as Texture2D;
                }
                else
                {
                    Console.WriteLine("FAILED TO FETCH BuilderTools ModContainer");
                }
                Inited = true;
                Load();
            }
        }

        public override bool HasEarlyInit()
        {
            return true;
        }

        public override void DeInit()
        {
            harmony.UnpatchAll(HarmonyID);
        }

        public override void Init()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    static class Patches
    {
        [HarmonyPatch(typeof(ModuleHammer), "ControlInput")]
        static class ModuleHammer_ControlInput
        {
            static bool Prefix(ModuleHammer __instance, int aim, bool fire)
            {
                if (aim != ModuleWeaponGroupController.aim_ID && !fire)
                {
                    if (ModuleWeaponGroupController.groups_for_hammer.TryGetValue(__instance, out var groups))
                    {
                        if (groups.Any(g => g.fireNextFrame))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}

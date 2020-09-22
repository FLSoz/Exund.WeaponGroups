using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Nuterra.BlockInjector;

namespace Exund.WeaponGroups
{
    public class WeaponGroupsMod
    {
        public static BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        internal static string asm_path = Assembly.GetExecutingAssembly().Location.Replace("Exund.WeaponGroups.dll", "");

        public static void Load()
        {
            var holder = new GameObject();
            holder.AddComponent<GroupControllerEditor>();

            GameObject.DontDestroyOnLoad(holder);

            new BlockPrefabBuilder(BlockTypes.HE_StdBlock_01_111)
                .SetBlockID(7030)
                .SetName("Hawkeye Weapons Controller")
                .SetDescription("This block can control the firing of weapons (guns, drills and hammers).\n\nUsed to manage weapon groups.")
                .SetFaction(FactionSubTypes.HE)
                .SetGrade(2)
                .SetCategory(BlockCategories.Accessories)
                .SetRarity(BlockRarity.Rare)
                .SetSize(IntVector3.one)
                .SetAPsManual(new Vector3[]
                {
                    Vector3.down * 0.5f,
                    Vector3.forward * 0.5f,
                    Vector3.left * 0.5f
                })
                .SetPrice(24705)
                .SetRecipe(new Dictionary<ChunkTypes, int> {
                    { ChunkTypes.HardenedTitanic, 1 },
                    { ChunkTypes.HeatCoil, 2 },
                    { ChunkTypes.SensoryTransmitter, 1 },
                    { ChunkTypes.TitanicAlloy, 1 },
                    { ChunkTypes.SeedAI, 1 }
                })
                .SetModel(GameObjectJSON.MeshFromFile(asm_path + "Assets/weapon_group_fusebox.obj"), true, GameObjectJSON.GetObjectFromGameResources<Material>("HE_Main"))
                .SetIcon(GameObjectJSON.ImageFromFile(asm_path + "Assets/weapon_group_fusebox.png"))
                .AddComponent<ModuleWeaponGroupController>()
                .SetCustomEmissionMode(BlockPrefabBuilder.EmissionMode.Active)
                .SetDropFromCrates(true)
                .RegisterLater();
        }
    }
}

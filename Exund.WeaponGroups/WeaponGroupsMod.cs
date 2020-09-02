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

            var cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            new BlockPrefabBuilder(BlockTypes.HE_StdBlock_01_111)
                .SetBlockID(7030)
                .SetName("Hawkeye Weapons Controller")
                .SetDescription("This block can control the firing of weapons (guns, drills and hammers).\n\nUsed to manage weapon groups.")
                .SetFaction(FactionSubTypes.HE)
                .SetCategory(BlockCategories.Accessories)
                .SetRarity(BlockRarity.Rare)
                .SetSize(IntVector3.one)
                .SetModel(cube1, MakeCopy:false)
                /*.SetPrice(58860)
                .SetRecipe(new Dictionary<ChunkTypes, int> {
                    { ChunkTypes.SeedAI, 5 }
                })*/
                //.SetModel(GameObjectJSON.MeshFromFile(asm_path + "Assets/hadamard_superposer.obj"), true, GameObjectJSON.GetObjectFromGameResources<Material>("RR_Main"))
                //.SetIcon(GameObjectJSON.ImageFromFile(asm_path + "Assets/hadamard_superposer.png"))
                .AddComponent<ModuleWeaponGroupController>()
                .RegisterLater();
        }
    }
}

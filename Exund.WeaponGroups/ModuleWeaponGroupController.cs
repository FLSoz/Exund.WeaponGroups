using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Exund.WeaponGroups
{
    public class ModuleWeaponGroupController : Module
    {
        internal static readonly int aim_ID = 7;
        internal static Dictionary<ModuleHammer, List<WeaponGroup>> groups_for_hammer = new Dictionary<ModuleHammer, List<WeaponGroup>>();

        public List<WeaponGroup> groups = new List<WeaponGroup>();

        private SerialData data;


        internal static void RemoveGroupForHammer(ModuleHammer hammer, WeaponGroup group)
        {
            if (ModuleWeaponGroupController.groups_for_hammer.TryGetValue(hammer, out var groups))
            {
                groups.Remove(group);
            }
        }

        private void OnPool()
        {
            base.block.serializeEvent.Subscribe(this.OnSerialize);
            base.block.serializeTextEvent.Subscribe(this.OnSerialize);

            base.block.AttachEvent.Subscribe(OnAttach);
            base.block.DetachEvent.Subscribe(OnDetach);
        }

        void OnAttach()
        {
            block.tank.control.driveControlEvent.Subscribe(GetDriveControl);
            block.tank.DetachEvent.Subscribe(OnBlockDetached);
        }

        void OnDetach()
        {
            CleanHammersGroups();
            this.groups.Clear();
            block.tank.control.driveControlEvent.Unsubscribe(GetDriveControl);
            block.tank.DetachEvent.Unsubscribe(OnBlockDetached);
        }

        void OnRecycle()
        {
            CleanHammersGroups();
        }

        void OnBlockDetached(TankBlock block, Tank tank)
        {
            foreach (var g in groups)
            {
                for (int i = 0; i < g.weapons.Count; i++)
                {
                    var w = g.weapons[i];
                    if (w.block == block)
                    {
                        if(w.hammer)
                        {
                            RemoveGroupForHammer(w.hammer, g);
                        }
                        w.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                        g.weapons.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        private void OnTankPostSpawn()
        {
            if (data == null) return;
            var blockman = base.block.tank.blockman;
            foreach (var group in data.groups)
            {
                var actual_group = new WeaponGroup()
                {
                    name = group.name,
                    keyCode = (KeyCode)group.keyCode
                };

                var weapons = group.positions.Select(p =>
                {
                    var block = blockman.GetBlockAtPosition(p);
                    if (block)
                    {
                        var weapon = new WeaponWrapper(block);
                        if(weapon.hammer)
                        {
                            if (ModuleWeaponGroupController.groups_for_hammer.TryGetValue(weapon.hammer, out var groups))
                            {
                                groups.Add(actual_group);
                            }
                            else
                            {
                                ModuleWeaponGroupController.groups_for_hammer.Add(weapon.hammer, new List<ModuleWeaponGroupController.WeaponGroup>() { actual_group });
                            }
                        }
                        return weapon;
                    }
                    return null;
                }).Where(ww => ww != null).ToList();

                actual_group.weapons = weapons;

                groups.Add(actual_group);
            }
            base.block.tank.ResetPhysicsEvent.Unsubscribe(this.OnTankPostSpawn);
        }


        private void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            if (saving)
            {
                var serialDataSave = new SerialData();
                var list = new List<WeaponGroupSerial>();
                foreach (var group in groups)
                {
                    list.Add(new WeaponGroupSerial()
                    {
                        name = group.name,
                        positions = group.weapons.Select(w => w.block.cachedLocalPosition).ToArray(),
                        keyCode = (int)group.keyCode
                    });
                }
                serialDataSave.groups = list.ToArray();
                serialDataSave.Store(blockSpec.saveState);
                return;
            }
            SerialData serialData = Module.SerialData<ModuleWeaponGroupController.SerialData>.Retrieve(blockSpec.saveState);
            if (serialData != null)
            {
                data = serialData;
                base.block.tank.ResetPhysicsEvent.Subscribe(this.OnTankPostSpawn);
            }
        }

        void GetDriveControl(TankControl.ControlState state)
        {
            foreach (var group in groups)
            {
                var temp = Input.GetKey(group.keyCode);
                if (group.fireNextFrame && !temp || temp)
                {
                    group.fireNextFrame = temp;
                    group.Fire();
                }
            }
        }

        void CleanHammersGroups()
        {
            foreach (var g in groups)
            {
                foreach (var w in g.weapons)
                {
                    if (w.hammer)
                    {
                        RemoveGroupForHammer(w.hammer, g);
                    }
                }
            }
        }

        private class SerialData : Module.SerialData<ModuleWeaponGroupController.SerialData>
        {
            public WeaponGroupSerial[] groups;
        }

        [Serializable]
        public class WeaponGroupSerial
        {
            public string name;
            public Vector3[] positions;
            public int keyCode;
        }

        public class WeaponGroup
        {
            public string name = "New Group";
            public List<WeaponWrapper> weapons = new List<WeaponWrapper>();
            public KeyCode keyCode = KeyCode.Space;

            public bool fireNextFrame;
            public bool forceFireNextFrame;
            public bool forceNoFireNextFrame;

            public void Fire()
            {
                foreach (var w in weapons)
                {
                    w.Fire(fireNextFrame);
                }
            }
        }

        public class WeaponWrapper
        {
            public static MethodInfo drill_ControlInput = typeof(ModuleDrill).GetMethod("ControlInput", WeaponGroupsMod.bindingFlags);
            public static FieldInfo drill_m_Spinning = typeof(ModuleDrill).GetField("m_Spinning", WeaponGroupsMod.bindingFlags);

            public static MethodInfo hammer_ControlInput = typeof(ModuleHammer).GetMethod("ControlInput", WeaponGroupsMod.bindingFlags);
            public static FieldInfo hammer_m_OperatingState = typeof(ModuleHammer).GetField("m_OperatingState", WeaponGroupsMod.bindingFlags);
            public static FieldInfo hammer_actuator = typeof(ModuleHammer).GetField("actuator", WeaponGroupsMod.bindingFlags);

            //public static MethodInfo weapon_ControlInputTargeted = typeof(ModuleWeapon).GetMethod("ControlInputTargeted", WeaponGroupsMod.bindingFlags);
            //public static MethodInfo weapon_ControlInputManual = typeof(ModuleWeapon).GetMethod("ControlInputManual", WeaponGroupsMod.bindingFlags);


            public ModuleWeapon weapon;
            public ModuleDrill drill;
            public ModuleHammer hammer;

            public TankBlock block;

            public WeaponWrapper(TankBlock block)
            {
                this.block = block;
                this.weapon = block.gameObject.GetComponent<ModuleWeapon>();
                this.drill = block.gameObject.GetComponent<ModuleDrill>();
                this.hammer = block.gameObject.GetComponent<ModuleHammer>();
            }

            public void Fire(bool fire)
            {
                if (weapon)
                {
                    //USE https://github.com/Aceba1/Control-Blocks/blob/master/Control Block/ClusterBody.cs LINE 74
                    weapon.FireControl = fire;
                }
                if (drill)
                {
                    drill_ControlInput.Invoke(drill, new object[] { 0, fire });
                }
                if (hammer)
                {
                    hammer_ControlInput.Invoke(hammer, new object[] { aim_ID, fire });
                    
                }
            }
        }
    }
}

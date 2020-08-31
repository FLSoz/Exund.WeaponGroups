using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Exund.WeaponGroups
{
    public class ModuleWeaponGroupController : Module
    {
        public List<WeaponGroup> groups = new List<WeaponGroup>();

        private SerialData data;

        private void OnPool()
        {
            base.block.serializeEvent.Subscribe(this.OnSerialize);
            base.block.serializeTextEvent.Subscribe(this.OnSerialize);
        }

        private void OnTankPostSpawn()
        {
            if (data == null) return;
            var blockman = base.block.tank.blockman;
            foreach (var group in data.groups)
            {
                groups.Add(new WeaponGroup()
                {
                    name = group.name,
                    weapons = group.positions.Select(p =>
                    {
                        return new WeaponWrapper(blockman.GetBlockAtPosition(p));
                    }).ToList(),
                    keyCode = (KeyCode)group.keyCode
                });
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
                //serialDataSave.Store(blockSpec.saveState);
                return;
            }
            SerialData serialData = Module.SerialData<ModuleWeaponGroupController.SerialData>.Retrieve(blockSpec.saveState);
            if (serialData != null)
            {
                data = serialData;
                base.block.tank.ResetPhysicsEvent.Subscribe(this.OnTankPostSpawn);
            }
        }

        void Update()
        {
            foreach (var group in groups)
            {
                if (Input.GetKey(group.keyCode)) group.Fire();
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

            public void Fire()
            {
                foreach (var w in weapons)
                {
                    w.Fire();
                }
            }
        }

        public class WeaponWrapper
        {
            public static MethodInfo drill_ControlInput = typeof(ModuleDrill).GetMethod("ControlInput", WeaponGroupsMod.bindingFlags);
            public static MethodInfo hammer_ControlInput = typeof(ModuleHammer).GetMethod("ControlInput", WeaponGroupsMod.bindingFlags);
            public static MethodInfo weapon_ControlInputTargeted = typeof(ModuleWeapon).GetMethod("ControlInputTargeted", WeaponGroupsMod.bindingFlags);
            public static MethodInfo weapon_ControlInputManual = typeof(ModuleWeapon).GetMethod("ControlInputManual", WeaponGroupsMod.bindingFlags);

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

            public void Fire()
            {
                if (weapon)
                {
                    //USE https://github.com/Aceba1/Control-Blocks/blob/master/Control Block/ClusterBody.cs LINE 74
                    var tankControl = block.tank.control;
                    if(tankControl.TargetRadiusWorld != 0f)
                    {
                        //weapon_ControlInputTargeted.Invoke(weapon, new object[] { tankControl.TargetPositionWorld, tankControl.TargetRadiusWorld });
                        weapon_ControlInputManual.Invoke(weapon, new object[] { 0, 0 });
                        weapon.Process();
                    }    
                }
                if(drill)
                {
                    drill_ControlInput.Invoke(drill, new object[] { 0, true });
                }
                if(hammer)
                {
                    hammer_ControlInput.Invoke(hammer, new object[] { 0, true });
                }
            }
        }
    }
}

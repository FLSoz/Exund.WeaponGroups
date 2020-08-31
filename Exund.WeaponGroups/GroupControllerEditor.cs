using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Exund.WeaponGroups
{
    class GroupControllerEditor : MonoBehaviour
    {
        private readonly int ID = 7790;
        private Rect Win = new Rect(0, 0, 500f, 200f);
        private Vector2 scroll;

        ModuleWeaponGroupController module;
        ModuleWeaponGroupController.WeaponGroup selectedGroup;
        bool renaming = false;
        bool changeKey = false;
        bool selectingWeapon = false;

        void Update()
        {
            if(module && !module.block.tank)
            {
                Clean();
            }

            if(module && selectedGroup != null && selectingWeapon && Input.GetMouseButtonDown(0))
            {
                selectingWeapon = false;
                try
                {
                    var temp_block = Singleton.Manager<ManPointer>.inst.targetVisible.block;
                    if (temp_block && temp_block.tank == module.block.tank)
                    {
                        var weapon = new ModuleWeaponGroupController.WeaponWrapper(temp_block);
                        if(weapon.weapon || weapon.hammer || weapon.drill)
                        {
                            selectedGroup.weapons.Add(weapon);
                            weapon.block.visible.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                        }
                    }
                }
                catch {}
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (module)
                {
                    module.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                }

                try
                {
                    var temp_block = Singleton.Manager<ManPointer>.inst.targetVisible.block;
                    if (temp_block && temp_block.tank && temp_block.gameObject.GetComponent<ModuleWeaponGroupController>())
                    {
                        module = temp_block.gameObject.GetComponent<ModuleWeaponGroupController>();
                        module.block.visible.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                    }
                }
                catch
                {
                    Clean();
                }

                useGUILayout = module;
            }
        }

        void OnGUI()
        {
            if (!module) return;
            Win = GUI.Window(ID, Win, DoWindow, "Weapon Group Controller");
        }

        private void DoWindow(int id)
        {
            if(selectedGroup != null && changeKey)
            {
                Event current = Event.current;
                if (current.isKey)
                {
                    selectedGroup.keyCode = current.keyCode;
                    changeKey = false;
                }
            }


            GUILayout.BeginVertical();
            scroll = GUILayout.BeginScrollView(scroll/*, GUILayout.Height(Win.height)*/);
            {
                for (int i = 0; i < module.groups.Count; i++)
                {
                    var group = module.groups[i];

                    GUILayout.BeginHorizontal(GUI.skin.button);
                    {
                        if (renaming && group == selectedGroup)
                        {
                            group.name = GUILayout.TextField(group.name, GUILayout.Width(125f));
                        }
                        else
                        {
                            GUILayout.Label(group.name, GUILayout.Width(125f));
                        }

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(renaming && group == selectedGroup ? "X" : "Rename"))
                        {
                            renaming = !renaming;
                        }

                        if (GUILayout.Button(changeKey && group == selectedGroup ? "Press a key" : group.keyCode.ToString())) changeKey = true;

                        if (GUILayout.Button("Delete"))
                        {
                            module.groups.RemoveAt(i);
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Input.GetMouseButtonDown(0) && group != selectedGroup)
                    {
                        CleanGroup();

                        selectedGroup = group;

                        foreach (var w in selectedGroup.weapons)
                        {
                            w.block.visible.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                        }
                    } else {
                        if (group == selectedGroup)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(25f);
                            {
                                GUILayout.BeginVertical();
                                for (int j = 0; j < group.weapons.Count; j++)
                                {
                                    var weapon = group.weapons[j];
                                    GUILayout.BeginHorizontal(GUI.skin.button);
                                    {
                                        GUILayout.Label(StringLookup.GetItemName(ObjectTypes.Block, (int)weapon.block.BlockType), GUILayout.Width(125f));

                                        GUILayout.FlexibleSpace();

                                        if (GUILayout.Button("Delete"))
                                        {
                                            weapon.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                                            group.weapons.RemoveAt(j);
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                if (GUILayout.Button("Add weapon to group"))
                                {
                                    selectingWeapon = true;
                                }
                                GUILayout.EndVertical();
                            }
                            GUILayout.EndHorizontal();

                        }
                    }
                }
            }
            GUILayout.EndScrollView();
            if(GUILayout.Button("Add group"))
            {
                CleanGroup();
                selectedGroup = new ModuleWeaponGroupController.WeaponGroup();
                module.groups.Add(selectedGroup);
            }
            GUILayout.EndVertical();
        }

        void Clean()
        {
            if (!module) return;
            module.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
            module = null;

            CleanGroup();
            selectedGroup = null;
        }

        void CleanGroup()
        {
            if (selectedGroup != null)
            {
                foreach (var w in selectedGroup.weapons)
                {
                    w.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                }
            }
            selectingWeapon = false;
            changeKey = false;
            renaming = false;
        }

        private static void ConfirmPopup(string text, out bool? result)
        {
            result = null;
            var width = 300f;
            var height = 150f;
            GUI.BeginGroup(new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height), GUI.skin.window);
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(text);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Yes")) result = true;
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("No")) result = false;
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();              
            GUI.EndGroup();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Nuterra.BlockInjector;

namespace Exund.WeaponGroups
{
    class GroupControllerEditor : MonoBehaviour
    {
        static Texture2D icon_cancel = GameObjectJSON.ImageFromFile(Path.Combine(WeaponGroupsMod.asm_path, "Assets/Icons/cancel.png"));
        static Texture2D icon_remove = GameObjectJSON.ImageFromFile(Path.Combine(WeaponGroupsMod.asm_path, "Assets/Icons/remove.png"));
        static Texture2D icon_rename = GameObjectJSON.ImageFromFile(Path.Combine(WeaponGroupsMod.asm_path, "Assets/Icons/rename.png"));
        static float icon_size = 30;

        static GUIStyle textField;
        static GUIStyle middledText;
        static GUIStyle bigText;

        private readonly int ID = 7790;
        private Rect Win = new Rect(0, 0, 500f, 300f);
        private Vector2 scroll;

        ModuleWeaponGroupController module;
        ModuleWeaponGroupController.WeaponGroup selectedGroup;
        bool renaming = false;
        bool changeKey = false;
        bool selectingWeapon = false;
        bool weaponSelected = false;
        bool groupAdded = false;
        bool expanded = false;

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
                        if(weapon.weapon /*|| weapon.hammer || weapon.drill*/)
                        {
                            selectedGroup.weapons.Add(weapon);
                            weapon.block.visible.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                            weaponSelected = true;
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
            if(textField == null)
            {
                textField = new GUIStyle(GUI.skin.textField)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 25
                };

                middledText = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft
                };

                bigText = new GUIStyle(middledText)
                {
                    fontSize = 25
                };
            }

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
            scroll = GUILayout.BeginScrollView(scroll);
            {
                for (int i = 0; i < module.groups.Count; i++)
                {
                    var group = module.groups[i];

                    GUILayout.BeginHorizontal(GUI.skin.button);
                    {
                        if (renaming && group == selectedGroup)
                        {
                            group.name = GUILayout.TextField(group.name, textField, GUILayout.Width(250), GUILayout.Height(icon_size));
                        }
                        else
                        {
                            GUILayout.Label(group.name, bigText, GUILayout.Width(250));
                        }

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(renaming && group == selectedGroup ? icon_cancel : icon_rename, GUILayout.Width(icon_size), GUILayout.Height(icon_size)))
                        {
                            renaming = !renaming;
                            changeKey = false;
                        }

                        if (GUILayout.Button(changeKey && group == selectedGroup ? "Press a key" : group.keyCode.ToString(), GUILayout.MinWidth(icon_size))) changeKey = true;

                        if (GUILayout.Button(icon_remove, GUILayout.Width(icon_size), GUILayout.Height(icon_size)))
                        {
                            module.groups.RemoveAt(i);
                            CleanGroup();
                        }
                    }
                    GUILayout.EndHorizontal();

                    var tempgroup = selectedGroup;
                    var tempExpanded = expanded;
                    if (Event.current.type == EventType.Repaint)
                    {
                        var lastrect = GUILayoutUtility.GetLastRect();
                        if (groupAdded && i == module.groups.Count - 1)
                        {
                            groupAdded = false;
                            scroll.y = lastrect.y + lastrect.height / 2;
                        }
                        if (lastrect.Contains(Event.current.mousePosition) && Input.GetMouseButtonDown(0)) 
                        {
                            if (group != selectedGroup)
                            {
                                CleanGroup();
                                expanded = true;

                                selectedGroup = group;

                                foreach (var w in selectedGroup.weapons)
                                {
                                    w.block.visible.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                                }
                            } else {
                                expanded = !expanded;
                            }
                        }
                    }
                    
                    if(selectedGroup == tempgroup) {
                        if (group == selectedGroup && tempExpanded)
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
                                        GUILayout.Label(StringLookup.GetItemName(ObjectTypes.Block, (int)weapon.block.BlockType), middledText, GUILayout.Width(250));

                                        GUILayout.FlexibleSpace();

                                        if (GUILayout.Button(icon_remove, GUILayout.Width(icon_size), GUILayout.Height(icon_size)))
                                        {
                                            weapon.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                                            group.weapons.RemoveAt(j);
                                        }
                                    }
                                    GUILayout.EndHorizontal();

                                    if(Event.current.type == EventType.Repaint && weaponSelected && j == group.weapons.Count - 1)
                                    {
                                        weaponSelected = false;
                                        var rect = GUILayoutUtility.GetLastRect();
                                        scroll.y = rect.y + rect.height / 2;
                                    }
                                }
                                if (GUILayout.Button(selectingWeapon ? "Selecting weapon..." : "Add weapon to group"))
                                {
                                    selectingWeapon = true;
                                }
                                GUILayout.EndVertical();
                            }
                            GUILayout.EndHorizontal();

                        }
                    }

                    /*if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Input.GetMouseButtonDown(0))
                    {
                        if (group != selectedGroup)
                        {
                            CleanGroup();
                            expanded = true;

                            selectedGroup = group;

                            foreach (var w in selectedGroup.weapons)
                            {
                                w.block.visible.EnableOutlineGlow(true, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                            }
                        } else {
                            expanded = !expanded;
                        }
                    } else {
                        if (group == selectedGroup /*&& expanded)
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

                                        if (GUILayout.Button(icon_remove, GUILayout.Width(icon_size), GUILayout.Height(icon_size)))
                                        {
                                            weapon.block.visible.EnableOutlineGlow(false, cakeslice.Outline.OutlineEnableReason.ScriptHighlight);
                                            group.weapons.RemoveAt(j);
                                        }
                                    }
                                    GUILayout.EndHorizontal();

                                    if(Event.current.type == EventType.Repaint && weaponSelected)
                                    {
                                        weaponSelected = false;
                                        var rect = GUILayoutUtility.GetLastRect();
                                        scroll.y = rect.y + rect.height / 2;
                                    }
                                }
                                if (GUILayout.Button(selectingWeapon ? "Selecting weapon..." : "Add weapon to group"))
                                {
                                    selectingWeapon = true;
                                }
                                GUILayout.EndVertical();
                            }
                            GUILayout.EndHorizontal();
                        }
                    }

                    if(Event.current.type == EventType.Repaint && groupAdded && i == module.groups.Count - 1)
                    {
                        weaponSelected = false;
                        var rect = GUILayoutUtility.GetLastRect();
                        scroll.y = rect.y + rect.height / 2;
                    }*/
                }
            }
            GUILayout.EndScrollView();
            if(GUILayout.Button("Add group"))
            {
                CleanGroup();
                selectedGroup = new ModuleWeaponGroupController.WeaponGroup();
                module.groups.Add(selectedGroup);
                groupAdded = true;
            }
            GUILayout.EndVertical();

            GUI.DragWindow();
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
            weaponSelected = false;
            groupAdded = false;
            changeKey = false;
            renaming = false;
            expanded = false;
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

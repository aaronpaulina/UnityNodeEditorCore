﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public partial class NodeEditorWindow {

    public void DrawToolbar() {
        EditorGUILayout.BeginHorizontal("Toolbar");

        if (DropdownButton("File", 50)) FileContextMenu();
        if (DropdownButton("Edit", 50)) EditContextMenu();
        if (DropdownButton("View", 50)) { }
        if (DropdownButton("Settings", 70)) { }
        if (DropdownButton("Tools", 50)) ToolsContextMenu();

        //Draw hover info
        if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint) {
            if (IsHoveringNode) {
                    GUILayout.Space(20);
                    string hoverInfo = hoveredNode.GetType().ToString();
                    if (IsHoveringPort) hoverInfo += " > " + hoveredPort.name;
                    GUILayout.Label(hoverInfo);
            }
        }

        // Make the toolbar extend all throughout the window extension.
        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }

    public void FileContextMenu() {
        GenericMenu contextMenu = new GenericMenu();

        contextMenu.AddItem(new GUIContent("Save"), false, Save);
        contextMenu.AddItem(new GUIContent("Save As"), false, SaveAs);

        contextMenu.DropDown(new Rect(5f, 17f, 0f, 0f));
    }

    public void EditContextMenu() {
        GenericMenu contextMenu = new GenericMenu();
        contextMenu.AddItem(new GUIContent("Clear"), false, () => graph.Clear());

        contextMenu.DropDown(new Rect(5f, 17f, 0f, 0f));
    }

    public void ToolsContextMenu() {
        GenericMenu contextMenu = new GenericMenu();
        contextMenu.AddItem(new GUIContent("Debug Custom Node Editors"), false, () => CacheCustomNodeEditors());

        contextMenu.DropDown(new Rect(5f, 17f, 0f, 0f));
    }
}

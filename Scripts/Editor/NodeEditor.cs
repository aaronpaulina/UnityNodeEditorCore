﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

/// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
public class NodeEditor {

    public Node target;

    /// <summary> Draws the node GUI.</summary>
    /// <param name="portPositions">Port handle positions need to be returned to the NodeEditorWindow </param>
    public virtual void OnNodeGUI(out Dictionary<NodePort,Vector2> portPositions) {
        DrawDefaultHeaderGUI();
        DrawDefaultNodePortsGUI(out portPositions);
        DrawDefaultNodeBodyGUI();
    }

    protected void DrawDefaultHeaderGUI() {
        
        GUILayout.Label(target.name, NodeEditorResources.styles.headerStyle, GUILayout.Height(30));
    }

    /// <summary> Draws standard editors for all fields marked with <see cref="Node.InputAttribute"/> or <see cref="Node.OutputAttribute"/> </summary>
    protected void DrawDefaultNodePortsGUI(out Dictionary<NodePort, Vector2> portPositions) {
        portPositions = new Dictionary<NodePort, Vector2>();

        Event e = Event.current;

        GUILayout.BeginHorizontal();

        //Inputs
        GUILayout.BeginVertical();
        for (int i = 0; i < target.InputCount; i++) {
            Vector2 handlePoint = DrawNodePortGUI(target.inputs[i]);
            portPositions.Add(target.inputs[i], handlePoint);
        }
        GUILayout.EndVertical();

        //Outputs
        GUILayout.BeginVertical();
        for (int i = 0; i < target.OutputCount; i++) {
            Vector2 handlePoint = DrawNodePortGUI(target.outputs[i]);
            portPositions.Add(target.outputs[i], handlePoint);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    /// <summary> Draws standard field editors for all public fields </summary>
    protected void DrawDefaultNodeBodyGUI() {
        FieldInfo[] fields = GetInspectorFields(target);
        for (int i = 0; i < fields.Length; i++) {
            object[] fieldAttribs = fields[i].GetCustomAttributes(false);
            if (NodeEditorUtilities.HasAttrib<Node.InputAttribute>(fieldAttribs) || NodeEditorUtilities.HasAttrib<Node.OutputAttribute>(fieldAttribs)) continue;
            DrawFieldInfoDrawerGUI(fields[i]);
        }
        EditorGUILayout.Space();
    }

    /// <summary> Draw node port GUI using automatic layouting. Returns port handle position. </summary>
    protected Vector2 DrawNodePortGUI(NodePort port) {
        GUIStyle style = port.direction == NodePort.IO.Input ? NodeEditorResources.styles.inputStyle : NodeEditorResources.styles.outputStyle;
        Rect rect = GUILayoutUtility.GetRect(new GUIContent(port.name.PrettifyCamelCase()), style);
        return DrawNodePortGUI(rect, port);
    }

    /// <summary> Draw node port GUI in rect. Returns port handle position. </summary>
    protected Vector2 DrawNodePortGUI(Rect rect, NodePort port) {
        GUIStyle style = port.direction == NodePort.IO.Input ? NodeEditorResources.styles.inputStyle : NodeEditorResources.styles.outputStyle;
        GUI.Label(rect, new GUIContent(port.name.PrettifyCamelCase()), style);

        Vector2 handlePoint = rect.center;

        switch (port.direction) {
            case NodePort.IO.Input: handlePoint.x = rect.xMin; break;
            case NodePort.IO.Output: handlePoint.x = rect.xMax; break;
        }
        return handlePoint;
    }

    private static FieldInfo[] GetInspectorFields(Node node) {
        return node.GetType().GetFields().Where(f => f.IsPublic || f.GetCustomAttributes(typeof(SerializeField),false) != null).ToArray();
    }

    private void DrawFieldInfoDrawerGUI(FieldInfo fieldInfo) {
        Type fieldType = fieldInfo.FieldType;
        string fieldName = fieldInfo.Name;
        string fieldPrettyName = fieldName.PrettifyCamelCase();
        object fieldValue = fieldInfo.GetValue(target);
        object[] fieldAttribs = fieldInfo.GetCustomAttributes(false);

        HeaderAttribute headerAttrib;
        if (NodeEditorUtilities.GetAttrib(fieldAttribs, out headerAttrib)) {
            EditorGUILayout.LabelField(headerAttrib.header);
        }

        EditorGUI.BeginChangeCheck();
        if (fieldType == typeof(int)) {
            fieldValue = EditorGUILayout.IntField(fieldPrettyName, (int)fieldValue);
        }
        else if (fieldType == typeof(bool)) {
            fieldValue = EditorGUILayout.Toggle(fieldPrettyName, (bool)fieldValue);
        }
        else if (fieldType.IsEnum) {
            fieldValue = EditorGUILayout.EnumPopup(fieldPrettyName, (Enum)fieldValue);
        }
        else if (fieldType == typeof(string)) {

            if (fieldName == "name") return; //Ignore 'name'
            TextAreaAttribute textAreaAttrib;
            if (NodeEditorUtilities.GetAttrib(fieldAttribs, out textAreaAttrib)) {
                fieldValue = EditorGUILayout.TextArea(fieldValue != null ? (string)fieldValue : "");
            }
            else
                fieldValue = EditorGUILayout.TextField(fieldPrettyName, fieldValue != null ? (string)fieldValue : "");
        }
        else if (fieldType == typeof(Rect)) {
            if (fieldName == "position") return; //Ignore 'position'
            fieldValue = EditorGUILayout.RectField(fieldPrettyName, (Rect)fieldValue);
        }
        else if (fieldType == typeof(float)) {
            fieldValue = EditorGUILayout.FloatField(fieldPrettyName, (float)fieldValue);
        }
        else if (fieldType == typeof(Vector2)) {
            fieldValue = EditorGUILayout.Vector2Field(fieldPrettyName, (Vector2)fieldValue);
        }
        else if (fieldType == typeof(Vector3)) {
            fieldValue = EditorGUILayout.Vector3Field(new GUIContent(fieldPrettyName), (Vector3)fieldValue);
        }
        else if (fieldType == typeof(Vector4)) {
            fieldValue = EditorGUILayout.Vector4Field(fieldPrettyName, (Vector4)fieldValue);
        }
        else if (fieldType == typeof(Color)) {
            fieldValue = EditorGUILayout.ColorField(fieldPrettyName, (Color)fieldValue);
        }
        else if (fieldType == typeof(AnimationCurve)) {
            AnimationCurve curve = fieldValue != null ? (AnimationCurve)fieldValue : new AnimationCurve();
            curve = EditorGUILayout.CurveField(fieldPrettyName, curve);
            if (fieldValue != curve) fieldInfo.SetValue(target, curve);
        }
        else if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)) || fieldType == typeof(UnityEngine.Object)) {
            if (fieldName == "graph") return; //Ignore 'graph'
            fieldValue = EditorGUILayout.ObjectField(fieldPrettyName, (UnityEngine.Object)fieldValue, fieldType, true);
        }

        if (EditorGUI.EndChangeCheck()) {
            fieldInfo.SetValue(target, fieldValue);
        }
    }

    public virtual int GetWidth() {
        return 200;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class CustomNodeEditorAttribute : Attribute {
    public Type inspectedType { get { return _inspectedType; } }
    private Type _inspectedType;
    public string contextMenuName { get { return _contextMenuName; } }
    private string _contextMenuName;
    /// <summary> Tells a NodeEditor which Node type it is an editor for </summary>
    /// <param name="inspectedType">Type that this editor can edit</param>
    /// <param name="contextMenuName">Path to the node</param>
    public CustomNodeEditorAttribute(Type inspectedType, string contextMenuName) {
        _inspectedType = inspectedType;
        _contextMenuName = contextMenuName;
    }
}

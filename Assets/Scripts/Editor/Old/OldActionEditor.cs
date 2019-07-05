using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Editor;
using UnityEditor;
using UnityEngine;

public class OldActionEditor : EditorWindow
{
    private List<Editor.Old.Box> _boxes;

    private GUIStyle _boxStyle;
    private GUIStyle _selectedBoxStyle;

    private Vector2 _offset;
    private Vector2 _drag;
    
    [MenuItem("Window/Action Builder")]
    private static void OpenWindow()
    {
        var window = GetWindow<OldActionEditor>();
        window.titleContent = new GUIContent("Action Builder");
    }

    private void OnEnable()
    {
        _boxStyle = new GUIStyle
        {
            normal = {background = BoxTextures.MakeBoxTexture(Color.green, 5) },//EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D},
            border = new RectOffset(12, 12, 12, 12)
        };

        _selectedBoxStyle = new GUIStyle
        {
            normal =  {background = _boxStyle.normal.background.Tint(Color.black, 50)},//EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D},
            border = new RectOffset(12, 12, 12, 12)
        };
    }

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        
        DrawBoxes();

        ProcessBoxEvents(Event.current);
        ProcessEvents(Event.current);
        
        if (GUI.changed) Repaint();

    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        var widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        var heightDivs = Mathf.CeilToInt(position.height / gridSpacing);
 
        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
 
        _offset += _drag * 0.5f;
        var newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);
 
        for (var i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }
 
        for (var j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }
 
        Handles.color = Color.white;
        Handles.EndGUI();
    }
    
    private void DrawBoxes()
    {
        _boxes?.ForEach(x => x.Draw());
    }

    private void ProcessBoxEvents(Event e)
    {
        if (_boxes == null) return;
        
        for (var i = _boxes.Count - 1; i >= 0; --i)
        {
            var guiChanged = _boxes[i].ProcessEvents(e);
            
            if (guiChanged)
            {
                GUI.changed = true;
            }
        }
    }
    
    private void ProcessEvents(Event e)
    {
        _drag = Vector2.zero;
        
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }

                break;
            
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }

                break;
        }
    }

    private void OnDrag(Vector2 delta)
    {
        _drag = delta;
        _boxes?.ForEach(x => x.Drag(delta));
        GUI.changed = true;
    }
    
    private void ProcessContextMenu(Vector2 MousePosition)
    {
       GenericMenu genericMenu = new GenericMenu();
       genericMenu.AddItem(new GUIContent("Add box"), false, () => OnClickAddNode(MousePosition));
       genericMenu.ShowAsContext();
    }

    private void OnClickAddNode(Vector2 mousePosition)
    {
        if (_boxes is null)
            _boxes = new List<Editor.Old.Box>();
        
        _boxes.Add(new Editor.Old.Box(mousePosition, 200, 200, _boxStyle, _selectedBoxStyle, OnClickRemoveBox));
    }

    private void OnClickRemoveBox(Editor.Old.Box box)
    {
        _boxes.Remove(box);
    }
    
}

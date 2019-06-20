using System;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Editor
{
    public class Box
    {
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }
        
        public Rect Rect;
        public string Title;
        public bool IsDragged;
        public bool IsSelected;
        public bool Resizing;
        public Direction ResizeDir;

        public GUIStyle Style;
        public GUIStyle DefaultBoxStyle;
        public GUIStyle SelectedBoxStyle;

        public Action<Box> OnRemoveBox;
        
        public Box(Vector3 position, float width, float height, GUIStyle boxStyle, GUIStyle selectedBoxStyle, Action<Box> onClickRemoveBox)
        {
            Rect = new Rect(position.x, position.y, width, height);
            Style = boxStyle;
            DefaultBoxStyle = boxStyle;
            SelectedBoxStyle = selectedBoxStyle;
            OnRemoveBox = onClickRemoveBox;
        }

        public void Drag(Vector2 delta)
        {
            Rect.position += delta;
        }

        public void Draw()
        {
            GUI.Box(Rect, Title, Style);
        }
        
        private void Resize(Vector2 mousePos)
        {
            switch (ResizeDir)
            {
                case Direction.Left:
                    Rect.xMin = mousePos.x;
                    break;
                case Direction.Right:
                    Rect.xMax = mousePos.x;
                    break;
                case Direction.Up:
                    Rect.yMin = mousePos.y;
                    break;
                case Direction.Down:
                    Rect.yMin = mousePos.y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool ShouldResize(Vector2 mousePos)
        {
            bool IsWithinXOf(float num, float range, float value)
            {
                return num > value - range && num < value + range;
            }
            
            if (IsWithinXOf(mousePos.x, 25, Rect.xMax))
            {
                ResizeDir = Direction.Right;
                return true;
            }

            if (IsWithinXOf(mousePos.x, 25, Rect.xMin))
            {
                ResizeDir = Direction.Left;
                return true;
            }

            if (IsWithinXOf(mousePos.y, 25, Rect.yMax))
            {
                ResizeDir = Direction.Down;
                return true;
            }

            if (IsWithinXOf(mousePos.y, 25, Rect.yMin))
            {
                ResizeDir = Direction.Up;
                return true;
            }

            return false;
        }
        
        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (Rect.Contains(e.mousePosition))
                        {
                            if (ShouldResize(e.mousePosition))
                            {
                                Resizing = true;
                            }
                            else
                            {
                                IsDragged = true;
                                IsSelected = true;
                                
                                Style = SelectedBoxStyle;
                            }
                            GUI.changed = true;
                        }
                        else
                        {
                            GUI.changed = true;
                            IsSelected = false;
                            Style = DefaultBoxStyle;
                        }
                    }

                    if (e.button == 1 && IsSelected && Rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    
                    break;
                
                case EventType.MouseUp:
                    IsDragged = false;
                    Resizing = false;
                    break;
                
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        if (IsDragged)
                        {
                            Drag(e.delta);
                        }
                        else if (Resizing)
                            Resize(e.mousePosition);
                        e.Use();
                        return true;
                    }

                    break;
            }
            
            return false;
        }

        private void ProcessContextMenu()
        {
            var genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove box"), false, OnClickRemoveBox);
            genericMenu.ShowAsContext();
        }

        private void OnClickRemoveBox()
        {
            OnRemoveBox?.Invoke(this);
        }
        
        
    }
}
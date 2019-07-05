using System;
using UnityEditor;
using UnityEngine;

namespace Editor.Old
{
    public class BoxInfo
    {
        public string Name;
        public Color Color;

        public BoxInfo(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }
    
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
        private bool _isDragged;
        private bool _isSelected;
        private bool _resizing;
        private Direction _resizeDir;

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
            switch (_resizeDir)
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
            if (mousePos.x.IsWithinXOf(25, Rect.xMax))
            {
                _resizeDir = Direction.Right;
                return true;
            }

            if (mousePos.x.IsWithinXOf(25, Rect.xMin))
            {
                _resizeDir = Direction.Left;
                return true;
            }

            if (mousePos.y.IsWithinXOf(25, Rect.yMax))
            {
                _resizeDir = Direction.Down;
                return true;
            }

            if (mousePos.y.IsWithinXOf(25, Rect.yMin))
            {
                _resizeDir = Direction.Up;
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
                                _resizing = true;
                            }
                            else
                            {
                                _isDragged = true;
                                _isSelected = true;
                                
                                Style = SelectedBoxStyle;
                            }
                            GUI.changed = true;
                        }
                        else
                        {
                            GUI.changed = true;
                            _isSelected = false;
                            Style = DefaultBoxStyle;
                        }
                    }

                    if (e.button == 1 && _isSelected && Rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    
                    break;
                
                case EventType.MouseUp:
                    _isDragged = false;
                    _resizing = false;
                    break;
                
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        if (_isDragged)
                        {
                            Drag(e.delta);
                            e.Use();
                        }
                        else if (_resizing)
                        {
                            Resize(e.mousePosition);
                            e.Use();
                        }

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
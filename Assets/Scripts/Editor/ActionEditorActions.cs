using System;
using UnityEngine;

namespace Editor
{
    public partial class ActionEditorWindow
    {
        private const int LeftMouse = 0;
        private const int RightMouse = 1;
        
        private void ProcessEvents()
        {
            var e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:
                    break;
                case EventType.MouseUp:
                    break;
                case EventType.MouseMove:
                    break;
                case EventType.MouseDrag:
                    if (e.button == RightMouse)
                    {
                        _panOffset += e.delta * _zoom;
                        GUI.changed = true;
                    }

                    break;
                case EventType.KeyDown:
                    break;
                case EventType.KeyUp:
                    break;
                case EventType.ScrollWheel:
                    var oldZoom = _zoom;
                    if (e.delta.y > 0) _zoom += 0.1f * _zoom;
                    else _zoom -= 0.1f * _zoom;
                    if (ActionEditorPreferences.Settings.zoomToMouse) 
                        _panOffset += (1 - oldZoom / _zoom) * (WindowToGridPosition(e.mousePosition) + _panOffset);
                    GUI.changed = true;
                    break;
                case EventType.Repaint:
                    break;
                case EventType.Layout:
                    break;
                case EventType.DragUpdated:
                    break;
                case EventType.DragPerform:
                    break;
                case EventType.DragExited:
                    break;
                case EventType.Ignore:
                    break;
                case EventType.Used:
                    break;
                case EventType.ValidateCommand:
                    break;
                case EventType.ExecuteCommand:
                    break;
                case EventType.ContextClick:
                    break;
                case EventType.MouseEnterWindow:
                    break;
                case EventType.MouseLeaveWindow:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public partial class ActionEditorWindow : EditorWindow
    {
        private Action _currentAction;

        private float _zoom = 1.0f;
        private Vector2 _panOffset;
        
        [MenuItem("Window/Action Builder")]
        private static void OpenWindow()
        {
            var window = GetWindow<ActionEditorWindow>();
            window.titleContent = new GUIContent("Action Builder");
        }

        private void OnSelectionChange()
        {
            var selection = Selection.GetFiltered<Action>(SelectionMode.Assets)[0];
            if (selection)
                OpenAction(selection);
        }

        private void OpenAction(Action action)
        {
            _currentAction = action;
            
        }

        private Vector2 WindowToGridPosition(Vector2 windowPosition) {
            return (windowPosition - position.size * 0.5f - _panOffset / _zoom) * _zoom;
        }
    }
}
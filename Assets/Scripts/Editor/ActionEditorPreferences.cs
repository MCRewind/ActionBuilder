using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ActionEditorPreferences
    {

        public static ActionEditorSettings Settings = new ActionEditorSettings();
        
        [Serializable]
        public class ActionEditorSettings : ISerializationCallbackReceiver
        {
            public bool zoomToMouse = true;
            
            [SerializeField] private Color32 _gridLineColor = new Color(0.45f, 0.45f, 0.45f);

            public Color32 gridLineColor
            {
                get => _gridLineColor;
                set
                {
                    _gridLineColor = value;
                    _gridTexture = null;
                    _crossTexture = null;
                }
            }
            
            [SerializeField] private Color32 _gridBgColor = new Color(0.18f, 0.18f, 0.18f);

            public Color32 gridBgColor
            {
                get => _gridBgColor;
                set
                {
                    _gridBgColor = value;
                    _gridTexture = null;
                }
            }
            
            private Texture2D _gridTexture;

            public Texture2D gridTexture
            {
                get
                {
                    if (_gridTexture == null)
                        _gridTexture = ActionEditorResources.GenerateGridTexture(gridLineColor, gridBgColor);
                    return _gridTexture;
                }
            }

            private Texture2D _crossTexture;

            public Texture2D crossTexture
            {
                get
                {
                    if (_crossTexture == null)
                        _crossTexture = ActionEditorResources.GenerateCrossTexture(gridLineColor);
                    return _crossTexture;
                }
            }

            public void OnBeforeSerialize()
            {
                throw new NotImplementedException();
            }

            public void OnAfterDeserialize()
            {
                throw new NotImplementedException();
            }
        }
        
#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider CreateActionBuilderSettingsProvider() {
            var provider = new SettingsProvider("Preferences/Action Builder", SettingsScope.User) {
                guiHandler = searchContext => { ActionEditorPreferences.PreferencesGUI(); },
                keywords = new HashSet<string>(new [] { "action builder", "action", "builder", "box"})
            };
            return provider;
        }
#endif

#if !UNITY_2019_1_OR_NEWER
        [PreferenceItem("Node Editor")]
#endif
        private static void PreferencesGUI() {
           
        }
    }
}
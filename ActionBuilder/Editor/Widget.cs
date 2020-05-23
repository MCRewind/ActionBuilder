using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace ActionBuilder.Editor
{
    public abstract class Widget
    {
        private bool _begun;
        
        protected ImGuiWindowFlags Flags = ImGuiWindowFlags.NoCollapse;

        protected GraphicsDevice GD;
        
        protected int NumStyleVarPushes;
        
        public bool IsVisible = true;
        
        public bool IsWindow { get; protected set; } = true;
        public abstract string Title { get; }
        public Dictionary<string, Widget> RequiredWidgets { get; protected set; } = null;
        public Vector2 Size { get; protected set; } = new Vector2(-1.0f, -1.0f);
        public Vector2 Position { get; protected set; } = new Vector2(-1.0f, -1.0f);
        
        public Action OnStart { get; protected set; }
        
        public Widget(GraphicsDevice gd)
        {
            GD = gd;
        }
        
        public bool Begin()
        {
            OnStart?.Invoke();
            
            if (!IsWindow)
                return true;

            if (!IsVisible)
                return false;

            // If pos is not (-1, -1)
            if (Math.Abs(Position.X + 1.0f) > float.Epsilon &&
                Math.Abs(Position.Y + 1.0f) > float.Epsilon)
            {
                ImGui.SetNextWindowPos(Position);
            }
            
            // If size is not (-1, -1)
            if (Math.Abs(Size.X + 1.0f) > float.Epsilon &&
                Math.Abs(Size.Y + 1.0f) > float.Epsilon)
            {
                ImGui.SetNextWindowSize(Size);
            } 
            
            if (ImGui.Begin(Title, ref IsVisible, Flags))
            {
                _begun = true;
            }

            return _begun;
        }

        public bool End()
        {
            if (_begun)
            {
                ImGui.End();
            }

            ImGui.PopStyleVar(NumStyleVarPushes);
            NumStyleVarPushes = 0;
            
            _begun = false;
            
            return true;
        }

        public abstract void Tick();

    }
}
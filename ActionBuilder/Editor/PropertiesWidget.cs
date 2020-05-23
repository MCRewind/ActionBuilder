using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace ActionBuilder.Editor
{
    public class PropertiesWidget : Widget
    {
        public override string Title => "Properties";
        
        public PropertiesWidget(GraphicsDevice gd) : base(gd)
        {
            RequiredWidgets = new Dictionary<string, Widget>
            {
                ["Toolbar"] = null,
                ["MenuBar"] = null,
            };
            
            Flags = 
                ImGuiWindowFlags.NoCollapse      |
                ImGuiWindowFlags.NoTitleBar      |
                ImGuiWindowFlags.NoMove          |
                ImGuiWindowFlags.NoResize        |
                ImGuiWindowFlags.NoScrollbar     |
                ImGuiWindowFlags.NoSavedSettings;
            
            OnStart += () =>
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
                NumStyleVarPushes = 1;
            };
        }
 
        public override void Tick()
        {
            Position = new Vector2(0, RequiredWidgets["Toolbar"].Position.Y + RequiredWidgets["Toolbar"].Size.Y + 6f);
            Size = new Vector2(100, GD.Viewport.Height - Position.Y);
            
            ImGui.Spacing();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - ImGui.CalcTextSize("Properties").X / 2);
            ImGui.Text("PROPERTIES");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace ActionBuilder.Editor
{
    public class ToolbarWidget : Widget
    {
        public override string Title => "Toolbar";

        public ToolbarWidget(GraphicsDevice gd) : base(gd)
        {
            RequiredWidgets = new Dictionary<string, Widget>
            {
                ["MenuBar"] = null
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
            Position = new Vector2(0, RequiredWidgets["MenuBar"].Size.Y);
            Size = new Vector2(GD.Viewport.Width, 25); 
        }
    }
}
using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace ActionBuilder.Editor
{
    public class MenuBarWidget : Widget
    {
        public override string Title => "MenuBar";

        public MenuBarWidget(GraphicsDevice gd) : base(gd)
        {
            IsWindow = false;
            Flags = ImGuiWindowFlags.MenuBar;
        }

        public override void Tick()
        {
            if (ImGui.BeginMainMenuBar())
            {
                Size = ImGui.GetWindowSize();
                Position = ImGui.GetWindowPos();
                
                if (ImGui.BeginMenu("File"))
                {
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    ImGui.EndMenu();
                }
                
                ImGui.EndMainMenuBar();
            }
        }
    }
}
using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public partial class ActionEditorWindow
    {
        private void OnGUI()
        {
            ProcessEvents();
            DrawGrid(position, zoom, panOffset);
            GUI.changed = true;
        }

        private void DrawGrid(Rect rect, float zoom, Vector2 panOffset) {

            rect.position = Vector2.zero;

            var center = rect.size / 2f;
            var gridTex = ActionEditorPreferences.Settings.gridTexture;
            var crossTex = ActionEditorPreferences.Settings.crossTexture;

            // Offset from origin in tile units
            var xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
            var yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

            var tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            var tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
            var tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

            var tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
        }
    }
}
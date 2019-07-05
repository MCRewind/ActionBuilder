using UnityEngine;

namespace Editor
{
    public static class ActionEditorResources
    {
        public static Texture2D GenerateGridTexture(Color line, Color bg)
        {
            var tex = new Texture2D(64, 64);
            var cols = new Color[64 * 64];
            for (var y = 0; y < 64; y++) {
                for (var x = 0; x < 64; x++) {
                    var col = bg;
                    if (y % 16 == 0 || x % 16 == 0) col = Color.Lerp(line, bg, 0.65f);
                    if (y == 63 || x == 63) col = Color.Lerp(line, bg, 0.35f);
                    cols[y * 64 + x] = col;
                }
            }
            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateCrossTexture(Color line)
        {
            var tex = new Texture2D(64, 64);
            var cols = new Color[64 * 64];
            for (var y = 0; y < 64; y++) {
                for (var x = 0; x < 64; x++) {
                    var col = line;
                    if (y != 31 && x != 31) col.a = 0;
                    cols[y * 64 + x] = col;
                }
            }
            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }
    }
}
using UnityEngine;

namespace Editor
{
    public static class BoxTextures
    {
        public static Texture2D MakeBoxTexture(Color color, int radius)
        {
            var tex = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            
            var color1 = new Color(color.r, color.g, color.b, .5f);
            var color2 = new Color(color.r / 2, color.g / 2, color.b / 2, .5f);
            
            for (var i = 0; i < tex.width; i++)
            {
                for (var j = 0; j < tex.height; j++)
                {
                    if (i.IsWithinXOf(radius, 0) || j.IsWithinXOf(radius, 0) || i.IsWithinXOf(radius, tex.width - 1) || j.IsWithinXOf(radius, tex.height - 1))
                        tex.SetPixel(i, j, color2);
                    else 
                        tex.SetPixel(i, j, color1);
                }
            }

            tex.Apply();
            
            return tex;
        }
    }
}
using UnityEngine;

namespace Editor
{
    public static class Texture2DExtensions
    {
        public static Texture2D Tint(this Texture2D texture, Color tint, int percent)
        {
            var pixels = texture.GetPixels32();
            
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = (byte)(pixels[i].r * (int)tint.r / 255);
                pixels[i].g = (byte)(pixels[i].g * (int)tint.g / 255);
                pixels[i].b = (byte)(pixels[i].b * (int)tint.b / 255);
            }
            
            texture.SetPixels32(pixels);
            texture.Apply();
            
            return texture;
        }
    }
}
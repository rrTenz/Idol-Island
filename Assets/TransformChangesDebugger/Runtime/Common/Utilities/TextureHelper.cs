using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.Utilities
{
    public class TextureHelper
    {
        public static Texture2D CreateTexture( int width, int height, Color color)
        {
            var pix = new Color[width * height];
            for(var i = 0; i < pix.Length; ++i)
            {
                pix[i] = color;
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static Texture2D CreateGuiBackgroundColor(Color color) => CreateTexture(2, 2, color);
    }
}
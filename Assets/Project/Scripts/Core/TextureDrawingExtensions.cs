using UnityEngine;


public static class TextureDrawingExtensions
{
    
    public static void DrawFilledRectangle(this Color[] pixels, RectInt rect, Color color, int textureWidth, int textureHeight)
    {
        for (int y = rect.y; y < rect.y + rect.height; y++)
        {
            for (int x = rect.x; x < rect.x + rect.width; x++)
            {
                
                if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
                {
                    pixels[y * textureWidth + x] = color;
                }
            }
        }
    }

    
    public static void DrawOutlineRectangle(this Color[] pixels, RectInt rect, Color color, int thickness, int textureWidth, int textureHeight)
    {
        
        pixels.DrawFilledRectangle(new RectInt(rect.x, rect.y, rect.width, thickness), color, textureWidth, textureHeight);
        
        pixels.DrawFilledRectangle(new RectInt(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color, textureWidth, textureHeight);
        
        pixels.DrawFilledRectangle(new RectInt(rect.x, rect.y, thickness, rect.height), color, textureWidth, textureHeight);
        
        pixels.DrawFilledRectangle(new RectInt(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color, textureWidth, textureHeight);
    }
}

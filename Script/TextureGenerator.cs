using System.Collections;
using UnityEngine;

public static class TextureGenerator  {
    public static Texture2D TextureFromColourMap(Color[,] colourMap , int length)
    {
        Color[] colorMap = new Color[length * length];
        for(int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                colorMap[y * length + x] = colourMap[x, y];
            }
        }
        Texture2D texture = new Texture2D(length, length);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
}

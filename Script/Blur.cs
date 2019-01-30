using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blur : MonoBehaviour
{
    static private float _sum = 0;
    static private float blurPixelCount = 0;

     static public float[,] FastBlur(float[,] array, int radius, int iterations)
    {

        float[,] blurred = array;

        for (var i = 0; i < iterations; i++)
        {

            blurred = BlurImage(blurred, radius, true);
            blurred = BlurImage(blurred, radius, false);
        }

        return blurred;
    }

    static float[,] BlurImage(float[,] array, int blurSize, bool horizontal)
    {

        float[,] blurred = new float[array.GetLength(0), array.GetLength(1)];
        int _W = array.GetLength(0);
        int _H = array.GetLength(1);
        int xx, yy, x, y;

        if (horizontal)
        {

            for (yy = 0; yy < _H; yy++)
            {

                for (xx = 0; xx < _W; xx++)
                {

                    ResetPixel();

                    //Right side of pixel
                    for (x = xx; (x < xx + blurSize && x < _W) ; x++) {

                AddPixel(array[x, yy]);
            }

            //Left side of pixel
            for (x = xx; (x > xx - blurSize && x > 0) ; x--) {

                AddPixel(array[x, yy]);
            }

            CalcPixel();

            for (x = xx; x < xx + blurSize && x < _W; x++) {

                blurred[x, yy] = _sum;
            }
        }
    }
}
 
    else {
 
        for (xx = 0; xx<_W; xx++) {
 
            for (yy = 0; yy<_H; yy++) {
 
                ResetPixel();
 
                //Over pixel
                for (y = yy; (y<yy + blurSize && y<_H); y++) {
 
                    AddPixel(array[xx, y]);
                }
 
                //Under pixel
                for (y = yy; (y > yy - blurSize && y > 0); y--) {
 
                    AddPixel(array[xx, y]);
                }
 
                CalcPixel();
 
                for (y = yy; y<yy + blurSize && y<_H; y++) {
 
                    blurred[xx, y] = _sum;
                }
            }
        }
    }
    return blurred;
}

    static void AddPixel(float pixel)
{

    _sum += pixel;
    blurPixelCount++;
}

    static void ResetPixel()
{

    _sum = 0;
    blurPixelCount = 0;
}

    static void CalcPixel()
{
    _sum = _sum / blurPixelCount;
}

    /*
    private float[,] _sourceArray;
    private float _windowSize;
    private float _sum=0;

    public float[,] BlurArray(float[,] array, int radius, int iterations)
    {
        _windowSize = radius * 2 + 1;

        for (var i = 0; i < iterations; i++)
        {
            array = OneDimensialBlur(array, radius, true);
            array = OneDimensialBlur(array, radius, false);
        }

        return array;
    }

    private float[,] OneDimensialBlur(float[,] array, int radius, bool horizontal)
    {

        _sourceArray = array;

        float[,] blurred = new float[array.GetLength(0), array.GetLength(0)];

        if (horizontal)
        {
            for (int imgY = 0; imgY < array.GetLength(0); ++imgY)
            {
                ResetSum();

                for (int imgX = 0; imgX < array.GetLength(0); imgX++)
                {
                    if (imgX == 0)
                        for (int x = radius * -1; x <= radius; ++x)
                            AddValue(GetPixelWithXCheck(x, imgY));
                    else
                    {
                        var toExclude = GetPixelWithXCheck(imgX - radius - 1, imgY);
                        var toInclude = GetPixelWithXCheck(imgX + radius, imgY);

                        SubstValue(toExclude);
                        AddValue(toInclude);
                    }
                    blurred[imgX, imgY] = CalcPixelFromSum();
                }
            }
        }
        else
        {
            for (int imgX = 0; imgX < array.GetLength(0); imgX++)
            {
                ResetSum();

                for (int imgY = 0; imgY < array.GetLength(0); ++imgY)
                {
                    if (imgY == 0)
                        for (int y = radius * -1; y <= radius; ++y)
                            AddValue(GetPixelWithYCheck(imgX, y));
                    else
                    {
                        var toExclude = GetPixelWithYCheck(imgX, imgY - radius - 1);
                        var toInclude = GetPixelWithYCheck(radius, imgY + radius);

                        SubstValue(toExclude);
                        AddValue(toInclude);
                    }

                    blurred[imgX, imgY] = CalcPixelFromSum();
                }
            }
        }

        return blurred;
    }

    private float GetPixelWithXCheck(int x, int y)
    {
        if (x <= 0) return _sourceArray[0, y];
        if (x >= _sourceArray.GetLength(0)) return _sourceArray[_sourceArray.GetLength(0) - 1, y];
        return _sourceArray[x, y];
    }

    private float GetPixelWithYCheck(int x, int y)
    {

        if (y <= 0) return _sourceArray[x, 0];
        if (y >= _sourceArray.GetLength(0)) return _sourceArray[x, _sourceArray.GetLength(0) - 1];
        return _sourceArray[x, y];
    }

    private void AddValue(float value)
    {
        _sum += value;
    }

    private void SubstValue(float value)
    {
        _sum -= value;
    }

    private void ResetSum()
    {
        _sum = 0.0f;
    }

    float CalcPixelFromSum()
    {
        return _sum / _windowSize;
    }

}

    public class chunkData
{
    public float maxValueF;
    public float minValueF;
    public Texture2D textureToBlur;

    public chunkData(Texture2D texture)
    {
        maxValueF = float.MinValue;
        minValueF = float.MaxValue;
        textureToBlur = texture;
    }
    */
}
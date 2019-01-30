using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTest : MonoBehaviour {


    public int sizeT;
    public float rescaleNoise = 0;
    public float amplitude;
    public bool treePlacementMethod;
    public float bestValueOfRescaleIs;
    int MaxOfLastBest = -1;

    public Texture2D DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                    colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x,y]);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();

        return texture;
    }

    public void genrateDetails()
    {
        int maxBest =0;

        System.Random prng = new System.Random(0);

        float[,] noiseMap = new float[sizeT * 2, sizeT * 2];
        float[,] treeMap = new float[sizeT * 2, sizeT * 2];

        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);
        for (int y = 0; y < sizeT * 2; y++)
        {
            for (int x = 0; x < sizeT * 2; x++)
            {
                float sampleX = (x + offsetX) * rescaleNoise;
                float sampleY = (y + offsetY) * rescaleNoise;
                noiseMap[x, y] = Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
            }
        }

        if(treePlacementMethod)
            for (int yc = 0; yc < sizeT; yc++)
            {
                for (int xc = 0; xc < sizeT; xc++)
                {
                    double max = 0;
                    for (int yn = yc - 1; yn <= yc + 1; yn++)
                    {
                        for (int xn = xc - 1; xn <= xc + 1; xn++)
                        {
                            double e = noiseMap[xn + 50, yn + 50];
                            if (e > max) { max = e; }
                        }
                    }
                    if (noiseMap[xc, yc] == max)
                    {
                        maxBest++;
                        treeMap[xc, yc] = 1;
                    }
                    else
                    {
                        treeMap[xc, yc] = 0;
                    }
                }
            }

        if (MaxOfLastBest < maxBest && maxBest != sizeT*sizeT)
        {
            Debug.Log("rescale = " + rescaleNoise + " max Best = " + maxBest);
            bestValueOfRescaleIs = rescaleNoise;
            MaxOfLastBest = maxBest;
        }

        if(treePlacementMethod)
            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = DrawNoiseMap(treeMap);
        else
            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = DrawNoiseMap(noiseMap);

    }

    private void Update()
    {
        genrateDetails();
        //rescaleNoise += 0.1f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HeightMapGenerator
{
    [System.Serializable]
    public struct BiomeType
    {
        public string biomeName;
        public float height;
        public bool water;
        public Color biomeColorID;
        //public NoiseParameters noiseParameters;
        public Details[] details;
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float localHeight;
        public Color colour;
        public Details[] details;
    }

    [System.Serializable]
    public struct Details
    {

        public string name;
        public GameObject asset;
        public float density;
        public Vector2 rescale;
        public float heightAdjust;
        public DetailType type;

    }

    public struct noiseArrays
    {
        public string m_name;
        public float[,] m_noiseMap;

        public noiseArrays(string name, float[,] noiseMap)
        {
            m_name = name;
            m_noiseMap = noiseMap;
        }
    }

    public enum DetailType { Rock , Tree , Veget };

    public static float[,] GenerateArrayFromTexture(Texture2D texture)
    {
        float[,] fallofmap = new float[texture.width, texture.height];
        Color[] fallof = new Color[texture.width * texture.height];
        fallof = texture.GetPixels();
        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.width; j++)
            {
                fallofmap[i, j] = fallof[i * texture.width + j].grayscale;
            }
        }
        return fallofmap;
    }

    public static Color[,] GenerateBiomeArrayFromTexture(Texture2D texture)
    {
        Color[,] fallofmap = new Color[texture.width, texture.height];
        Color[] fallof = new Color[texture.width * texture.height];
        fallof = texture.GetPixels();
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.width; y++)
            {
                fallofmap[x, y] = fallof[y * texture.width + x];
            }
        }
        return fallofmap;
    }

    public static float[,] GenerateNoiseMap(NoiseParameters noiseParameters, int size)
    {

        int seed = noiseParameters.seed;
        float scale = noiseParameters.scale;
        int octaves = noiseParameters.octaves;
        float persistance = noiseParameters.persistance;
        float lacunarity = noiseParameters.lacunarity;
        float[,] noiseMap = new float[size, size];


        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = size / 2f;
        float halfHeight = size / 2f;


        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    /*public static float[,] GenerateBiomeOctaveArray(Color[,] biomesMap, BiomeType[] biomeType)
    {
        List<noiseArrays> generatedMaps = new List<noiseArrays>();
        int[] nameCorespondance = new int[biomeType.Length];
        int size = 1;

        generatedMaps.Add(new noiseArrays(biomeType[0].noiseParameters.name, GenerateNoiseMap(biomeType[0].noiseParameters, biomesMap.GetLength(0))));

        for (int j = 0; j < nameCorespondance.Length; j++)
        {
            int index = 0;

            foreach (noiseArrays noisArray in generatedMaps)
            {
                index++;
                if (noisArray.m_name == biomeType[j].noiseParameters.name)
                {
                    break;
                }
                else if (index == size)
                {
                    generatedMaps.Add(new noiseArrays(biomeType[j].noiseParameters.name, GenerateNoiseMap(biomeType[j].noiseParameters, biomesMap.GetLength(0))));
                    size++;
                    break;
                }
            }
        }

        float[,] generatedMap = new float[biomesMap.GetLength(0), biomesMap.GetLength(0)];

        for (int y = 0; y < biomesMap.GetLength(0); y++)
        {
            for (int x = 0; x < biomesMap.GetLength(0); x++)
            {
                float heightValue = 0;
                Color biomeMapPxColor = biomesMap[x, y];
                foreach (BiomeType biome in biomeType)
                {
                    if (biome.biomeColorID == biomeMapPxColor)
                    {
                        foreach (noiseArrays noisArray in generatedMaps)
                        {
                            if (biome.noiseParameters.name == noisArray.m_name)
                                heightValue = noisArray.m_noiseMap[x, y];
                        }
                    }
                }

                generatedMap[x, y] = heightValue;
            }
        }

        return generatedMap;
    }*/

    public static float[,] GenerateDefinitiveMap(Color[,] biomesMap, BiomeType[] biomeType)
    {
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        int size = biomesMap.GetLength(0);
        float[,] definitiveHeightMap = new float[size, size];
        //float[,] biomeOcatveArray = GenerateBiomeOctaveArray(biomesMap, biomeType);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseHeight;
                //noiseHeight = biomeOcatveArray[x, y];

                if (1 > maxNoiseHeight)
                {
                    maxNoiseHeight = 1;
                }
                else if (1 < minNoiseHeight)
                {
                    minNoiseHeight = 1;
                }

                definitiveHeightMap[x, y] = 1;
            }
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                definitiveHeightMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, definitiveHeightMap[x, y]);
            }
        }
        //AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        return definitiveHeightMap;
    }

}

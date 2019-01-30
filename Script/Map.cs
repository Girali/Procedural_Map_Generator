using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class Map : MonoBehaviour {

    public Texture2D biomesMapTexture;

    public AnimationCurve generalHeightCurve;
    public HeightMapGenerator.BiomeType[] Biomes;
    public Renderer textureRender;
    public float heightMultiplyer;
    public float intansity;

    float[,] biomesHeightsMap;
    float[,] generalHeightMap;
    float[,] riverMap;
    Color[,] biomesMap;
    int chunkSize = 241;

    /*
    private void DrawNoiseMap(float[,] noisMap)
    {
        int width = noisMap.GetLength(0);
        int height = noisMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.white, Color.black, noisMap[x, y]);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();

        File.WriteAllBytes(EditorUtility.OpenFilePanel("Save it", "C:/Users/Girali/Desktop","") + "texture" + ".png", texture.EncodeToPNG());
    }*/

    private void GenerateBiomesHeights()
    {
        Color[,] biomesMap = HeightMapGenerator.GenerateBiomeArrayFromTexture(biomesMapTexture);
        float[,] definitiveMap = HeightMapGenerator.GenerateDefinitiveMap(biomesMap, Biomes);
        //DrawNoiseMap(definitiveMap);
    }

    public static float[,] GenerateChunkMap(float[,] map, Vector2 center, int size)
    {
        float[,] chunk = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                chunk[i, j] = map[((int)center.x) + i, ((int)center.y) + j];
            }
        }
        return chunk;
    }

    public static Color[,] GenerateChunkColorMap(Color[,] map, Vector2 center, int size)
    {
        Color[,] chunk = new Color[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                chunk[x, y] = map[((int)center.x) + x, ((int)center.y) + y];
            }
        }
        return chunk;
    }


    public void DrawMesh(MeshData meshData , MeshFilter meshFilter)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
    }

    //FILE SAV LIST ::::
    //      - GeneralHeights
    //      - BiomesHeights
    //      - Rivers

    float[,] RoundIt(float[,] array)
    {
        for (int y = 0; y < array.GetLength(0); y++)
            for (int x = 0; x < array.GetLength(0); x++)
            {

                array[x,y] = (float)System.Math.Round(array[x,y], 2);
            }
        return array;
    }

    static public float rigenoiseOctaves(float[,] rivers, int nx, int ny , float octaves)
    {
        float eSum = 0;
        for (int currentOctave = 0; currentOctave < octaves; currentOctave++)
        {

            if (currentOctave == 0)
                eSum += 1f * ridgenoiseF(rivers,1 * nx, 1 * ny);
            else
                eSum += (1f / currentOctave * 2f) * ridgenoiseF(rivers,(currentOctave * 2) * nx, (currentOctave * 2) * ny) * eSum;
        }
        return eSum;
    }

    static public float ridgenoiseF(float[,] rivers,int nx,int ny)
    {
        if (nx >= rivers.GetLength(0)-1 || ny >= rivers.GetLength(0)-1)
            return 0;
        else
            return 2f * (0.5f - Mathf.Abs((0.5f - (float)rivers[nx, ny])));
    }



    static public float rigenoiseOctavesFromNoise(float nx, float ny, float octaves)
    {
        float eSum = 0;
        for (int currentOctave = 0; currentOctave < octaves; currentOctave++)
        {
            if (currentOctave == 0)
                eSum += 1f * ridgenoise(1f * nx, 1f * ny);
            else
                eSum += (1f / currentOctave * 2f) * ridgenoise((currentOctave * 2f) * nx, (currentOctave * 2f) * ny) * eSum;
        }
        return eSum;
    }

    static public float ridgenoise(float nx, float ny)
    {
        return 2f * (0.5f - Mathf.Abs((0.5f - (float)Mathf.PerlinNoise(nx, ny))));
    }



    private void Start()
    {
        //Texture2D riversMap = new Texture2D(biomesMap.GetLength(0), biomesMap.GetLength(0));
        float[,] rivers = new float[biomesMapTexture.width, biomesMapTexture.width];
        float[,] generalHeights = new float[biomesMapTexture.width, biomesMapTexture.width];
        biomesMap = HeightMapGenerator.GenerateBiomeArrayFromTexture(biomesMapTexture);

        for(int y = 0; y < biomesMapTexture.width; y++)
        {
            for (int x = 0; x < biomesMapTexture.width; x++)
            {
                foreach (HeightMapGenerator.BiomeType biome in Biomes)
                {
                    if (biomesMap[x, y] == biome.biomeColorID)
                    {
                        if (biome.water)
                        {
                            rivers[x, y] = 0;
                        }
                        else
                        {
                            rivers[x, y] = biome.height;
                        }
                    }
                }
            }
        }

        //rivers = Blur.FastBlur(rivers, 2,45);

        //for (int y = 0; y < biomesMapTexture.width; y++)
        //{
        //    for (int x = 0; x < biomesMapTexture.width; x++)
        //    {
        //        rivers[x,y] = rigenoiseOctaves(rivers, x, y, 2);
        //    }
        //}
        System.Random prng = new System.Random(1);
        Vector2[] octaveOffsets = new Vector2[3];

        for (int i = 0; i < 3; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        for (int y = 0; y < biomesMapTexture.width; y++)
        {
            for (int x = 0; x < biomesMapTexture.width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                float perlinValue = 0;

                for (int i = 0; i < 3; i++)
                {
                    float sampleX = (x - biomesMapTexture.width / 2f) / 300f * frequency + octaveOffsets[i].x;
                    float sampleY = (y - biomesMapTexture.width / 2f) / 300f * frequency + octaveOffsets[i].y;
                    float e = rigenoiseOctavesFromNoise(sampleX, sampleY, 3);
                    perlinValue = Mathf.Pow(e, 3);


                    noiseHeight += perlinValue * amplitude;

                    amplitude *= 2.1f;
                    frequency *= 0.2f;

                    generalHeights[x, y] = noiseHeight * rivers[x, y];
                }
            }
        }

        DrawNoiseMap(generalHeights);

        
        biomesHeightsMap = SaveLoadManager.Load ("BiomesHeights");
        riverMap = SaveLoadManager.Load("Rivers");
        generalHeightMap = SaveLoadManager.Load("GeneralHeights");

        int addj = 0;
        for (int i = 1; i < (generalHeightMap.GetLength(0) / chunkSize); i++)
        {
            addj --;
            int addi = 0;
            for (int j = 1; j <( generalHeightMap .GetLength(0)/ chunkSize ); j++)
            {
                Vector2 offset = new Vector2(chunkSize * i, chunkSize * j);
                addi--;
                Vector2 yoffset = new Vector2((chunkSize * i)+ addj, (chunkSize * j)+addi);

                //  CHUNK SEPERATION
                float[,] chunckgeneralHeightMap = GenerateChunkMap(generalHeightMap, yoffset, chunkSize);
                float[,] chuncbiomesHeightsMap = GenerateChunkMap(biomesHeightsMap, yoffset, chunkSize);
                float[,] chunkRiverMap = GenerateChunkMap(riverMap, yoffset, chunkSize);
                Color[,] chuncbiomesMap = GenerateChunkColorMap(biomesMap, yoffset, chunkSize);

                GameObject terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
                terrain.transform.position = new Vector3(offset.x - i, 0, -offset.y + j);
                terrain.transform.localEulerAngles = new Vector3(0, 0, 0);
                terrain.SetActive(true);
                terrain.tag = "Chunk";
                Renderer renderer = terrain.GetComponent<Renderer>();
                Texture2D texture2D = TextureGenerator.TextureFromColourMap(chuncbiomesMap, chuncbiomesMap.GetLength(0));
                MeshFilter meshFilter = terrain.GetComponent<MeshFilter>();
                MeshData meshData = new MeshData(100,100);//MeshGenerator.GenerateTerrainMesh(chunkRiverMap, chuncbiomesMap, chuncbiomesHeightsMap, chunckgeneralHeightMap, Biomes, generalHeightCurve, heightMultiplyer, intansity);
                DrawMesh(meshData, meshFilter);
                renderer.material.mainTexture = texture2D;
                renderer.material.SetFloat("_Glossiness", 0f);
            }
        }
    }

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                    colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }


        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;
    }
}

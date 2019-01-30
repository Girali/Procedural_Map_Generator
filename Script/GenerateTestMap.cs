using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTestMap : MonoBehaviour {

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Texture2D biomesMapTexture;
    public Texture2D waterMat;
    public NoiseParameters noiseParameters;

    public Vector2 offset;
    public int rescaling;

    [Range(0, 10)]
    public float lacunarity;
    [Range(0, 5)]
    public float persistance;
    public float scale;
    [Range(0, 5)]
    public int octaves;
    public int seed;
    public int size;
    public bool autoUpdate;

    public bool blur;
    public int radius = 15;
    public int iternation = 2;

    public float rescaleNoise;
    public float amplitude;

    public bool rig = true;
    [Range(1, 6)]
    public int rigExponent = 2;
    [Range(1, 6)]
    public int rigOctaves = 2;

    public Map mapComponent;

    HeightMapGenerator.BiomeType[] biomes;

    public static MapData GenerateNoiseMap(Texture2D biomesMapTexture, HeightMapGenerator.BiomeType[] biomes, Vector2 offset, int size, int seed, float scale, int octaves, float persistance, float lacunarity, bool rig, int rigExponent, int rigOctaves, int radius, int iternation, bool blur)
    {
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
                float perlinValue = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    if (!rig)
                    {
                        perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    }
                    else
                    {
                        float e = rigenoiseOctaves(sampleX, sampleY, rigOctaves);
                        perlinValue = Mathf.Pow(e, rigExponent);
                    }

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

        if (blur)
            noiseMap = Blur.FastBlur(noiseMap, radius, iternation);

        float[,] generalHeights = new float[size, size];
        float[,] landMap = new float[size, size];
        float[,] rivers = new float[size, size];
        Color[,] biomesMap = HeightMapGenerator.GenerateBiomeArrayFromTexture(biomesMapTexture);
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        float minHeightr = float.MaxValue;
        float maxHeightr = float.MinValue;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                foreach (HeightMapGenerator.BiomeType biome in biomes)
                {
                    if (biomesMap[x, y] == biome.biomeColorID)
                    {
                        landMap[x, y] = biome.height;
                        if (biome.water)
                        {
                            rivers[x, y] = 1;
                            generalHeights[x, y] = 0;
                        }
                        else
                        {
                            rivers[x, y] = 0;
                            generalHeights[x, y] = biome.height;
                        }
                    }
                }
            }
        }

        float[,] riverMap = new float[rivers.GetLength(0), rivers.GetLength(1)];

        generalHeights = Blur.FastBlur(generalHeights, 3, 50);
        landMap = Blur.FastBlur(generalHeights, 2, 10);
        rivers = Blur.FastBlur(rivers, 3, 10);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                noiseMap[x, y] *= generalHeights[x, y] * 10f;
                riverMap[x, y] = (noiseMap[x, y] * 10f )* landMap[x, y] ;
                noiseMap[x, y] -= rivers[x, y] * 5f;
                //riverMap[x, y] -= rivers[x, y] * 10f;

                if (riverMap[x, y] < minHeightr)
                    minHeightr = riverMap[x, y];
                if (riverMap[x, y] > maxHeightr)
                    maxHeightr = riverMap[x, y];

                if (noiseMap[x, y] < minHeight)
                    minHeight = noiseMap[x, y];
                if (noiseMap[x, y] > maxHeight)
                    maxHeight = noiseMap[x, y];
            }
        }

        riverMap = Blur.FastBlur(riverMap,5, 25);



        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (rivers[x, y] <= 0.001f)
                    riverMap[x, y] = 0;
                noiseMap[x, y] = Mathf.Lerp(0, 10, (Mathf.Abs(noiseMap[x, y] - minHeight)) / (maxHeight - minHeight));
                riverMap[x, y] = Mathf.Lerp(0, 4.5f, (Mathf.Abs(riverMap[x, y] - minHeightr)) / (maxHeightr - minHeightr));
            }
        }

        noiseMap = Blur.FastBlur(noiseMap, 2, 5);

        MapData mapData = new MapData();
        mapData.water = riverMap;
        mapData.terrain = noiseMap;


        return mapData;
    }

    static float[,] waterArray(int size, Texture2D biomesMapTexture , HeightMapGenerator.BiomeType[] biomes)
    {

        Color[,] biomesMap = HeightMapGenerator.GenerateBiomeArrayFromTexture(biomesMapTexture);
        float[,] rivers = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                foreach (HeightMapGenerator.BiomeType biome in biomes)
                {
                    if (biomesMap[x, y] == biome.biomeColorID)
                    {
                        if (biome.water)
                        {
                            rivers[x, y] = 45;
                        }
                        else
                        {
                            rivers[x, y] = -1;
                        }
                    }
                }
            }
        }

        return rivers;
    }

    static float interpolateAbsolute(float a ,float b, float x , float y,float t)
    {
        float value = 0;
        if (x < 0)
        {
            value = Mathf.Lerp(a, b, (Mathf.Abs(t - x))/ (Mathf.Abs(x) + y));
        }
        else
        {
            value = Mathf.Lerp(a, b, (t-x) / (y-x));
        }

        return value;
    }

    static public float rigenoiseOctaves(float nx, float ny,float octaves)
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

    public Texture2D DrawNoiseMap(float[,] noiseMap,float min ,float max)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (noiseMap[x, y] <= 0)
                    colourMap[y * width + x] = Color.blue;
                else
                    colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.Lerp(0, 0.5f, (Mathf.Abs(noiseMap[x, y] - min)) / (max - min)));
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();

        return texture;
    }

    public float detHeight(Vector3 offset, int x, int y, float[,] heights)
    {
        if (x == heights.GetLength(0)-1 )
            x--;
        if (y == heights.GetLength(0)-1)
            y--;


        float value = 0;
        float rationy = Mathf.Abs(heights[x, y] - heights[x + 1, y]);
        float rationx = Mathf.Abs(heights[x, y] - heights[x , y + 1]);
        float interval = 0.1f;

        if (rationx > rationy)
            value = Mathf.Lerp(heights[x, y], heights[x, y + 1], -offset.z);

        else if (rationx < rationy)
            value = Mathf.Lerp(heights[x, y], heights[x + 1, y], offset.x);

        else if (rationx == rationy)
        {
            float t = (offset.x + -offset.z) / 2f;
            value = Mathf.Lerp(heights[x, y], heights[x +1, y+1], t);
        }

        return value;
    }

    public void genrateDetails(Color[,] biomesMap, HeightMapGenerator.BiomeType[] biomes , float[,] heightsMap ,Transform chunk)
    {
        int sizeT = biomesMap.GetLength(0);

        System.Random prng = new System.Random(0);

        float[,] noiseMap = new float[sizeT*2, sizeT * 2];

        foreach (HeightMapGenerator.BiomeType biome in biomes)
        {
            foreach (HeightMapGenerator.Details detail in biome.details)
            {

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

                for (int yc = 0; yc < sizeT; yc++)
                {
                    for (int xc = 0; xc < sizeT; xc++)
                    {
                        if (biome.biomeColorID == biomesMap[xc, yc])
                        {
                            double max = 0;

                            for (int yn = yc - (int)detail.density; yn <= yc + detail.density; yn++)
                            {
                                for (int xn = xc - (int)detail.density; xn <= xc + detail.density; xn++)
                                {

                                    double e = noiseMap[xn + 50, yn + 50];
                                    if (e > max) { max = e; }
                                }
                            }
                            if (noiseMap[xc, yc] == max)
                            {
                                if (detail.type == HeightMapGenerator.DetailType.Rock)
                                {
                                    GameObject g = Instantiate(detail.asset, chunk);
                                    Vector3 v3 = new Vector3(Random.Range(-1f, 1f), detail.heightAdjust, Random.Range(-1f, 1f));
                                    float s = Random.Range(detail.rescale.x, detail.rescale.x);

                                    g.transform.localPosition = new Vector3(v3.x + xc - sizeT / 2, detHeight(v3, xc, yc, heightsMap), v3.z + -yc + sizeT / 2);
                                    g.transform.parent = null;
                                    g.transform.eulerAngles = Random.insideUnitSphere * 360;
                                    g.transform.localScale = Vector3.one * s;
                                    g.transform.parent = chunk.parent;
                                    g.tag = "Chunk";
                                    g.SetActive(true);
                                }
                                if (detail.type == HeightMapGenerator.DetailType.Tree)
                                {
                                    GameObject g = Instantiate(detail.asset, chunk);
                                    Vector3 v3 = new Vector3(Random.Range(-1f, 1f), detail.heightAdjust, Random.Range(-1f, 1f));
                                    float s = Random.Range(detail.rescale.x, detail.rescale.x);
                                    g.transform.localPosition = new Vector3(v3.x + xc - sizeT / 2, detHeight(v3, xc, yc, heightsMap), v3.z + -yc + sizeT / 2);
                                    g.transform.parent = null;
                                    g.transform.eulerAngles = new Vector3(0,Random.Range(0, 1f) * 360,0);
                                    g.transform.localScale = Vector3.one * s;
                                    g.transform.parent = chunk.parent;
                                    g.tag = "Chunk";
                                    g.SetActive(true);
                                }
                                if (detail.type == HeightMapGenerator.DetailType.Veget)
                                {
                                    GameObject g = Instantiate(detail.asset, chunk);
                                    Vector3 v3 = new Vector3(Random.Range(-1f, 1f), detail.heightAdjust, Random.Range(-1f, 1f));
                                    float s = Random.Range(detail.rescale.x, detail.rescale.x);
                                    g.transform.localPosition = new Vector3(v3.x + xc - sizeT / 2, heightsMap[xc, yc] + v3.y, v3.z + -yc + sizeT / 2);
                                    g.transform.parent = null;
                                    g.transform.eulerAngles = new Vector3(0, Random.Range(0, 1f) * 360, 0);
                                    g.transform.localScale = Vector3.one * s;
                                    g.transform.parent = chunk.parent;
                                    g.tag = "Chunk";
                                    g.SetActive(true);
                                }
                            }
                        }
                    }
                }
            }
        }
        
    }


    public void GenerateTest()
    {
        GameObject[] chunks = GameObject.FindGameObjectsWithTag("Chunk");
        foreach (GameObject chunk in chunks)
            DestroyImmediate(chunk);

        biomes = mapComponent.Biomes;

        MapData noisMap = GenerateNoiseMap(biomesMapTexture, biomes, offset, biomesMapTexture.width, seed, scale, octaves, persistance, lacunarity, rig, rigExponent, rigOctaves, radius, iternation, blur);

        Color[,] biomesMap = HeightMapGenerator.GenerateBiomeArrayFromTexture(biomesMapTexture);

        float max = float.MinValue;
        float min = float.MaxValue;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (noisMap.terrain[x, y] < min)
                    min = noisMap.terrain[x, y];
                if (noisMap.terrain[x, y] > max)
                    max = noisMap.terrain[x, y];
            }
        }

        for (int i =0; i < (biomesMapTexture.width/ size ); i++)
        {
            for (int j = 0; j < ( biomesMapTexture.width/ size ); j++)
            {
                Vector2 offset = new Vector2(size * i - (i*4), size * j - (j*4));

                //  CHUNK SEPERATION
                float[,] chunckGeneralHeightMap = Map.GenerateChunkMap(noisMap.terrain, offset, size);
                float[,] waterChunk = Map.GenerateChunkMap(noisMap.water, offset, size);
                Texture2D chunkMat = TextureGenerator.TextureFromColourMap(Map.GenerateChunkColorMap(biomesMap, offset, size), size);
                GameObject terrain = new GameObject("bigChunk");
                terrain.transform.position = new Vector3(offset.x * rescaling, 0, -offset.y * rescaling);
                terrain.tag = "Chunk";

                Color[,] subBiomesMap = HeightMapGenerator.GenerateBiomeArrayFromTexture(chunkMat);
                

                for (int I = 0; I < 4; I++)
                {
                    for (int J = 0; J < 4; J++)
                    {
                        Vector2 offsets = new Vector2(size/4 * I-I, size/4 * J-J);

                        //  CHUNK SEPERATION
                        float[,] subChunckGeneralHeightMap = Map.GenerateChunkMap(chunckGeneralHeightMap, offsets, size/4);
                        float[,] subWaterChunk = Map.GenerateChunkMap(waterChunk, offsets, size/4);

                        Texture2D subChunkMat = TextureGenerator.TextureFromColourMap(Map.GenerateChunkColorMap(subBiomesMap, offsets, size/4), size/4);
                        Color[,] subColorBiomeMap = HeightMapGenerator.GenerateBiomeArrayFromTexture(subChunkMat);

                        GameObject subTerrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        subTerrain.transform.parent = terrain.transform;
                        subTerrain.GetComponent<MeshFilter>().sharedMesh = MeshGenerator.GenerateTerrainMesh(subChunckGeneralHeightMap).CreateMesh();
                        subTerrain.GetComponent<MeshRenderer>().material.mainTexture = subChunkMat;//DrawNoiseMap(chunckgeneralHeightMap,min , max);
                        subTerrain.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0f);
                        DestroyImmediate(subTerrain.GetComponent<MeshCollider>());
                        subTerrain.transform.localPosition = new Vector3(offsets.x * rescaling, 0, -offsets.y * rescaling);
                        subTerrain.transform.localScale = new Vector3(1*rescaling , 15*rescaling, 1 * rescaling);
                        subTerrain.name = "landChunk";
                        subTerrain.tag = "Chunk";
                        

                        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        water.transform.parent = terrain.transform;
                        water.GetComponent<MeshFilter>().sharedMesh = MeshGenerator.GenerateTerrainMesh(subWaterChunk).CreateMesh();
                        water.GetComponent<MeshRenderer>().material.mainTexture = waterMat;
                        water.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 1f);
                        DestroyImmediate(water.GetComponent<MeshCollider>());
                        water.transform.localPosition = new Vector3( offsets.x * rescaling, 47f* 16, -offsets.y * rescaling);
                        water.transform.localScale = new Vector3(1 * rescaling, 15 * rescaling, 1 * rescaling);
                        water.name = "waterChunk";
                        water.tag = "Chunk";

                        genrateDetails(subColorBiomeMap, biomes, subChunckGeneralHeightMap, subTerrain.transform);

                    }
                }

                



                /*
                terrain.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0f);
                DestroyImmediate(terrain.GetComponent<MeshCollider>());
                terrain.transform.position = new Vector3((offset.x - i) * rescaling, 0, (-offset.y + j )* rescaling);
                terrain.transform.localScale = new Vector3(1*rescaling, 15 * rescaling, 1 * rescaling);
                terrain.tag = "Chunk";

                GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
                water.GetComponent<MeshFilter>().sharedMesh = MeshGenerator.GenerateTerrainMesh(waterChunk).CreateMesh();
                water.GetComponent<MeshRenderer>().material.mainTexture = waterMat;

                water.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 1f);
                DestroyImmediate(water.GetComponent<MeshCollider>());
                water.transform.position = new Vector3((offset.x - i)* rescaling, 47.5f, (-offset.y + j)* rescaling);
                water.transform.localScale = new Vector3(1* rescaling, 15*rescaling, 1* rescaling);
                water.tag = "Chunk";*/
            }
        }

        
    }

    public struct MapData
    {
        public float[,] water;
        public float[,] terrain;

    };

    
    public void LoadOctave()
    {
        if (noiseParameters)
        {

            lacunarity = noiseParameters.lacunarity;
            persistance = noiseParameters.persistance    ;
            scale = noiseParameters.scale                ;
            octaves = noiseParameters.octaves            ;
            seed = noiseParameters.seed                  ;
            blur = noiseParameters.blur                  ;
            radius = noiseParameters.radius              ;
            iternation = noiseParameters.iternation      ;
            rig = noiseParameters.rig                    ;
            rigExponent = noiseParameters.rigExponent    ;
            rigOctaves = noiseParameters.rigOctaves      ;

            GenerateTest();
        }
    }

    public void SaveGenerated()
    {
        if (noiseParameters)
        {
            noiseParameters.lacunarity = lacunarity;
            noiseParameters.persistance  = persistance ;
            noiseParameters.scale        = scale       ;
            noiseParameters.octaves      = octaves     ;
            noiseParameters.seed         = seed        ;
            noiseParameters.blur         = blur        ;
            noiseParameters.radius       = radius      ;
            noiseParameters.iternation   = iternation  ;
            noiseParameters.rig          = rig         ;
            noiseParameters.rigExponent  = rigExponent ;
            noiseParameters.rigOctaves   = rigOctaves  ;
        }  
    }      
           
    
}

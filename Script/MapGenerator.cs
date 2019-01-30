using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

    public Texture2D texture;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int editiorMapDetail;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    float[,] fallofMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public TerrainType[] Regions;

    public void DrawMapEditor()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
    }

    public void RequestMapData(Vector2 Center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(Center ,callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 Center,Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(Center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void MeshDataThread(MapData mapData , int lod, Action<MeshData> callback)
    {
        //MeshData meshData = MeshGenerator.GnerateTerrainMesh();
        lock (meshDataThreadInfoQueue)
        {
            //meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    public void RequestMeshData(MapData mapData ,int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData,lod, callback);
        };
        new Thread(threadStart).Start();
    }

    private void Update()
    {

        if (mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i< mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parametre);
            }
        }

        if(meshDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parametre);
            }
        }
    }

    MapData GenerateMapData(Vector2 Center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, Center + offset ,normalizeMode);
        //float[,] chunk = GenerateFalllofChunk(fallofMap,Center,mapChunkSize);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for( int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                //noiseMap[x, y] = noiseMap[x, y]-chunk[x, y];

                float currentHeight = noiseMap[x, y];

                for(int i = 0; i< Regions.Length; i++)
                {
                    if(currentHeight >=Regions [i].height)
                    {
                        colorMap[y * mapChunkSize + x] = Regions[i].colour;
                    }
                    else
                    {
                        colorMap[y * mapChunkSize + x] = Regions[0].colour;
                        break;
                    }
                }

            }
        }

        return new MapData(noiseMap, colorMap);

    }

    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parametre;

        public MapThreadInfo(Action<T> callback, T parametre)
        {
            this.callback = callback;
            this.parametre = parametre;
        }
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float [,] heightMap , Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}
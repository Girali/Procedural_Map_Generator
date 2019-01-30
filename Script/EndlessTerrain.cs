using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

    const float scale = 1f;

    const float viwerMoveThreadHoldChunkUpdate = 25f;
    const float sqiwerMoveThreadHoldChunkUpdate = viwerMoveThreadHoldChunkUpdate * viwerMoveThreadHoldChunkUpdate;

    public LODInfo[] detailsLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material material;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunkDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        maxViewDst = detailsLevels[detailsLevels.Length - 1].visibleDistThreshHold;
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunkDistance = Mathf.RoundToInt(maxViewDst / chunkSize);

        UpdateVisibleChunk();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqiwerMoveThreadHoldChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunk();
        }
    }

    void UpdateVisibleChunk()
    {
        for (int i = 0; i < terrainChunkVisibleLastUpdate.Count; i++)
        {
            terrainChunkVisibleLastUpdate[i].SetVisible(false);
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkDistance; yOffset <= chunkDistance; yOffset++)
        {
            for (int xOffset = -chunkDistance; xOffset <= chunkDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize,detailsLevels, transform, material));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailsLevel;
        LODMesh[] lodMeshs;

        MapData mapData;
        bool mapDataRecived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size,LODInfo[] detailsLevel , Transform parent, Material material)
        {
            this.detailsLevel = detailsLevel;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);

            lodMeshs = new LODMesh[detailsLevel.Length];
            for(int i = 0; i<detailsLevel.Length; i++)
            {
                lodMeshs[i] = new LODMesh(detailsLevel[i].lod , UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position,OnMapDataRecived);
        }

        void OnMapDataRecived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataRecived = true;

            //Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize);
            //meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        void OnMeshDataRecived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataRecived)
            {
                float viewerDstDrontNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool isVisible = viewerDstDrontNearestEdge <= maxViewDst;

                if (isVisible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailsLevel.Length - 1; i++)
                    {
                        if (viewerDstDrontNearestEdge > detailsLevel[i].visibleDistThreshHold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshs[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    terrainChunkVisibleLastUpdate.Add(this);
                }

                SetVisible(isVisible);
            }
            }
            public void SetVisible(bool visible)
            {
                meshObject.SetActive(visible);
            }

            public bool IsVisile()
            {
                return meshObject.activeSelf;
            }
     
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallBack;

        public LODMesh(int lod ,System.Action updateCallBack)
        {
            this.lod = lod;
            this.updateCallBack = updateCallBack;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallBack();
        }

        public void RequestMesh( MapData mapData)
        {
            hasRequestMesh = true;
            mapGenerator.RequestMeshData(mapData,lod , OnMeshDataReceived);
        }
    }
    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistThreshHold;
    }
}

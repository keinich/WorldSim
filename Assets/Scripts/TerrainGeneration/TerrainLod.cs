using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLod : MonoBehaviour
{
  const float scale = 2f;

  const float viewerMoveThresholdForChunkUpdate = 25;
  const float sqrViewerMoveThresholdForUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

  public LODInfo[] detailLevels;  
  public static float maxViewDist = 450;

  public Transform viewer;
  public Material material;

  public static Vector2 viewerPosition;
  Vector2 viewerPositionOld;
  static TerrainGenerator terrainGenerator;
  int chunkSize;
  int chunksVisibleInViewDistance;

  Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
  static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

  private void Start() {
    terrainGenerator = FindObjectOfType<TerrainGenerator>();

    maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDist / chunkSize);

    UpdateVisibleChunks();
  }

  private void OnValidate() {
    terrainGenerator = FindObjectOfType<TerrainGenerator>();
  }

  private void Update() {
    viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

    if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForUpdate) {
      viewerPositionOld = viewerPosition;
      UpdateVisibleChunks();
    }
  }

  void UpdateVisibleChunks() {

    for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
      terrainChunksVisibleLastUpdate[i].SetVisible(false);
    }
    terrainChunksVisibleLastUpdate.Clear();

    int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
    int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

    for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
      for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {
        Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

        if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
          terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
        }
        else {
          terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, material));
        }
      }
    }
  }

  public class TerrainChunk {

    GameObject meshObject;
    Vector2 position;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;

    TerrainGenerator.TerrainMapData terrainData;
    bool terrainDataReceived;
    int previousLODIndex = -1;

    public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
      this.detailLevels = detailLevels;

      position = coord * size;
      bounds = new Bounds(position, Vector2.one * size);
      Vector3 positionV3 = new Vector3(position.x, 0, position.y);

      meshObject = new GameObject("Terrain Chunk");
      meshRenderer = meshObject.AddComponent<MeshRenderer>();
      meshRenderer.material = material;
      meshFilter = meshObject.AddComponent<MeshFilter>();

      meshObject.transform.position = positionV3 * scale;
      meshObject.transform.parent = parent;
      meshObject.transform.localScale = Vector3.one * scale;

      SetVisible(false);

      lodMeshes = new LODMesh[detailLevels.Length];
      for (int i = 0; i < detailLevels.Length; i++) {
        lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
      }

      terrainGenerator.RequestTerrainData(position, OnTerrainDataReceived);
    }

    void OnTerrainDataReceived(TerrainGenerator.TerrainMapData terrainData) {
      this.terrainData = terrainData;
      terrainDataReceived = true;

      //Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
      //meshRenderer.material.mainTexture = texture;

      UpdateTerrainChunk();
    }

    void OnMeshDataReceived(MeshData meshData) {
      print("Mesh Data received");
      meshFilter.mesh = meshData.CreateMesh();
    }

    public void UpdateTerrainChunk() {

      if (!terrainDataReceived) {
        return;
      }

      float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
      bool visible = viewerDstFromNearestEdge <= maxViewDist;

      if (visible) {
        int lodIndex = 0;

        for (int i = 0; i < detailLevels.Length - 1; i++) {
          if (viewerDstFromNearestEdge > detailLevels[i].visibleDistanceThreshold) {
            lodIndex = i + 1;
          }
          else {
            break;
          }
        }

        if (lodIndex != previousLODIndex) {
          LODMesh lodMesh = lodMeshes[lodIndex];
          if (lodMesh.hasMesh) {
            previousLODIndex = lodIndex;
            meshFilter.mesh = lodMesh.mesh;
          }
          else if (!lodMesh.hasRequestedMesh) {
            lodMesh.RequestMesh(terrainData);
          }
        }
        terrainChunksVisibleLastUpdate.Add(this);
      }
      SetVisible(visible);
    }

    public void SetVisible(bool visible) {
      meshObject.SetActive(visible);
    }

    public bool IsVisible() {
      return meshObject.activeSelf;
    }
  }

  class LODMesh {

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    System.Action updateCallback;

    public LODMesh(int lod, System.Action updateCallback) {
      this.lod = lod;
      this.updateCallback = updateCallback;
    }

    void OnMeshDataReceived(MeshData meshData) {
      mesh = meshData.CreateMesh();
      hasMesh = true;

      updateCallback();
    }

    public void RequestMesh(TerrainGenerator.TerrainMapData terrainData) {
      hasRequestedMesh = true;
      terrainGenerator.RequestMeshData(terrainData, lod, OnMeshDataReceived);
    }

  }

  [System.Serializable]
  public struct LODInfo {
    public int lod;
    public float visibleDistanceThreshold;
  }
}

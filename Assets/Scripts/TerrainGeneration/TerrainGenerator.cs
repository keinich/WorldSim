using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

  public bool printTimers;

  [Header("Mesh Settings")]
  public int mapSize = 255;
  public float scale = 20;
  public float elevationScale = 10;
  public Material material;



  // Internal
  float[] map;
  Mesh mesh;

  MeshRenderer meshRenderer;
  MeshFilter meshFilter;

  static HeightmapGenerator heightmapGenerator;

  Queue<TerrainThreadInfo<TerrainMapData>> mapDataThreadInfoQueue = new Queue<TerrainThreadInfo<TerrainMapData>>();
  Queue<TerrainThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<TerrainThreadInfo<MeshData>>();

  private void Start() {
    heightmapGenerator = GetComponent<HeightmapGenerator>();
    if (!heightmapGenerator) {
      Debug.Log("No HeightmapGenerator!");
    }
    CreateTerrain();
  }

  public void CreateTerrain() {
    GenerateHeightMap();
    Erosion erosion = GetComponent<Erosion>();
    if (erosion) {
      erosion.ErodeGpu(map, mapSize);
    }
  }

  private void OnValidate() {
  }

  void Update() {
    if (mapDataThreadInfoQueue.Count > 0) {
      for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
        TerrainThreadInfo<TerrainMapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
        threadInfo.callback(threadInfo.parameter);
      }
    }
    if (meshDataThreadInfoQueue.Count > 0) {
      for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
        TerrainThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
        threadInfo.callback(threadInfo.parameter);
      }
    }
  }

  public void GenerateHeightMap() {
    Erosion erosion = GetComponent<Erosion>();
    int mapSizeWithBorder = mapSize;
    if (erosion) {
      mapSizeWithBorder = mapSize + erosion.erosionBrushRadius * 2;
    }
    map = FindObjectOfType<HeightmapGenerator>().GenerateHeightMap(mapSizeWithBorder);
    erosion.ErodeGpu(map, mapSize);
  }

  public TerrainMapData GenerateTerrainData(Vector2 chunkCoord, int chunkSize) {
    if (!heightmapGenerator) {
      return new TerrainMapData();
    }
    float[,] heightMap = GetHeightMap(map);
    float[,] chunkHeightMap = GetChunkHeightMap(heightMap, chunkCoord, chunkSize);
    return new TerrainMapData(chunkHeightMap);
  }

  public float[,] GetChunkHeightMap(float[,] heightMap, Vector2 chunkCoord, int chunkSize) {
    float[,] result = new float[chunkSize + 3, chunkSize + 3];
    //return result;
    int startX = (int)chunkCoord.x * chunkSize;
    int startY = -(int)chunkCoord.y * chunkSize;
    for (int x = 0; x < chunkSize + 3; x++) {
      for (int y = 0; y < chunkSize + 3; y++) {
        int heightX = startX + x;
        int heightY = startY + y;
        if (heightX < 0 || heightX >= heightMap.GetLength(0) || heightY < 0 || heightY >= heightMap.GetLength(1)) {
          continue;
        }
        result[x, y] = heightMap[heightX, heightY] * 10.0f;
      }
    }
    return result;
  }

  public void ContructMesh() {

    ConstructTerrain();

    Vector3[] verts = new Vector3[mapSize * mapSize];
    int[] triangles = new int[(mapSize - 1) * (mapSize - 1) * 6];
    int t = 0;

    for (int i = 0; i < mapSize * mapSize; i++) {
      int x = i % mapSize;
      int y = i / mapSize;
      int erosionBrushRadius = 0;
      int mapSizeWithBorder = mapSize;
      Erosion erosion = GetComponent<Erosion>();      
      if (erosion) {
        mapSizeWithBorder = mapSize + erosion.erosionBrushRadius * 2;
        erosionBrushRadius = erosion.erosionBrushRadius;
      }
      int borderedMapIndex = (y + erosionBrushRadius) * mapSizeWithBorder + x + erosionBrushRadius;
      int meshMapIndex = y * mapSize + x;

      Vector2 percent = new Vector2(x / (mapSize - 1f), y / (mapSize - 1f));
      Vector3 pos = new Vector3(percent.x * 2 - 1, 0, percent.y * 2 - 1) * scale;

      float normalizedHeight = map[borderedMapIndex];
      pos += Vector3.up * normalizedHeight * elevationScale;
      verts[meshMapIndex] = pos;

      // Construct triangles
      if (x != mapSize - 1 && y != mapSize - 1) {
        t = (y * (mapSize - 1) + x) * 3 * 2;

        triangles[t + 0] = meshMapIndex + mapSize;
        triangles[t + 1] = meshMapIndex + mapSize + 1;
        triangles[t + 2] = meshMapIndex;

        triangles[t + 3] = meshMapIndex + mapSize + 1;
        triangles[t + 4] = meshMapIndex + 1;
        triangles[t + 5] = meshMapIndex;
        t += 6;
      }
    }

    if (mesh == null) {
      mesh = new Mesh();
    }
    else {
      mesh.Clear();
    }
    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.vertices = verts;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();

    AssignMeshComponents();
    meshFilter.sharedMesh = mesh;
    meshRenderer.sharedMaterial = material;

    material.SetFloat("_MaxHeight", elevationScale);
  }

  public void RequestTerrainData(Vector2 chunkCoord, int chunkSize, Action<TerrainMapData> callback) {
    ThreadStart threadStart = delegate {
      TerrainDataThread(chunkCoord, chunkSize, callback);
    };

    new Thread(threadStart).Start();
  }

  void TerrainDataThread(Vector2 chunkCoord, int chunkSize, Action<TerrainMapData> callback) {
    TerrainMapData terrainData = GenerateTerrainData(chunkCoord, chunkSize);
    lock (mapDataThreadInfoQueue) {
      mapDataThreadInfoQueue.Enqueue(new TerrainThreadInfo<TerrainMapData>(callback, terrainData));
    }
  }

  public void RequestMeshData(TerrainMapData mapData, int lod, Action<MeshData> callback) {
    //Debug.Log("Requesting Mesh Data");
    ThreadStart threadStart = delegate {
      MeshDataThread(mapData, lod, callback);
    };

    new Thread(threadStart).Start();
  }

  void MeshDataThread(TerrainMapData mapData, int lod, Action<MeshData> callback) {
    MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, elevationScale, new AnimationCurve(), lod);
    lock (meshDataThreadInfoQueue) {
      meshDataThreadInfoQueue.Enqueue(new TerrainThreadInfo<MeshData>(callback, meshData));
    }
  }

  private void ConstructTerrain() {
    Terrain terrain = FindObjectOfType<Terrain>();
    terrain.terrainData.heightmapResolution = mapSize;
    float[,] heightMap = GetTerrainHeightMap(map);
    terrain.terrainData.SetHeights(0, 0, heightMap);
  }

  private float[,] GetTerrainHeightMap(float[] map) {
    int s = (int)Mathf.Sqrt(map.Length);
    float[,] result = new float[s, s];
    for (int i = 0; i < s * s; i++) {
      int x = i % s;
      int y = i / s;
      result[x, y] = map[i] * 0.2f;
    }

    return result;
  }

  private float[,] GetHeightMap(float[] map) {
    int s = (int)Mathf.Sqrt(map.Length);
    float[,] result = new float[s, s];
    for (int i = 0; i < s * s; i++) {
      int x = i % s;
      int y = i / s;
      result[x, y] = map[i] * 0.1f;
    }

    return result;
  }

  void AssignMeshComponents() {
    // Find/creator mesh holder object in children
    string meshHolderName = "Mesh Holder";
    Transform meshHolder = transform.Find(meshHolderName);
    if (meshHolder == null) {
      meshHolder = new GameObject(meshHolderName).transform;
      meshHolder.transform.parent = transform;
      meshHolder.transform.localPosition = Vector3.zero;
      meshHolder.transform.localRotation = Quaternion.identity;
    }

    // Ensure mesh renderer and filter components are assigned
    if (!meshHolder.gameObject.GetComponent<MeshFilter>()) {
      meshHolder.gameObject.AddComponent<MeshFilter>();
    }
    if (!meshHolder.GetComponent<MeshRenderer>()) {
      meshHolder.gameObject.AddComponent<MeshRenderer>();
    }

    meshRenderer = meshHolder.GetComponent<MeshRenderer>();
    meshFilter = meshHolder.GetComponent<MeshFilter>();
  }

  struct TerrainThreadInfo<T> {
    public readonly Action<T> callback;
    public readonly T parameter;

    public TerrainThreadInfo(Action<T> callback, T parameter) {
      this.callback = callback;
      this.parameter = parameter;
    }
  }

  public struct TerrainMapData {
    public readonly float[,] heightMap;

    public TerrainMapData(float[,] heightMap) {
      this.heightMap = heightMap;
    }

  }

}
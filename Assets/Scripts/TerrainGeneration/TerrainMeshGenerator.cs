using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainMeshGenerator : MonoBehaviour {

  public bool printTimers;

  [Header("Layout")]
  public Texture2D mountainMask;
  public float mountainMapBlurResolution;
  public float mountainMapBlurSize;

  [Header("Perlin Noise")]
  public int seed;
  public bool randomizeSeed;

  public int numOctaves = 7;
  public float persistence = .5f;
  public float lacunarity = 2;
  public float initialScale = 2;

  [Header("Erosion")]
  [Range(0, 1)]
  public float inertia = .05f; // At zero, water will instantly change direction to flow downhill. At 1, water will never change direction. 
  public float sedimentCapacityFactor = 4; // Multiplier for how much sediment a droplet can carry
  public float minSedimentCapacity = .01f; // Used to prevent carry capacity getting too close to zero on flatter terrain
  [Range(0, 1)]
  public float erodeSpeed = .3f;
  [Range(0, 1)]
  public float depositSpeed = .3f;
  [Range(0, 1)]
  public float evaporateSpeed = .01f;
  public float gravity = 4;


  public ComputeShader erosion;
  public int numErosionIterations = 50000;
  public int erosionBrushRadius = 3;

  public int maxLifetime = 30;

  public float startSpeed = 1;
  public float startWater = 1;

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

  Queue<TerrainThreadInfo<TerrainMapData>> mapDataThreadInfoQueue = new Queue<TerrainThreadInfo<TerrainMapData>>();
  Queue<TerrainThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<TerrainThreadInfo<MeshData>>();

  private void Start() {
    CreateTerrain();
  }

  public void CreateTerrain() {
    GenerateHeightMap();
    Erosion.Erode(map, mapSize, numErosionIterations, erosionBrushRadius, erosion, maxLifetime, inertia, depositSpeed, minSedimentCapacity, evaporateSpeed, sedimentCapacityFactor, erodeSpeed, startSpeed, startWater, gravity);
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
    int mapSizeWithBorder = mapSize;
    mapSizeWithBorder = mapSize + erosionBrushRadius * 2;
    map = HeightmapGenerator.GenerateHeightMap(
      mapSizeWithBorder, seed, randomizeSeed, numOctaves, initialScale, persistence, lacunarity, mountainMask, mountainMapBlurResolution, mountainMapBlurSize
    );
    Erosion.Erode(map, mapSize, numErosionIterations, erosionBrushRadius, erosion, maxLifetime, inertia, depositSpeed, minSedimentCapacity, evaporateSpeed, sedimentCapacityFactor, erodeSpeed, startSpeed, startWater, gravity);
  }

  public TerrainMapData GenerateTerrainData(Vector2 chunkCoord, int chunkSize) {
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

    Vector3[] verts = new Vector3[mapSize * mapSize];
    int[] triangles = new int[(mapSize - 1) * (mapSize - 1) * 6];
    int t = 0;

    for (int i = 0; i < mapSize * mapSize; i++) {
      int x = i % mapSize;
      int y = i / mapSize;
      int mapSizeWithBorder = mapSize;
      mapSizeWithBorder = mapSize + erosionBrushRadius * 2;
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
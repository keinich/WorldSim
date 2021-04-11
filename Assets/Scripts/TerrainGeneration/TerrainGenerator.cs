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

  [Header("Erosion Settings")]
  public ComputeShader erosion;
  public int numErosionIterations = 50000;
  public int erosionBrushRadius = 3;

  public int maxLifetime = 30;
  public float sedimentCapacityFactor = 3;
  public float minSedimentCapacity = .01f;
  public float depositSpeed = 0.3f;
  public float erodeSpeed = 0.3f;

  public float evaporateSpeed = .01f;
  public float gravity = 4;
  public float startSpeed = 1;
  public float startWater = 1;
  [Range(0, 1)]
  public float inertia = 0.3f;

  // Internal
  float[] map;
  Mesh mesh;
  int mapSizeWithBorder;

  MeshRenderer meshRenderer;
  MeshFilter meshFilter;

  static HeightmapGenerator heightmapGenerator;

  Queue<TerrainThreadInfo<TerrainMapData>> mapDataThreadInfoQueue = new Queue<TerrainThreadInfo<TerrainMapData>>();
  Queue<TerrainThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<TerrainThreadInfo<MeshData>>();

  private void Start() {
    heightmapGenerator = FindObjectOfType<HeightmapGenerator>();
    if (!heightmapGenerator) {
      Debug.Log("No HeightmapGenerator!");
    }
    CreateTerrain();
  }

  public void CreateTerrain() {
    GenerateHeightMap();
    Erode();
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
    mapSizeWithBorder = mapSize + erosionBrushRadius * 2;
    map = FindObjectOfType<HeightmapGenerator>().GenerateHeightMap(mapSizeWithBorder);
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
    return result;
  }

  public void Erode() {
    int numThreads = numErosionIterations / 1024;

    // Create brush
    List<int> brushIndexOffsets = new List<int>();
    List<float> brushWeights = new List<float>();

    float weightSum = 0;
    for (int brushY = -erosionBrushRadius; brushY <= erosionBrushRadius; brushY++) {
      for (int brushX = -erosionBrushRadius; brushX <= erosionBrushRadius; brushX++) {
        float sqrDst = brushX * brushX + brushY * brushY;
        if (sqrDst < erosionBrushRadius * erosionBrushRadius) {
          brushIndexOffsets.Add(brushY * mapSize + brushX);
          float brushWeight = 1 - Mathf.Sqrt(sqrDst) / erosionBrushRadius;
          weightSum += brushWeight;
          brushWeights.Add(brushWeight);
        }
      }
    }
    for (int i = 0; i < brushWeights.Count; i++) {
      brushWeights[i] /= weightSum;
    }

    // Send brush data to compute shader
    ComputeBuffer brushIndexBuffer = new ComputeBuffer(brushIndexOffsets.Count, sizeof(int));
    ComputeBuffer brushWeightBuffer = new ComputeBuffer(brushWeights.Count, sizeof(int));
    brushIndexBuffer.SetData(brushIndexOffsets);
    brushWeightBuffer.SetData(brushWeights);
    erosion.SetBuffer(0, "brushIndices", brushIndexBuffer);
    erosion.SetBuffer(0, "brushWeights", brushWeightBuffer);

    // Generate random indices for droplet placement
    int[] randomIndices = new int[numErosionIterations];
    for (int i = 0; i < numErosionIterations; i++) {
      int randomX = UnityEngine.Random.Range(erosionBrushRadius, mapSize + erosionBrushRadius);
      int randomY = UnityEngine.Random.Range(erosionBrushRadius, mapSize + erosionBrushRadius);
      randomIndices[i] = randomY * mapSize + randomX;
    }

    // Send random indices to compute shader
    ComputeBuffer randomIndexBuffer = new ComputeBuffer(randomIndices.Length, sizeof(int));
    randomIndexBuffer.SetData(randomIndices);
    erosion.SetBuffer(0, "randomIndices", randomIndexBuffer);

    // Heightmap buffer
    ComputeBuffer mapBuffer = new ComputeBuffer(map.Length, sizeof(float));
    mapBuffer.SetData(map);
    erosion.SetBuffer(0, "map", mapBuffer);

    // Settings
    erosion.SetInt("borderSize", erosionBrushRadius);
    erosion.SetInt("mapSize", mapSizeWithBorder);
    erosion.SetInt("brushLength", brushIndexOffsets.Count);
    erosion.SetInt("maxLifetime", maxLifetime);
    erosion.SetFloat("inertia", inertia);
    erosion.SetFloat("sedimentCapacityFactor", sedimentCapacityFactor);
    erosion.SetFloat("minSedimentCapacity", minSedimentCapacity);
    erosion.SetFloat("depositSpeed", depositSpeed);
    erosion.SetFloat("erodeSpeed", erodeSpeed);
    erosion.SetFloat("evaporateSpeed", evaporateSpeed);
    erosion.SetFloat("gravity", gravity);
    erosion.SetFloat("startSpeed", startSpeed);
    erosion.SetFloat("startWater", startWater);

    // Run compute shader
    erosion.Dispatch(0, numThreads, 1, 1);
    mapBuffer.GetData(map);

    // Release buffers
    mapBuffer.Release();
    randomIndexBuffer.Release();
    brushIndexBuffer.Release();
    brushWeightBuffer.Release();
  }

  public void ContructMesh() {

    //ConstructTerrain();

    Vector3[] verts = new Vector3[mapSize * mapSize];
    int[] triangles = new int[(mapSize - 1) * (mapSize - 1) * 6];
    int t = 0;

    for (int i = 0; i < mapSize * mapSize; i++) {
      int x = i % mapSize;
      int y = i / mapSize;
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
    MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, 1, new AnimationCurve(), lod);
    lock (meshDataThreadInfoQueue) {
      meshDataThreadInfoQueue.Enqueue(new TerrainThreadInfo<MeshData>(callback, meshData));
    }
  }

  private void ConstructTerrain() {
    Terrain terrain = FindObjectOfType<Terrain>();
    terrain.terrainData.heightmapResolution = mapSize;
    float[,] heightMap = GetHeightMap(map);
    terrain.terrainData.SetHeights(0, 0, heightMap);
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
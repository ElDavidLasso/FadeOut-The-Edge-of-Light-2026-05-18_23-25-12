using UnityEngine;

public enum TileType { Empty, Floor }
public enum PivotLocation { Corner, ReverseCorner }

// Etiquetas de zona para que el algoritmo sea consciente del contexto
public enum ZoneType { None, Room, LongCorridor, ShortCorridor }

[System.Serializable]
public struct WallData
{
    public GameObject prefab;
    public PivotLocation pivotType;
}
[System.Serializable]
public struct PropData
{
    public GameObject prefab;
    public enum PlacementType { Floor, Ceiling }
    public PlacementType placement;
}

public class BSPTranslator : MonoBehaviour
{
    [Header("Suelos Aleatorios")]
    [SerializeField] private GameObject[] floorPrefabs;

    [Header("Techo del Nivel")]
    [SerializeField] private GameObject ceilingPrefab;

    [Header("Sistema de Puertas")]
    [Tooltip("Prefab del marco/puerta mejorado con pivote en la esquina")]
    [SerializeField] private GameObject doorPrefab;

    [Header("Paredes Dinámicas")]
    [SerializeField] private WallData[] wallDataArray;

    [Header("Configuración Geométrica")]
    [SerializeField] private Transform environmentParent;
    [SerializeField] private float tileSize = 3f;
    [SerializeField] private float ceilingHeight = 3f;

    [Header("Iluminación del Nivel")]
    [Tooltip("El prefab de tu lámpara de techo")]
    [SerializeField] private GameObject ceilingLampPrefab;

    // Matriz de zonas
    private ZoneType[,] zoneGrid;
    private TileType[,] mapGrid;
    private bool[,] occupiedGrid;
    private int width;
    private int height;

    public void TranslateTo3D(NodeBSP rootNode, int mapWidth, int mapHeight, Vector2Int playerSpawnGrid)
    {
        if (floorPrefabs == null || floorPrefabs.Length == 0 ||
            wallDataArray == null || wallDataArray.Length == 0 ||
            ceilingPrefab == null || doorPrefab == null)
        {
            Debug.LogError("Faltan prefabs en el Inspector.");
            return;
        }

        this.width = mapWidth;
        this.height = mapHeight;

        mapGrid = new TileType[width, height];
        zoneGrid = new ZoneType[width, height];
        occupiedGrid = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapGrid[x, y] = TileType.Empty;
                zoneGrid[x, y] = ZoneType.None;
                occupiedGrid[x, y] = false;
            }
        }

        ClearOldMap();
        CarveMapData(rootNode);
        BuildDungeon3D();

        // 1. Capa de Iluminación Estructural
        GenerateLighting(rootNode);

        // 2. Capa de Utilería Aleatoria
        ScatterProps(rootNode);
    }

    private void ClearOldMap()
    {
        if (environmentParent == null) return;
        for (int i = environmentParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = environmentParent.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    private void CarveMapData(NodeBSP node)
    {
        if (node == null) return;

        if (node.IsLeaf)
        {
            CarveRectangle(node.roomBounds, ZoneType.Room);
        }
        else
        {
            if (node.Corridors != null)
            {
                foreach (RectInt corridor in node.Corridors)
                {
                    int length = Mathf.Max(corridor.width, corridor.height);
                    ZoneType type = (length > 4) ? ZoneType.LongCorridor : ZoneType.ShortCorridor;
                    CarveRectangle(corridor, type);
                }
            }
            CarveMapData(node.leftChild);
            CarveMapData(node.rightChild);
        }
    }

    private void CarveRectangle(RectInt rect, ZoneType type)
    {
        int xMax = Mathf.Min(rect.x + rect.width, width);
        int yMax = Mathf.Min(rect.y + rect.height, height);

        for (int x = Mathf.Max(0, rect.x); x < xMax; x++)
        {
            for (int y = Mathf.Max(0, rect.y); y < yMax; y++)
            {
                mapGrid[x, y] = TileType.Floor;

                if (zoneGrid[x, y] != ZoneType.Room)
                {
                    zoneGrid[x, y] = type;
                }
            }
        }
    }

    private void BuildDungeon3D()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapGrid[x, y] == TileType.Floor)
                {
                    int randomFloorIndex = Random.Range(0, floorPrefabs.Length);
                    Instantiate(floorPrefabs[randomFloorIndex], new Vector3(x * tileSize, 0, y * tileSize), Quaternion.identity, environmentParent);

                    Instantiate(ceilingPrefab, new Vector3(x * tileSize, ceilingHeight, y * tileSize), ceilingPrefab.transform.rotation, environmentParent);

                    Vector3 floorPos = new Vector3(x * tileSize, 0, y * tileSize);

                    foreach (Vector2Int dir in directions)
                    {
                        int nx = x + dir.x;
                        int ny = y + dir.y;

                        if (nx < 0 || nx >= width || ny < 0 || ny >= height || mapGrid[nx, ny] == TileType.Empty)
                        {
                            SpawnWallElement(floorPos, dir);
                        }
                        else if (zoneGrid[x, y] == ZoneType.Room && zoneGrid[nx, ny] == ZoneType.LongCorridor)
                        {
                            SpawnDoorElement(floorPos, dir);
                        }
                    }
                }
            }
        }
    }

    [Header("Sistema de Props")]
    [SerializeField] private PropData[] propDataArray;

    [Range(0.01f, 0.5f)]
    [SerializeField] private float propDensity = 0.1f;

    private void ScatterProps(NodeBSP node)
    {
        if (node == null) return;

        if (node.IsLeaf)
        {
            int roomArea = node.roomBounds.width * node.roomBounds.height;
            int targetPropCount = Mathf.CeilToInt(roomArea * propDensity);

            int spawnedProps = 0;
            int safetyFallback = 0;

            while (spawnedProps < targetPropCount && safetyFallback < roomArea * 2)
            {
                safetyFallback++;

                int rx = Random.Range(node.roomBounds.x + 1, node.roomBounds.x + node.roomBounds.width - 1);
                int ry = Random.Range(node.roomBounds.y + 1, node.roomBounds.y + node.roomBounds.height - 1);

                if (rx >= 0 && rx < width && ry >= 0 && ry < height)
                {
                    if (mapGrid[rx, ry] == TileType.Floor &&
                        zoneGrid[rx, ry] == ZoneType.Room &&
                        !occupiedGrid[rx, ry])
                    {
                        occupiedGrid[rx, ry] = true;

                        PlaceRandomProp(new Vector3(rx * tileSize, 0, ry * tileSize));
                        spawnedProps++;
                    }
                }
            }
        }
        else
        {
            ScatterProps(node.leftChild);
            ScatterProps(node.rightChild);
        }
    }

    private void PlaceRandomProp(Vector3 position)
    {
        PropData data = propDataArray[Random.Range(0, propDataArray.Length)];

        float yOffset = (data.placement == PropData.PlacementType.Floor) ? 0 : ceilingHeight;
        Vector3 finalPos = new Vector3(
        position.x + (tileSize / 2f),
        yOffset,
        (position.z + (tileSize / 2f)) - tileSize // <--- EL AJUSTE AQUÍ
        );

        float[] angles = { 0f, 90f, 180f, -90f };
        float randomAngle = angles[Random.Range(0, angles.Length)];

        Vector3 prefabEuler = data.prefab.transform.eulerAngles;
        Quaternion propRotation = Quaternion.Euler(prefabEuler.x, prefabEuler.y + randomAngle, prefabEuler.z);

        Instantiate(data.prefab, finalPos, propRotation, environmentParent);
    }

    private void GenerateLighting(NodeBSP rootNode)
    {
        if (ceilingLampPrefab == null) return;

        bool[,] lampGrid = new bool[width, height];

        GenerateRoomLighting(rootNode, ref lampGrid);
        GenerateVisualCorridorLighting();
    }

    private void GenerateVisualCorridorLighting()
    {
        bool[,] processedCorridors = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (IsCorridor(x, y) && !processedCorridors[x, y])
                {
                    bool isHorizontal = false;
                    if ((x > 0 && IsCorridor(x - 1, y)) || (x < width - 1 && IsCorridor(x + 1, y)))
                    {
                        isHorizontal = true;
                    }

                    System.Collections.Generic.List<Vector2Int> corridorLine = new System.Collections.Generic.List<Vector2Int>();

                    if (isHorizontal)
                    {
                        int cx = x;
                        while (cx >= 0 && IsCorridor(cx, y) && !processedCorridors[cx, y])
                        {
                            corridorLine.Add(new Vector2Int(cx, y));
                            processedCorridors[cx, y] = true;
                            cx--;
                        }
                        cx = x + 1;
                        while (cx < width && IsCorridor(cx, y) && !processedCorridors[cx, y])
                        {
                            corridorLine.Add(new Vector2Int(cx, y));
                            processedCorridors[cx, y] = true;
                            cx++;
                        }
                    }
                    else
                    {
                        int cy = y;
                        while (cy >= 0 && IsCorridor(x, cy) && !processedCorridors[x, cy])
                        {
                            corridorLine.Add(new Vector2Int(x, cy));
                            processedCorridors[x, cy] = true;
                            cy--;
                        }
                        cy = y + 1;
                        while (cy < height && IsCorridor(x, cy) && !processedCorridors[x, cy])
                        {
                            corridorLine.Add(new Vector2Int(x, cy));
                            processedCorridors[x, cy] = true;
                            cy++;
                        }
                    }

                    if (corridorLine.Count > 0)
                    {
                        float sumX = 0;
                        float sumY = 0;

                        foreach (Vector2Int tile in corridorLine)
                        {
                            sumX += tile.x;
                            sumY += tile.y;
                        }

                        float centerX = sumX / corridorLine.Count;
                        float centerY = sumY / corridorLine.Count;

                        Vector3 exactCenter = new Vector3(
                        centerX * tileSize + (tileSize / 2f),
                        ceilingHeight,
                        (centerY * tileSize + (tileSize / 2f)) - tileSize // <--- EL AJUSTE AQUÍ
                        );

                        // NOTA: Si ves que la lámpara del pasillo queda "cruzada" (perpendicular al muro),
                        // simplemente intercambia los valores 90f y 0f aquí.
                        float angleOffset = isHorizontal ? 90f : 0f;

                        Vector3 prefabEuler = ceilingLampPrefab.transform.eulerAngles;
                        Quaternion lampRot = Quaternion.Euler(prefabEuler.x, prefabEuler.y + angleOffset, prefabEuler.z);

                        GameObject spawnedLamp = Instantiate(ceilingLampPrefab, exactCenter, lampRot, environmentParent);

                        // --- MAGIA DEL LEAD: IGNORAR EL PIVOTE DEL ARTISTA 3D ---
                        ForceMeshCenterToPosition(spawnedLamp, exactCenter);
                    }
                }
            }
        }
    }

    // MÉTODO DE SEGURIDAD ABSOLUTA
    private void ForceMeshCenterToPosition(GameObject obj, Vector3 targetPosition)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        // Combinamos todas las mallas en una gran caja (Bounding Box)
        Bounds totalBounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            totalBounds.Encapsulate(r.bounds);
        }

        // Calculamos cuánto desfasó el artista el pivote de su centro real
        Vector3 pivotOffset = targetPosition - totalBounds.center;

        // Bloqueamos la corrección en Y para que la lámpara no se "hunda" en el techo
        pivotOffset.y = 0f;

        // Aplicamos el empuje inverso
        obj.transform.position += pivotOffset;
    }

    private bool IsCorridor(int x, int y)
    {
        return zoneGrid[x, y] == ZoneType.LongCorridor || zoneGrid[x, y] == ZoneType.ShortCorridor;
    }

    private void GenerateRoomLighting(NodeBSP node, ref bool[,] lampGrid)
    {
        if (node == null) return;

        if (node.IsLeaf)
        {
            int rx = Mathf.Max(0, node.roomBounds.x);
            int ry = Mathf.Max(0, node.roomBounds.y);
            int rw = Mathf.Min(node.roomBounds.width, width - rx);
            int rh = Mathf.Min(node.roomBounds.height, height - ry);

            if (rw <= 0 || rh <= 0) return;

            int spacing = 4;

            int countX = Mathf.Max(1, rw / spacing);
            int countY = Mathf.Max(1, rh / spacing);

            float stepX = (float)rw / countX;
            float stepY = (float)rh / countY;

            for (int i = 0; i < countX; i++)
            {
                for (int j = 0; j < countY; j++)
                {
                    float localX = (i * stepX) + (stepX / 2f);
                    float localY = (j * stepY) + (stepY / 2f);

                    float globalX = rx + localX;
                    float globalY = ry + localY;

                    // El cálculo simétrico sincronizado con tu geometría visual
                    Vector3 lampPos = new Vector3(
                        globalX * tileSize,
                        ceilingHeight,
                        (globalY * tileSize) - tileSize // <--- EL AJUSTE AQUÍ
                    );
                    Instantiate(ceilingLampPrefab, lampPos, ceilingLampPrefab.transform.rotation, environmentParent);

                    int tileX = Mathf.Clamp(Mathf.FloorToInt(globalX), 0, width - 1);
                    int tileY = Mathf.Clamp(Mathf.FloorToInt(globalY), 0, height - 1);
                    lampGrid[tileX, tileY] = true;
                }
            }
        }
        else
        {
            GenerateRoomLighting(node.leftChild, ref lampGrid);
            GenerateRoomLighting(node.rightChild, ref lampGrid);
        }
    }

    private void SpawnWallElement(Vector3 floorPos, Vector2Int dir)
    {
        int randomWallIndex = Random.Range(0, wallDataArray.Length);
        WallData selectedData = wallDataArray[randomWallIndex];
        Vector3 wallPos = CalculateObjectPosition(floorPos, dir, selectedData.pivotType);
        float targetAngleY = CalculateRotationAngle(dir, selectedData.pivotType);

        Vector3 prefabEuler = selectedData.prefab.transform.eulerAngles;
        Quaternion wallRotation = Quaternion.Euler(prefabEuler.x, prefabEuler.y + targetAngleY, prefabEuler.z);
        Instantiate(selectedData.prefab, wallPos, wallRotation, environmentParent);
    }

    private void SpawnDoorElement(Vector3 floorPos, Vector2Int dir)
    {
        Vector3 doorPos = CalculateObjectPosition(floorPos, dir, PivotLocation.Corner);
        float targetAngleY = CalculateRotationAngle(dir, PivotLocation.Corner);

        Vector3 prefabEuler = doorPrefab.transform.eulerAngles;
        Quaternion doorRotation = Quaternion.Euler(prefabEuler.x, prefabEuler.y + targetAngleY, prefabEuler.z);
        Instantiate(doorPrefab, doorPos, doorRotation, environmentParent);
    }

    private Vector3 CalculateObjectPosition(Vector3 floorPos, Vector2Int dir, PivotLocation pivot)
    {
        Vector3 pos = Vector3.zero;

        if (pivot == PivotLocation.Corner || pivot == PivotLocation.ReverseCorner)
        {
            // Las posiciones nacen perfectamente ancladas a las 4 esquinas del suelo
            if (dir == Vector2Int.up) pos = floorPos + new Vector3(0, 0, tileSize);
            else if (dir == Vector2Int.right) pos = floorPos + new Vector3(tileSize, 0, tileSize);
            else if (dir == Vector2Int.down) pos = floorPos + new Vector3(tileSize, 0, 0);
            else if (dir == Vector2Int.left) pos = floorPos + new Vector3(0, 0, 0);
            if (dir == Vector2Int.up || dir == Vector2Int.down) pos.z -= tileSize;
            if (dir == Vector2Int.right || dir == Vector2Int.left) pos.z -= tileSize;
        }

        return pos;
    }

    private float CalculateRotationAngle(Vector2Int dir, PivotLocation pivot)
    {
        float angle = 0f;
        if (pivot == PivotLocation.Corner)
        {
            if (dir == Vector2Int.up) angle = 0f;
            else if (dir == Vector2Int.down) angle = 180f;
            else if (dir == Vector2Int.right) angle = 90f;
            else if (dir == Vector2Int.left) angle = -90f;
        }
        else if (pivot == PivotLocation.ReverseCorner)
        {
            if (dir == Vector2Int.up) angle = 180f;
            else if (dir == Vector2Int.down) angle = 0f;
            else if (dir == Vector2Int.right) angle = -90f;
            else if (dir == Vector2Int.left) angle = 90f;
        }
        return angle;
    }
}
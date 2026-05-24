using UnityEngine;

public enum TileType { Empty, Floor }
public enum PivotLocation { Corner, ReverseCorner }

// NUEVO: Etiquetas de zona para que el algoritmo sea consciente del contexto
public enum ZoneType { None, Room, LongCorridor, ShortCorridor }

[System.Serializable]
public struct WallData
{
    public GameObject prefab;
    public PivotLocation pivotType;
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

    // Matriz de zonas: Sabe exactamente si estás parado en un cuarto, un pasillo largo o uno corto
    private ZoneType[,] zoneGrid;
    private TileType[,] mapGrid;
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapGrid[x, y] = TileType.Empty;
                zoneGrid[x, y] = ZoneType.None;
            }
        }

        ClearOldMap();
        CarveMapData(rootNode);
        BuildDungeon3D();
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
                    // Evaluamos matemáticamente qué tipo de pasillo nos dio el Builder
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

                // PRIORIDAD ESTRUCTURAL: Un pasillo nunca debe borrar la etiqueta de una habitación
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

                        // CASO A: FRONTERA AL VACÍO -> PARED CIEGA
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height || mapGrid[nx, ny] == TileType.Empty)
                        {
                            SpawnWallElement(floorPos, dir);
                        }
                        // CASO B: FRONTERA DE HABITACIÓN A PASILLO LARGO -> EXACTAMENTE 1 PUERTA
                        else if (zoneGrid[x, y] == ZoneType.Room && zoneGrid[nx, ny] == ZoneType.LongCorridor)
                        {
                            SpawnDoorElement(floorPos, dir);
                        }
                        // Nota Técnica: Si toca un ShortCorridor, no entra a ningún if, dejando un arco abierto.
                    }
                }
            }
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
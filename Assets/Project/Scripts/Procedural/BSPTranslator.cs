using UnityEngine;

public enum TileType { Empty, Floor }

// 1. Creamos las etiquetas para los tipos de pivote
public enum PivotLocation { Corner, CenterEdge }

// 2. Creamos una estructura de datos serializable para el Inspector
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

    [Header("Paredes Dinámicas (Soporte Multi-Pivote)")]
    [Tooltip("Configura cada pared con su tipo de pivote correspondiente")]
    [SerializeField] private WallData[] wallDataArray;

    [Header("Configuración")]
    [SerializeField] private Transform environmentParent;
    [SerializeField] private float tileSize = 3f;

    private TileType[,] mapGrid;
    private int width;
    private int height;

    public void TranslateTo3D(NodeBSP rootNode, int mapWidth, int mapHeight, Vector2Int playerSpawnGrid)
    {
        if (floorPrefabs == null || floorPrefabs.Length == 0 || wallDataArray == null || wallDataArray.Length == 0)
        {
            Debug.LogError("Faltan prefabs en el Inspector.");
            return;
        }

        this.width = mapWidth;
        this.height = mapHeight;

        mapGrid = new TileType[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mapGrid[x, y] = TileType.Empty;

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
            CarveRectangle(node.roomBounds);
        }
        else
        {
            if (node.Corridors != null)
            {
                foreach (RectInt corridor in node.Corridors)
                    CarveRectangle(corridor);
            }
            CarveMapData(node.leftChild);
            CarveMapData(node.rightChild);
        }
    }

    private void CarveRectangle(RectInt rect)
    {
        int xMax = Mathf.Min(rect.x + rect.width, width);
        int yMax = Mathf.Min(rect.y + rect.height, height);

        for (int x = Mathf.Max(0, rect.x); x < xMax; x++)
            for (int y = Mathf.Max(0, rect.y); y < yMax; y++)
                mapGrid[x, y] = TileType.Floor;
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

                    // La baldosa nace desde su esquina inferior izquierda
                    Vector3 floorPos = new Vector3(x * tileSize, 0, y * tileSize);

                    foreach (Vector2Int dir in directions)
                    {
                        int nx = x + dir.x;
                        int ny = y + dir.y;

                        if (nx < 0 || nx >= width || ny < 0 || ny >= height || mapGrid[nx, ny] == TileType.Empty)
                        {
                            // --- SELECCIÓN DE PARED CON METADATOS ---
                            int randomWallIndex = Random.Range(0, wallDataArray.Length);
                            WallData selectedData = wallDataArray[randomWallIndex];

                            Vector3 wallPos = Vector3.zero;
                            float targetAngleY = 0f;

                            // 1. EVALUACIÓN CONDICIONAL DE GEOMETRÍA
                            if (selectedData.pivotType == PivotLocation.Corner)
                            {
                                // Matemática para la Imagen 1 (Wall 1 - Esquina)
                                if (dir == Vector2Int.up) { targetAngleY = 0f; wallPos = floorPos + new Vector3(0, 0, tileSize); }
                                else if (dir == Vector2Int.right) { targetAngleY = 90f; wallPos = floorPos + new Vector3(tileSize, 0, tileSize); }
                                else if (dir == Vector2Int.down) { targetAngleY = 180f; wallPos = floorPos + new Vector3(tileSize, 0, 0); }
                                else if (dir == Vector2Int.left) { targetAngleY = -90f; wallPos = floorPos + new Vector3(0, 0, 0); }
                                if (dir == Vector2Int.up || dir == Vector2Int.down)
                                {
                                    wallPos.z -= tileSize;
                                }
                                if (dir == Vector2Int.right || dir == Vector2Int.left)
                                {
                                    wallPos.z -= tileSize;
                                }
                            }
                            else if (selectedData.pivotType == PivotLocation.CenterEdge)
                            {
                                // Matemática para la Imagen 2 (Wall 2 - Centro del borde)
                                if (dir == Vector2Int.up) { targetAngleY = 180f; wallPos = floorPos + new Vector3(0, 0, tileSize); }
                                else if (dir == Vector2Int.right) { targetAngleY = -90f; wallPos = floorPos + new Vector3(tileSize, 0, tileSize); }
                                else if (dir == Vector2Int.down) { targetAngleY = 0f; wallPos = floorPos + new Vector3(tileSize, 0, 0); }
                                else if (dir == Vector2Int.left) { targetAngleY = 90f; wallPos = floorPos + new Vector3(0, 0, 0); }
                                if (dir == Vector2Int.up || dir == Vector2Int.down)
                                {                                    
                                    wallPos.z -= tileSize;
                                }
                                if (dir == Vector2Int.right || dir == Vector2Int.left)
                                {
                                    wallPos.z -= tileSize;
                                }
                            }

                            // 2. EXTRACCIÓN DE LA ROTACIÓN CRUDA (Respeta el FBX base)
                            Vector3 prefabEuler = selectedData.prefab.transform.eulerAngles;
                            Quaternion wallRotation = Quaternion.Euler(prefabEuler.x, prefabEuler.y + targetAngleY, prefabEuler.z);

                            Instantiate(selectedData.prefab, wallPos, wallRotation, environmentParent);
                        }
                    }
                }
            }
        }
    }
}
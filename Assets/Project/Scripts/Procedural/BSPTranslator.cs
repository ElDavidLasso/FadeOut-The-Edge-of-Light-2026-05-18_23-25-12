using UnityEngine;

public enum TileType { Empty, Floor }

public class BSPTranslator : MonoBehaviour
{
    [Header("Assets Modulares")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    [Header("Configuración")]
    [SerializeField] private Transform environmentParent;
    [SerializeField] private float tileSize = 3f;

    private TileType[,] mapGrid;
    private int width;
    private int height;

    public void TranslateTo3D(NodeBSP rootNode, int mapWidth, int mapHeight, Vector2Int playerSpawnGrid)
    {
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
                    // El suelo se mantiene en su posición de grid estándar
                    Vector3 floorPos = new Vector3(x * tileSize, 0, y * tileSize);
                    Instantiate(floorPrefab, floorPos, Quaternion.identity, environmentParent);

                    foreach (Vector2Int dir in directions)
                    {
                        int nx = x + dir.x;
                        int ny = y + dir.y;

                        if (nx < 0 || nx >= width || ny < 0 || ny >= height || mapGrid[nx, ny] == TileType.Empty)
                        {
                            Vector3 wallPos = Vector3.zero;
                            float targetAngleY = 0f;

                            // RECALIBRACIÓN GEOMÉTRICA DE ANCLAJES (SOLUCIÓN AL DESFASE EN Z)
                            if (dir == Vector2Int.up) // Pared NORTE (Vacío arriba)
                            {
                                targetAngleY = 0f;
                                // CORRECCIÓN: La pared norte debe anclarse en la base superior izquierda del suelo, 
                                // pero sin empujarla un tile completo hacia adelante.
                                wallPos = floorPos + new Vector3(0, 0, tileSize);
                                wallPos.z -= tileSize;
                            }
                            else if (dir == Vector2Int.down) // Pared SUR (Vacío abajo)
                            {
                                targetAngleY = 180f;
                                // CORRECCIÓN: Para compensar el giro de 180° que empujaba la pared hacia atrás,
                                // sumamos el tileSize en X y lo dejamos en 0 en Z para que se despliegue sobre el borde inferior.
                                wallPos = floorPos + new Vector3(tileSize, 0, 0);
                                wallPos.z -= tileSize;
                            }
                            else if (dir == Vector2Int.right) // Pared ESTE (Vacío a la derecha)
                            {
                                targetAngleY = 90f;
                                wallPos = floorPos + new Vector3(tileSize, 0, tileSize);
                                wallPos.z -= tileSize;
                            }
                            else if (dir == Vector2Int.left) // Pared OESTE (Vacío a la izquierda)
                            {
                                targetAngleY = -90f;
                                wallPos = floorPos + new Vector3(0, 0, 0);
                                wallPos.z -= tileSize;
                            }

                            // Si tras aplicar esto notas que el desfase persiste por la configuración interna del FBX,
                            // la solución matemática estándar para prefabs con pivote invertido en Z es restar una unidad lineal:
                            if (dir == Vector2Int.up || dir == Vector2Int.down)
                            {
                                // Descomenta la línea de abajo SOLO si el modelo sigue apareciendo un casillero adelantado
                                //wallPos.z -= tileSize;  
                            }

                            Vector3 prefabEuler = wallPrefab.transform.eulerAngles;
                            Quaternion wallRotation = Quaternion.Euler(prefabEuler.x, prefabEuler.y + targetAngleY, prefabEuler.z);

                            Instantiate(wallPrefab, wallPos, wallRotation, environmentParent);
                        }
                    }
                }
            }
        }
    }
}
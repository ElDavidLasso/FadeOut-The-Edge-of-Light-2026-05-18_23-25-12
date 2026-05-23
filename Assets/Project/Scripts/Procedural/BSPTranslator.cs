using System.Collections.Generic;
using UnityEngine;

public class BSPTranslator : MonoBehaviour
{
    [Header("Assets Modulares")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    [Header("Jerarquía")]
    [SerializeField] private Transform environmentParent;

    private float tileSize = 1f;

    // Esta lista mágica evitará el Z-Fighting y nos dirá dónde colocar paredes
    private HashSet<Vector2Int> floorPositions;

    public void TranslateTo3D(NodeBSP rootNode)
    {
        if (rootNode == null) return;

        floorPositions = new HashSet<Vector2Int>();

        // 1. Limpieza de jerarquía
        foreach (Transform child in environmentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Extraer los datos lógicos (Mapear sin duplicados)
        ExtractFloorData(rootNode);

        // 3. Fase de Construcción
        InstantiateFloors();
        InstantiateWalls();
    }

    private void ExtractFloorData(NodeBSP node)
    {
        if (node == null) return;

        if (node.IsLeaf)
        {
            AddRectToSet(node.roomBounds);
        }
        else
        {
            // Ahora leemos TODOS los segmentos del pasillo
            foreach (RectInt corridor in node.Corridors)
            {
                AddRectToSet(corridor);
            }

            ExtractFloorData(node.leftChild);
            ExtractFloorData(node.rightChild);
        }
    }

    private void AddRectToSet(RectInt area)
    {
        // Guardamos las coordenadas en el HashSet. Si hay superposición, 
        // el HashSet automáticamente descarta el duplicado.
        for (int x = area.x; x < area.x + area.width; x++)
        {
            for (int y = area.y; y < area.y + area.height; y++)
            {
                floorPositions.Add(new Vector2Int(x, y));
            }
        }
    }

    private void InstantiateFloors()
    {
        foreach (Vector2Int pos in floorPositions)
        {
            Vector3 worldPosition = new Vector3(pos.x * tileSize, 0, pos.y * tileSize);
            Instantiate(floorPrefab, worldPosition, Quaternion.identity, environmentParent);
        }
    }

    private void InstantiateWalls()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int floorPos in floorPositions)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = floorPos + dir;

                // Regla limpia: Si mi vecino no es un piso, PONGO UNA PARED.
                // Eliminamos la restricción de duplicados en la misma casilla, 
                // permitiendo que las esquinas internas tengan dos paredes cruzadas perfectas.
                if (!floorPositions.Contains(neighborPos))
                {
                    Vector3 wallWorldPos = new Vector3(neighborPos.x * tileSize, 0, neighborPos.y * tileSize);

                    // La pared siempre mirará hacia la casilla de piso que la solicitó
                    Vector3 lookDirection = new Vector3(-dir.x, 0, -dir.y);
                    Quaternion wallRotation = Quaternion.LookRotation(lookDirection);

                    Instantiate(wallPrefab, wallWorldPos, wallRotation, environmentParent);
                }
            }
        }
    }
}

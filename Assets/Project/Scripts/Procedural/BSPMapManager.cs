using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPMapManager : MonoBehaviour
{
    [SerializeField] private int mapWidth = 50;
    [SerializeField] private int mapHeight = 50;
    [SerializeField] private int minRoomSize = 6;

    // El nodo raíz que contendrá a todo el árbol en memoria
    private NodeBSP rootNode;

    private void Start()
    {
        MapGeneration();
    }

    [ContextMenu("Generar Mapa")]
    public void MapGeneration()
    {
        // 1. Inicializar el espacio total (Sub-tarea 1.1)
        RectInt totalArea = new RectInt(0, 0, mapWidth, mapHeight);
        rootNode = new NodeBSP(totalArea);

        // 2. Crear el constructor y ejecutar la división espacial (Sub-tarea 1.2)
        BSPBuilder builder = new BSPBuilder(minRoomSize);
        builder.SplitNode(rootNode);

        // 3. Generar las habitaciones y trazar pasillos (Sub-tarea 1.3)
        builder.GenerateRoomsAndCorridors(rootNode);

        Debug.Log("¡Estructura de datos BSP generada con éxito en memoria!");
    }

    private void OnDrawGizmos()
    {
        if (rootNode == null) return;
        DrawGizmosRecursive(rootNode);
    }

    private void DrawGizmosRecursive(NodeBSP node)
    {
        if (node == null) return;

        // 1. Dibujar el contenedor (Sub-tarea 1.2) -> Líneas delgadas
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(new Vector3(node.bounds.x + node.bounds.width / 2f, 0, node.bounds.y + node.bounds.height / 2f),
                            new Vector3(node.bounds.width, 0.1f, node.bounds.height));

        if (node.IsLeaf)
        {
            // 2. Dibujar la habitación real (Sub-tarea 1.3) -> Bloque sólido de color
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(node.roomBounds.x + node.roomBounds.width / 2f, 0, node.roomBounds.y + node.roomBounds.height / 2f),
                            new Vector3(node.roomBounds.width, 0.2f, node.roomBounds.height));
        }
        else
        {
            // 3. Dibujar el pasillo que une a sus hijos (Sub-tarea 1.3) -> Caminos conectores
            if (node.Corridor.width > 0 && node.Corridor.height > 0)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(new Vector3(node.Corridor.x + node.Corridor.width / 2f, 0, node.Corridor.y + node.Corridor.height / 2f),
                                new Vector3(node.Corridor.width, 0.25f, node.Corridor.height));
            }

            DrawGizmosRecursive(node.leftChild);
            DrawGizmosRecursive(node.rightChild);
        }
    }
}


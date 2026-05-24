using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPMapManager : MonoBehaviour
{
    [Header("Configuración BSP")]
    [SerializeField] private int mapWidth = 50;
    [SerializeField] private int mapHeight = 50;

    // 1. AÑADIMOS LA VARIABLE FALTANTE AQUÍ (minNodeSize siempre debe ser mayor a minRoomSize)
    [SerializeField] private int minNodeSize = 12;
    [SerializeField] private int minRoomSize = 6;

    [Header("Referencias")]
    [SerializeField] private BSPTranslator translator;
    [SerializeField] private GameObject player;

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

        // 2. LINEA 31 CORREGIDA: Ahora le pasamos ambos parámetros al constructor
        BSPBuilder builder = new BSPBuilder(minNodeSize, minRoomSize);

        builder.SplitNode(rootNode);

        // 3. Generar las habitaciones y trazar pasillos (Sub-tarea 1.3)
        builder.GenerateStructures(rootNode);

        // Calcula el nodo hoja del spawn antes de traducir
        NodeBSP firstLeaf = GetFirstLeaf(rootNode);
        Vector2Int playerSpawnGrid = new Vector2Int(
            firstLeaf.roomBounds.x + (firstLeaf.roomBounds.width / 2),
            firstLeaf.roomBounds.y + (firstLeaf.roomBounds.height / 2)
        );

        // 4. Traducción a 3D enviando los datos de control de flujo
        translator.TranslateTo3D(rootNode, mapWidth, mapHeight, playerSpawnGrid);

        // 5. Spawn del jugador
        SpawnPlayer(rootNode);

        Debug.Log("¡Estructura de datos BSP generada con éxito en memoria!");
    }

    private void SpawnPlayer(NodeBSP node)
    {
        // Buscamos recursivamente la primera hoja (habitacion) empezando por la izquierda
        NodeBSP firstLeaf = GetFirstLeaf(node);

        if (firstLeaf != null)
        {
            // Calculamos el centro de la habitación lógica
            float spawnX = firstLeaf.roomBounds.x + (firstLeaf.roomBounds.width / 2f);
            float spawnZ = firstLeaf.roomBounds.y + (firstLeaf.roomBounds.height / 2f);

            // Altura segura para que el CharacterController no atraviese el piso
            Vector3 spawnPosition = new Vector3(spawnX, 1.5f, spawnZ);

            // Desactivar temporalmente el CharacterController para poder teletransportarlo
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = spawnPosition;

            if (cc != null) cc.enabled = true;

            Debug.Log($"Jugador instanciado exitosamente en la habitación: {spawnPosition}");
        }
    }
    private NodeBSP GetFirstLeaf(NodeBSP node)
    {
        if (node.IsLeaf) return node;

        // Priorizamos el hijo izquierdo para estandarizar el punto de inicio
        if (node.leftChild != null) return GetFirstLeaf(node.leftChild);
        if (node.rightChild != null) return GetFirstLeaf(node.rightChild);

        return null;
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
        Gizmos.DrawWireCube(
            new Vector3(node.bounds.x + node.bounds.width / 2f, 0, node.bounds.y + node.bounds.height / 2f),
            new Vector3(node.bounds.width, 0.1f, node.bounds.height)
        );

        if (node.IsLeaf)
        {
            // 2. Dibujar la habitación real (Sub-tarea 1.3) -> Bloque sólido de color
            Gizmos.color = Color.green;
            Gizmos.DrawCube(
                new Vector3(node.roomBounds.x + node.roomBounds.width / 2f, 0, node.roomBounds.y + node.roomBounds.height / 2f),
                new Vector3(node.roomBounds.width, 0.2f, node.roomBounds.height)
            );
        }
        else
        {
            // 3. Dibujar los pasillos (Ahora es una LISTA de pasillos para formar la "L")
            if (node.Corridors != null)
            {
                Gizmos.color = Color.cyan;
                foreach (RectInt corridorPart in node.Corridors)
                {
                    if (corridorPart.width > 0 && corridorPart.height > 0)
                    {
                        Gizmos.DrawCube(
                            new Vector3(corridorPart.x + corridorPart.width / 2f, 0, corridorPart.y + corridorPart.height / 2f),
                            new Vector3(corridorPart.width, 0.25f, corridorPart.height)
                        );
                    }
                }
            }

            // 4. Continuar bajando por el árbol
            DrawGizmosRecursive(node.leftChild);
            DrawGizmosRecursive(node.rightChild);
        }
    }
}


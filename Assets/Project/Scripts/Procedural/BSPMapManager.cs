using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.AI.Navigation;

public class BSPMapManager : MonoBehaviour
{
    [Header("Configuración BSP")]
    [SerializeField] private int mapWidth = 30;
    [SerializeField] private int mapHeight = 30;
    [SerializeField] private int minNodeSize = 10;
    [SerializeField] private int minRoomSize = 12;

    [Header("Referencias de Sistemas")]
    [SerializeField] private BSPTranslator translator;
    [SerializeField] private GameObject player;

    
    [SerializeField] private NavMeshSurface navMeshSurface;

    private NodeBSP rootNode;

    private void Start()
    {
        MapGeneration();
    }

    [ContextMenu("Generar Mapa")]
    public void MapGeneration()
    {
        RectInt totalArea = new RectInt(0, 0, mapWidth, mapHeight);
        rootNode = new NodeBSP(totalArea);

        BSPBuilder builder = new BSPBuilder(minNodeSize, minRoomSize);
        builder.SplitNode(rootNode);
        builder.GenerateStructures(rootNode);

        // Calcula el nodo hoja del spawn antes de traducir
        NodeBSP firstLeaf = GetFirstLeaf(rootNode);
        Vector2Int playerSpawnGrid = new Vector2Int(
            firstLeaf.roomBounds.x + (firstLeaf.roomBounds.width / 2),
            firstLeaf.roomBounds.y + (firstLeaf.roomBounds.height / 2)
        );

        
        translator.TranslateTo3D(rootNode, mapWidth, mapHeight, playerSpawnGrid);

        
        if (navMeshSurface != null)
        {
            
            navMeshSurface.BuildNavMesh();
            Debug.Log("¡NavMesh bakeado con éxito en Runtime!");
        }
        else
        {
            Debug.LogWarning("Falta asignar el NavMeshSurface en el BSPMapManager.");
        }

        
        SpawnPlayer(rootNode);

        Debug.Log("¡Estructura de datos BSP generada con éxito en memoria!");
    }

    private void SpawnPlayer(NodeBSP node)
    {
        NodeBSP firstLeaf = GetFirstLeaf(node);
        if (firstLeaf != null)
        {
            float spawnX = firstLeaf.roomBounds.x + (firstLeaf.roomBounds.width / 2f);
            float spawnZ = firstLeaf.roomBounds.y + (firstLeaf.roomBounds.height / 2f);

           
            Vector3 spawnPosition = new Vector3(spawnX * 3f, 1.5f, (spawnZ * 3f) - 3f);

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = spawnPosition;

            if (cc != null) cc.enabled = true;
        }
    }

    private NodeBSP GetFirstLeaf(NodeBSP node)
    {
        if (node.IsLeaf) return node;
        if (node.leftChild != null) return GetFirstLeaf(node.leftChild);
        if (node.rightChild != null) return GetFirstLeaf(node.rightChild);
        return null;
    }
}
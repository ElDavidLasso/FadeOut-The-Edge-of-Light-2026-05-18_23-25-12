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

    [Header("Sistema del Director (IA)")]
    [SerializeField] private GameObject enemy;

    private NodeBSP rootNode;
    private List<RectInt> allRooms = new List<RectInt>(); // Lista maestra de habitaciones

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

        
        allRooms.Clear();
        CollectAllRooms(rootNode);

        RectInt firstRoom = allRooms[0];
        Vector2Int playerSpawnGrid = new Vector2Int(
            firstRoom.x + (firstRoom.width / 2),
            firstRoom.y + (firstRoom.height / 2)
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


        SpawnEntities(rootNode);

        Debug.Log("¡Estructura de datos BSP generada con éxito en memoria!");
    }



    private void CollectAllRooms(NodeBSP node)
    {
        if (node == null) return;
        if (node.IsLeaf)
        {
            allRooms.Add(node.roomBounds);
        }
        else
        {
            CollectAllRooms(node.leftChild);
            CollectAllRooms(node.rightChild);
        }
    }

    private void SpawnEntities(NodeBSP root)
    {
        if (allRooms.Count < 2)
        {
            Debug.LogWarning("El BSP generó menos de 2 cuartos. El enemigo y el jugador podrían aparecer juntos.");
            return;
        }


        RectInt playerRoom = allRooms[0];
        Vector3 playerPos = new Vector3((playerRoom.x + playerRoom.width / 2f) * 3f, 1.5f, (playerRoom.y + playerRoom.height / 2f) * 3f);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = playerPos;

        if (cc != null) cc.enabled = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSafeSpawn(playerPos);
        }

        Debug.Log($"Jugador instanciado en el Cuarto 0: {playerPos}");

        if (enemy != null)
        {
            RectInt enemyRoom = allRooms[allRooms.Count - 1];
            Vector3 enemyPos = new Vector3((enemyRoom.x + enemyRoom.width / 2f) * 3f, 1.5f, (enemyRoom.y + enemyRoom.height / 2f) * 3f);


            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            enemy.transform.position = enemyPos;

            if (agent != null) agent.enabled = true;

            EnemyTeleportDirector tpSystem = enemy.GetComponent<EnemyTeleportDirector>();
            if (tpSystem != null)
            {
                tpSystem.Initialize(allRooms, player.transform, 3f);
            }

            Debug.Log($"Enemigo instanciado en el Cuarto {allRooms.Count - 1}: {enemyPos}");
        }
    }
}
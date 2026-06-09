using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyTeleportDirector : MonoBehaviour
{
    [Header("Configuración del Director")]
    [Tooltip("Tiempo inactivo antes de saltar (Usa 10 para probar)")]
    public float teleportCooldown = 60f;
    public AudioClip teleportScareSound;

    public bool isChasing = false;

    private float idleTimer = 0f;
    private int lastLoggedTime = 0; 

    private Transform playerTransform;
    private List<RectInt> allRooms;
    private float tileSize = 3f;

    private AudioSource audioSource;
    private NavMeshAgent agent;

    public Transform TargetPlayer => playerTransform;

    public void Initialize(List<RectInt> rooms, Transform player, float tileScale)
    {
        allRooms = rooms;
        playerTransform = player;
        tileSize = tileScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        agent = GetComponent<NavMeshAgent>();

        idleTimer = 0f;
        lastLoggedTime = 0;
    }

    private void Update()
    {
        if (playerTransform == null || allRooms == null || allRooms.Count == 0) return;

        
        if (isChasing)
        {
            idleTimer = 0f;
            lastLoggedTime = 0;
            return;
        }

        
        bool enemyInRoom = TryGetRoom(transform.position, out RectInt enemyRoom);
        bool playerInRoom = TryGetRoom(playerTransform.position, out RectInt playerRoom);

        
        bool areInSameRoom = (enemyInRoom && playerInRoom && enemyRoom.Equals(playerRoom));

        if (!areInSameRoom)
        {
            idleTimer += Time.deltaTime;

            
            int currentSeconds = Mathf.FloorToInt(idleTimer);
            if (currentSeconds > 0 && currentSeconds % 5 == 0 && currentSeconds != lastLoggedTime)
            {
                Debug.Log($"<color=orange>[Director] Reloj de tensión: {currentSeconds} seg.</color>");
                lastLoggedTime = currentSeconds;
            }

            if (idleTimer >= teleportCooldown)
            {
                
                ExecuteTeleport(enemyInRoom ? enemyRoom : new RectInt(-1, -1, 0, 0), playerInRoom ? playerRoom : new RectInt(-1, -1, 0, 0), playerInRoom);
                idleTimer = 0f;
                lastLoggedTime = 0;
            }
        }
        else
        {
            
            idleTimer = 0f;
            lastLoggedTime = 0;
        }
    }

    
    private bool TryGetRoom(Vector3 pos, out RectInt foundRoom)
    {
        int gridX = Mathf.FloorToInt(pos.x / tileSize);
        int gridY = Mathf.FloorToInt(pos.z / tileSize);

        foreach (var room in allRooms)
        {
            if (gridX >= room.x && gridX < room.x + room.width &&
                gridY >= room.y && gridY < room.y + room.height)
            {
                foundRoom = room;
                return true;
            }
        }

        foundRoom = new RectInt(-1, -1, 0, 0);
        return false;
    }

    private void ExecuteTeleport(RectInt currentEnemyRoom, RectInt currentPlayerRoom, bool playerIsInRoom)
    {
        RectInt targetRoom = new RectInt();
        float minDistance = float.MaxValue;
        Vector2 enemyGridPos = new Vector2(transform.position.x / tileSize, transform.position.z / tileSize);

        
        foreach (var room in allRooms)
        {
            if (room.Equals(currentEnemyRoom)) continue;

            Vector2 roomCenter = new Vector2(room.x + room.width / 2f, room.y + room.height / 2f);
            float dist = Vector2.Distance(enemyGridPos, roomCenter);

            if (dist < minDistance)
            {
                minDistance = dist;
                targetRoom = room;
            }
        }

        Vector3 intendedPosition = new Vector3(
            (targetRoom.x + (targetRoom.width / 2f)) * tileSize,
            transform.position.y,
            (targetRoom.y + (targetRoom.height / 2f)) * tileSize
        );

       
        NavMeshHit hit;
        if (NavMesh.SamplePosition(intendedPosition, out hit, 10f, NavMesh.AllAreas))
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.Warp(hit.position); 
            }
            else
            {
                transform.position = hit.position;
            }
        }
        else
        {
            transform.position = intendedPosition;
        }

        if (playerIsInRoom && targetRoom.Equals(currentPlayerRoom))
        {
            if (teleportScareSound != null) audioSource.PlayOneShot(teleportScareSound);
            Debug.Log("<color=red>¡Tensión Máxima!</color>");
            if (TelemetryManager.Instance != null) TelemetryManager.Instance.RegisterEnemyTeleport(true); 
        }
        else
        {
            Debug.Log($"<color=yellow>Director: Monstruo teletransportado.</color>");
            if (TelemetryManager.Instance != null) TelemetryManager.Instance.RegisterEnemyTeleport(false); 
        }
    }
}
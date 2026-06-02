using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Fleeing, WaitingAfterFlee, Attacking }

    [Header("Estados de la IA")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrolling;

    [Header("Configuración de Visión y Movimiento")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float patrolSpeed = 1.8f;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float fleeDistance = 12f;

    [Header("Combate y Animación")]
    [Tooltip("Distancia a la que el enemigo te atrapa y mata")]
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private Animator animator; 

    [Header("Filtros de Capas (Física)")]
    [SerializeField] private LayerMask obstacleLayer;

    private NavMeshAgent agent;
    private Transform player;
    private FlashlightDecay playerFlashlight;

    private Vector3 patrolTarget;
    private float waitTimer = 0f;
    private bool isDead = false; 
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerFlashlight = playerObj.GetComponentInChildren<FlashlightDecay>();
        }

        
        if (animator == null) animator = GetComponentInChildren<Animator>();

        SetNewPatrolPoint();
    }

    private void Update()
    {
        if (player == null || isDead) return;

        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        
        if (IsHitByFlashlight() && currentState != EnemyState.Attacking)
        {
            if (currentState != EnemyState.Fleeing)
            {
                StartFleeing();
            }
        }

        
        switch (currentState)
        {
            case EnemyState.Patrolling:
                ExecutePatrol();
                break;
            case EnemyState.Chasing:
                ExecuteChase();
                break;
            case EnemyState.Fleeing:
                ExecuteFlee();
                break;
            case EnemyState.WaitingAfterFlee:
                ExecuteWaitingAfterFlee();
                break;
        }
    }

    private void ExecutePatrol()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRadius && CanSeePlayer())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            SetNewPatrolPoint();
        }
    }

    private void ExecuteChase()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        
        if (distanceToPlayer <= attackDistance)
        {
            currentState = EnemyState.Attacking;
            agent.isStopped = true; 
            agent.velocity = Vector3.zero;

            
            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDirection);

            
            if (animator != null) animator.SetTrigger("Attack");

            TriggerGameOver();
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (distanceToPlayer > detectionRadius || !CanSeePlayer())
        {
            currentState = EnemyState.Patrolling;
        }
    }

    private void TriggerGameOver()
    {
        isDead = true;
        Debug.LogWarning("¡EL ENEMIGO TE HA ATRAPADO! GAME OVER.");

        FirstPersonHorror playerScript = player.GetComponent<FirstPersonHorror>();
        if (playerScript != null)
        {
            playerScript.TriggerDeath();
        }
    }


    private void StartFleeing()
    {
        currentState = EnemyState.Fleeing;
        agent.isStopped = false;
        agent.speed = fleeSpeed;

        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 targetFleePosition = transform.position + directionAwayFromPlayer * fleeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetFleePosition, out hit, fleeDistance, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    private void ExecuteFlee()
    {
        if (!agent.pathPending && agent.remainingDistance <= 0.6f)
        {
            currentState = EnemyState.WaitingAfterFlee;
            waitTimer = 5f;
        }
    }

    private void ExecuteWaitingAfterFlee()
    {
        agent.velocity = Vector3.zero;
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRadius && CanSeePlayer())
                currentState = EnemyState.Chasing;
            else
            {
                currentState = EnemyState.Patrolling;
                SetNewPatrolPoint();
            }
        }
    }

    private bool IsHitByFlashlight()
    {
        if (playerFlashlight == null || !playerFlashlight.IsLightEffective) return false;

        Transform camTransform = playerFlashlight.transform;
        Vector3 dirToEnemy = (transform.position - camTransform.position).normalized;
        float distance = Vector3.Distance(camTransform.position, transform.position);

        if (distance > playerFlashlight.CurrentRange) return false;

        float angle = Vector3.Angle(camTransform.forward, dirToEnemy);
        if (angle > playerFlashlight.SpotlightAngle / 2f) return false;

        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, dirToEnemy, out hit, distance, obstacleLayer))
            return false;

        return true;
    }

    private void SetNewPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
            agent.SetDestination(patrolTarget);
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (Physics.Raycast(transform.position + Vector3.up * 1f, directionToPlayer, distanceToPlayer, obstacleLayer))
            return false;

        return true;
    }
}
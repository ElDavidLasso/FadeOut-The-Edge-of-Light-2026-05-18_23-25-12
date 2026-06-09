using UnityEngine;
using UnityEngine.SceneManagement; 
public enum GameState { Booting, Playing, Paused, GameOver, Victory }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias Principales")]
    [Tooltip("Arrastra aquí a tu jugador")]
    public GameObject player;

    [Header("Contención de Errores (Fail-Safes)")]
    [Tooltip("Altura mínima permitida. Si el jugador cae por debajo de esto, el mapa falló en cargarlo.")]
    public float fallThresholdY = -15f;

    
    private Vector3 safeSpawnPosition;

   
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Se detectó más de un GameManager. Destruyendo el clon.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CurrentState = GameState.Playing;
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing && player != null)
        {
            if (player.transform.position.y < fallThresholdY)
            {
                RescueFallenPlayer();
            }
        }
        if (Input.GetKeyDown(KeyCode.F1)) TriggerVictory();
        if (Input.GetKeyDown(KeyCode.F2)) TriggerGameOver();
    }

    public void RegisterSafeSpawn(Vector3 spawnPos)
    {
        safeSpawnPosition = spawnPos;
    }

    private void RescueFallenPlayer()
    {
        Debug.LogWarning("¡Alerta de Sistema! Jugador cayó al vacío. Ejecutando protocolo de rescate...");


        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;


        player.transform.position = safeSpawnPosition;


        if (cc != null) cc.enabled = true;
    }


    public void TriggerVictory()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Victory;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.ExportMetrics("Victoria - Escapó");
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.GameOver;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.ExportMetrics("Derrota - Atrapado");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Booting, Playing, Paused, GameOver, Victory }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias Principales")]
    public GameObject player;

    [Header("Contención de Errores (Fail-Safes)")]
    public float fallThresholdY = -15f;

    private Vector3 safeSpawnPosition;
    private bool isPaused = false;


    private float matchTimer = 0f;

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Playing;
        isPaused = false;
        matchTimer = 0f; 
        LockCursor();
    }
    
    [Header("Referencias a Minimapa")]
    [SerializeField] private HUDMapVisualizer hudMap;
    [SerializeField] private float bspTileSize = 3f; 
    private void Update()
    {
        if (player == null) return;


        if (CurrentState == GameState.Playing)
        {
            matchTimer += Time.deltaTime;
            if (hudMap != null)
            {
                hudMap.UpdatePlayerPosition(player.transform.position, bspTileSize);
            }
        }

        if (CurrentState == GameState.Playing && player.transform.position.y < fallThresholdY)
        {
            RescueFallenPlayer();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public string GetFormattedMatchTime()
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(matchTimer);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    public void RegisterSafeSpawn(Vector3 spawnPos)
    {
        safeSpawnPosition = spawnPos;
    }

    private void RescueFallenPlayer()
    {
        Debug.LogWarning("¡Protocolo de rescate activado!");
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = safeSpawnPosition;
        if (cc != null) cc.enabled = true;
    }

    public void TogglePause()
    {
        if (CurrentState != GameState.Playing && CurrentState != GameState.Paused) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            if (UIManager.Instance != null) UIManager.Instance.ShowPauseMenu(true);
        }
        else
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            if (UIManager.Instance != null) UIManager.Instance.ShowPauseMenu(false);
            LockCursor();
        }
    }

    public void TriggerVictory()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Victory;
        Time.timeScale = 0f;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.ExportMetrics("Victoria - Escapó");

        if (UIManager.Instance != null) UIManager.Instance.ShowVictoryScreen();
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.ExportMetrics("Derrota - Atrapado");

        if (UIManager.Instance != null) UIManager.Instance.ShowGameOverScreen();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
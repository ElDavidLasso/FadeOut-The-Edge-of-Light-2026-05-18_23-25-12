using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Panel")]
    public GameObject hudPanel;
    public TextMeshProUGUI interactionPromptText;
    public Image batteryFillBar;
    public TextMeshProUGUI batteryPercentageText;

    
    [Header("Nuevos Componentes HUD")]
    public Image flashlightIcon; 
    public TextMeshProUGUI liveTimerText; 
    [Header("Menú de Pausa")]
    public GameObject pausePanel;

    [Header("Pantallas Finales")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Textos de Gamificación")]
    public TextMeshProUGUI victoryStatsText;
    public TextMeshProUGUI gameOverStatsText;

    
    private bool tutorialActive = true;
    private float tutorialTimer = 10f;

    private bool isLookingAtDoor = false;
    private bool promptExpired = false;
    private float promptTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        hudPanel.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);

        
        tutorialActive = true;
        tutorialTimer = 10f;
        interactionPromptText.gameObject.SetActive(true);
        interactionPromptText.text = "Presiona [F] para encender/apagar la linterna";
    }

    private void Update()
    {
        
        if (liveTimerText != null && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            liveTimerText.text = GameManager.Instance.GetFormattedMatchTime();
        }

        
        if (tutorialActive)
        {
            tutorialTimer -= Time.deltaTime;
            if (tutorialTimer <= 0f)
            {
                tutorialActive = false;
                interactionPromptText.gameObject.SetActive(false);
            }
        }
        
        else if (isLookingAtDoor && !promptExpired)
        {
            promptTimer += Time.deltaTime;
            if (promptTimer >= 5f)
            {
                promptExpired = true;
                interactionPromptText.gameObject.SetActive(false); 
            }
        }
    }

    
    public void UpdateFlashlightIcon(bool isOn)
    {
        if (flashlightIcon == null) return;

        
        flashlightIcon.color = isOn ? Color.white : new Color(1f, 1f, 1f, 0.25f);
    }

    
    public void OnLookingAtInteractable(bool looking)
    {
        if (tutorialActive) return; 

        if (looking)
        {
            if (!isLookingAtDoor) 
            {
                isLookingAtDoor = true;
                promptExpired = false;
                promptTimer = 0f;
                interactionPromptText.text = "Presiona [E] para interactuar";
                interactionPromptText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (isLookingAtDoor) 
            {
                isLookingAtDoor = false;
                promptExpired = false;
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateBatteryUI(float percentage)
    {
        if (batteryFillBar != null) batteryFillBar.fillAmount = percentage / 100f;
        if (batteryPercentageText != null) batteryPercentageText.text = $"{Mathf.RoundToInt(percentage)}%";
    }

    public void ShowPauseMenu(bool isPaused)
    {
        pausePanel.SetActive(isPaused);
        if (isPaused) UnlockCursor();
    }

    public void ShowGameOverScreen()
    {
        hudPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        UnlockCursor();
        FormatEndGameStats(gameOverStatsText);
    }

    public void ShowVictoryScreen()
    {
        hudPanel.SetActive(false);
        pausePanel.SetActive(false);
        victoryPanel.SetActive(true);
        UnlockCursor();
        FormatEndGameStats(victoryStatsText);
    }

    private void FormatEndGameStats(TextMeshProUGUI targetText)
    {
        if (targetText == null || TelemetryManager.Instance == null) return;
        var data = TelemetryManager.Instance.CurrentMetrics;

        targetText.text = $"<b>REPORTE CLÍNICO DE SUPERVIVENCIA</b>\n\n" +
                          $"<color=#FFD700>Tiempo Vivo:</color> {data.totalTimePlayed}\n" +
                          $"<color=#00FF00>Distancia Recorrida:</color> {data.totalDistanceTraveledMeters} metros\n" +
                          $"<color=#00FFFF>Umbrales Cruzados:</color> {data.totalDoorsOpened} puertas\n\n" +
                          $"<b>ANÁLISIS PSICOLÓGICO (UX):</b>\n" +
                          $"• Giros de Pánico (Paranoia): {data.franticTurnsCount}\n" +
                          $"• Crisis de Parálisis (Miedo): {data.freezeMomentsCount}\n" +
                          $"• Batería Restante: {Mathf.RoundToInt(data.finalFlashlightBattery)}%\n\n" +
                          $"<b>COMPORTAMIENTO DE LA ENTIDAD:</b>\n" +
                          $"• Persecuciones Iniciadas: {data.timesChased}\n" +
                          $"• Tiempo Siendo Cazado: {data.totalTimeInChase}";
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
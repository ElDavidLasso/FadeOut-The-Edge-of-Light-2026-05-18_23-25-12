using UnityEngine;

public class PlayerTelemetryTracker : MonoBehaviour
{
    [Header("Configuración de Umbrales (Tensión)")]
    [Tooltip("Segundos quieto para considerarlo 'Congelado por miedo'")]
    [SerializeField] private float freezeTimeThreshold = 3.0f;
    [Tooltip("Grados de rotación en 1 segundo para considerarlo 'Paranoia/Susto'")]
    [SerializeField] private float franticTurnDegrees = 120f;

    
    private FlashlightDecay flashlight;

    
    private float totalDistance = 0f;
    private int freezeCount = 0;
    private int franticTurnsCount = 0;

    
    private Vector3 lastPosition;
    private float currentFreezeTimer = 0f;

    
    private float lastYRotation;
    private float rotationAccumulator = 0f;
    private float rotationTimer = 0f;

    private void Start()
    {
        lastPosition = transform.position;
        lastYRotation = transform.eulerAngles.y;

        
        flashlight = GetComponentInChildren<FlashlightDecay>();
    }

    private void Update()
    {
        TrackDistanceAndFreezes();
        TrackFranticTurns();

        
        if (TelemetryManager.Instance != null)
        {
            TelemetryManager.Instance.UpdatePhysicalMetrics(totalDistance, freezeCount, franticTurnsCount);

             if (flashlight != null) TelemetryManager.Instance.UpdateBatteryMetric(flashlight.BatteryLevel);
        }
    }

    private void TrackDistanceAndFreezes()
    {
        float frameDistance = Vector3.Distance(transform.position, lastPosition);

        
        totalDistance += frameDistance;

        
        if (frameDistance < 0.05f * Time.deltaTime)
        {
            currentFreezeTimer += Time.deltaTime;

            
            if (currentFreezeTimer >= freezeTimeThreshold)
            {
                freezeCount++;
                currentFreezeTimer = 0f; 

                if (TelemetryManager.Instance != null)
                {
                    TelemetryManager.Instance.LogEvent("Tensión_UX", "El jugador se congeló en su lugar (Duda/Miedo)");
                }
            }
        }
        else
        {
            
            currentFreezeTimer = 0f;
        }

        lastPosition = transform.position;
    }

    private void TrackFranticTurns()
    {
        float currentYRotation = transform.eulerAngles.y;

        
        float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(lastYRotation, currentYRotation));

        rotationAccumulator += deltaAngle;
        rotationTimer += Time.deltaTime;

        
        if (rotationTimer >= 1.0f)
        {
            if (rotationAccumulator >= franticTurnDegrees)
            {
                franticTurnsCount++;
                if (TelemetryManager.Instance != null)
                {
                    TelemetryManager.Instance.LogEvent("Tensión_UX", "Giro brusco detectado (Paranoia/Reacción a sonido)");
                }
            }

            
            rotationAccumulator = 0f;
            rotationTimer = 0f;
        }

        lastYRotation = currentYRotation;
    }
}

using UnityEngine;

public class InteractiveDoor : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [Tooltip("Ángulo de apertura. Usa 90 o -90 según hacia dónde deba abrirse")]
    [SerializeField] private float openAngle = 90f;
    [Tooltip("Velocidad de apertura de la puerta")]
    [SerializeField] private float smoothness = 5f;

    [Header("Bisagra Virtual (Core de la Solución)")]
    [Tooltip("Desplazamiento desde el centro de la puerta hasta el borde. Ajusta el eje X (Ej: 0.5 o -0.5) hasta que encaje con el marco.")]
    [SerializeField] private Vector3 localHingeOffset = new Vector3(0.5f, 0f, 0f);

    private bool isOpen = false;
    private float currentAngle = 0f;

    
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 worldHingePoint;

    private void Start()
    {
        
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        
        worldHingePoint = transform.TransformPoint(localHingeOffset);
    }

    private void Update()
    {
        
        float targetAngle = isOpen ? openAngle : 0f;

       
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothness);

        
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        
        transform.RotateAround(worldHingePoint, transform.up, currentAngle);
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;


        if (TelemetryManager.Instance != null && isOpen)
        {            
            TelemetryManager.Instance.RegisterDoorOpened();
        }
        else
        {
            Debug.LogWarning("La puerta se abrió, pero no hay TelemetryManager en la escena para registrarlo.");
        }
    }
}
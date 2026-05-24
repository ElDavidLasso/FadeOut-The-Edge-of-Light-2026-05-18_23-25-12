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

    // Variables para almacenar la geometría estática
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 worldHingePoint;

    private void Start()
    {
        // 1. Guardamos el estado original crudo (Puerta Cerrada)
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // 2. Calculamos matemáticamente dónde existe la bisagra virtual en el mundo 3D
        // TransformPoint convierte nuestras coordenadas locales a globales, respetando escala y rotación.
        worldHingePoint = transform.TransformPoint(localHingeOffset);
    }

    private void Update()
    {
        // Determinamos el ángulo objetivo
        float targetAngle = isOpen ? openAngle : 0f;

        // Interpolación lineal del ángulo en tiempo real
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothness);

        // --- TÉCNICA AVANZADA DE LEAD ---
        // Para evitar desalineaciones por "floating point drift" (física de flotantes acumulada),
        // SIEMPRE reiniciamos la puerta a su posición cerrada antes de moverla de nuevo.
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Rotamos toda la malla alrededor de nuestra bisagra virtual matemática
        transform.RotateAround(worldHingePoint, transform.up, currentAngle);
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }
}
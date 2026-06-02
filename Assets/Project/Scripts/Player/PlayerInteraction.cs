using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Alcance")]
    [Tooltip("Distancia máxima en metros a la que el jugador puede interactuar con objetos")]
    [SerializeField] private float interactionDistance = 3f;

    [Tooltip("Capa de Unity asignada a los interactuables para optimizar el Raycast")]
    [SerializeField] private LayerMask interactableLayer;

    private Transform cameraTransform;

    private void Start()
    {
        
        Camera mainCam = GetComponentInChildren<Camera>();
        if (mainCam != null)
        {
            cameraTransform = mainCam.transform;
        }
        else
        {
            Debug.LogError("No se encontró una cámara hija en el objeto del Jugador.");
        }
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (cameraTransform == null) return;

        
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            
            InteractiveDoor door = hit.collider.GetComponentInParent<InteractiveDoor>() ?? hit.collider.GetComponent<InteractiveDoor>();

            if (door != null)
            {
                door.ToggleDoor();
            }
        }
    }
}

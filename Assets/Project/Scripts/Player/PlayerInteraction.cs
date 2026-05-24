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
        // Buscamos automáticamente la cámara principal en los objetos hijos
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
        // Escuchamos la tecla de acción de forma global
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (cameraTransform == null) return;

        // Creamos un rayo físico desde el centro óptico de la cámara hacia adelante
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        // Ejecutamos el Raycast optimizado por máscara de capa (LayerMask)
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Buscamos el componente en el objeto golpeado o en sus padres directos
            InteractiveDoor door = hit.collider.GetComponentInParent<InteractiveDoor>() ?? hit.collider.GetComponent<InteractiveDoor>();

            if (door != null)
            {
                door.ToggleDoor();
            }
        }
    }
}

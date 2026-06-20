using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Alcance")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private Transform cameraTransform;

    
    private bool wasLookingAtDoorLastFrame = false;

    private void Start()
    {
        Camera mainCam = GetComponentInChildren<Camera>();
        if (mainCam != null) cameraTransform = mainCam.transform;
        else Debug.LogError("No se encontró una cámara hija en el objeto del Jugador.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        
        CheckLookingAtDoor();
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

    
    private void CheckLookingAtDoor()
    {
        if (cameraTransform == null || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            InteractiveDoor door = hit.collider.GetComponentInParent<InteractiveDoor>() ?? hit.collider.GetComponent<InteractiveDoor>();
            if (door != null)
            {
                
                if (!wasLookingAtDoorLastFrame)
                {
                    if (UIManager.Instance != null) UIManager.Instance.OnLookingAtInteractable(true);
                    wasLookingAtDoorLastFrame = true;
                }
                return; 
            }
        }

        
        if (wasLookingAtDoorLastFrame)
        {
            if (UIManager.Instance != null) UIManager.Instance.OnLookingAtInteractable(false);
            wasLookingAtDoorLastFrame = false;
        }
    }
}

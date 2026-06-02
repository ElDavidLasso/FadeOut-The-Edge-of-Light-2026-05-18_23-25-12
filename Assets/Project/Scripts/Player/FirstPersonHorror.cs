using UnityEngine;
using System.Collections; 

[RequireComponent(typeof(CharacterController))]
public class FirstPersonHorror : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Parámetros de Cámara")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Animación y Modelo")]
    [Tooltip("Arrastra aquí el modelo 3D del jugador que contiene el Animator")]
    [SerializeField] private Animator animator;
    [Tooltip("Tiempo que tarda la animación de derrota antes de desaparecer (en segundos)")]
    [SerializeField] private float timeToDisappear = 2.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    private bool isDead = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        
        if (isDead) return;

        HandleMouseLook();
        HandleMovement();
        UpdateAnimations();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

       
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        if (moveDirection.magnitude > 1f) moveDirection.Normalize();

        
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }
        velocity.y -= gravity * Time.deltaTime;
       
        Vector3 finalMovement = (moveDirection * walkSpeed) + (Vector3.up * velocity.y);

        controller.Move(finalMovement * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);

        
        animator.SetFloat("Speed", horizontalVelocity.magnitude);
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;

        
        velocity = Vector3.zero;

       
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f); 
            animator.SetBool("Die", true);
        }

        
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        
        yield return new WaitForSeconds(timeToDisappear);

        
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        
        Debug.Log("El jugador ha muerto y desaparecido en la oscuridad.");

        
    }
}

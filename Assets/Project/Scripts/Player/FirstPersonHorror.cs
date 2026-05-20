using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonHorror : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    [SerializeField] private float walkSpeed = 3.5f; // Lento y tenso
    [SerializeField] private float gravity = 9.81f;

    [Header("Parámetros de Cámara")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Bloquear el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotación vertical de la cámara (Pitch)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        // Rotación horizontal del jugador (Yaw)
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Movimiento relativo hacia donde mira el jugador
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;

        // Normalizar para evitar movimiento diagonal más rápido
        if (moveDirection.magnitude > 1f) moveDirection.Normalize();

        controller.Move(moveDirection * walkSpeed * Time.deltaTime);

        // Gravedad estricta (sin salto)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Mantener al jugador pegado al suelo
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

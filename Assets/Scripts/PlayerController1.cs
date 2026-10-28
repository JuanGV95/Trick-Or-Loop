
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController1 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera Settings")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactionLayer = ~0;

   
   

    [Header("Player Manager")]
    [SerializeField] private PlayerManager playerManager;

   
    private Coroutine pressResetCoroutine;
    private InteractableItem currentInteractable;

    private InputAction moveAction;
    private InputAction interactAction;

    private CharacterController controller;
    private Animator animator;

    private bool isPickingUp = false;
    private InteractableItem pendingItem;

    private float verticalRotation = 0f;
    private float verticalVelocity = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Si no se asigna manualmente, busca la cámara principal.
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (playerManager == null)
        {
            playerManager = FindObjectOfType<PlayerManager>();
        }
    }

    private void OnEnable()
    {
        if (inputActionsAsset == null)
        {
            Debug.LogError("Asigna el Input Action Asset.");
            return;
        }

        var playerMap = inputActionsAsset.FindActionMap("Player");

        if (playerMap == null)
        {
            Debug.LogError("No se encontró el mapa Player.");
            return;
        }

        moveAction = playerMap.FindAction("Move");
        interactAction = playerMap.FindAction("Interact");

        inputActionsAsset.Enable();
    }

    private void Start()
    {
        
        if (playerCamera != null)
        {
            verticalRotation = playerCamera.localEulerAngles.x;

            if (verticalRotation > 180f)
                verticalRotation -= 360f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        if (inputActionsAsset != null)
            inputActionsAsset.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (isPickingUp)
        {
            animator.SetFloat("Speed", 0f);
            ApplyGravity();
            return;
        }

        HandleCameraRotation();
        HandleMovement();
        HandleInteraction();
    }

    private void HandleCameraRotation()
    {
        if (playerCamera == null || Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * 0.02f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.02f;

        // Girar el cuerpo horizontalmente.
        transform.Rotate(Vector3.up * mouseX);

        // Mirar hacia arriba y abajo con la cámara.
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(
            verticalRotation,
            minVerticalAngle,
            maxVerticalAngle
        );

        playerCamera.localRotation = Quaternion.Euler(
            verticalRotation,
            0f,
            0f
        );
    }

    private void HandleMovement()
    {
        if (moveAction == null)
            return;

        Vector2 inputValue = moveAction.ReadValue<Vector2>();

        // El movimiento sigue la orientación horizontal del jugador.
        Vector3 moveDirection =
            transform.right * inputValue.x +
            transform.forward * inputValue.y;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            controller.Move(
                moveDirection * moveSpeed * Time.deltaTime
            );
        }

        ApplyGravity();

        float currentSpeed = inputValue.magnitude * moveSpeed;

        animator.SetFloat(
            "Speed",
            currentSpeed,
            0.1f,
            Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        controller.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }

    private void HandleInteraction()
    {
        if (interactAction == null)
            return;

        if (interactAction.WasPressedThisFrame())
        {
            if (playerManager != null)
            {
                playerManager.RegisterInteractionPress();
            }
        }
    }





    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRadius
        );
    }
}

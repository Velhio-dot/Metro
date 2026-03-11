using UnityEngine;

public class Player1 : MonoBehaviour
{
    public static Player1 Instance { get; private set; }

    [Header("Ссылки на данные")]
    [SerializeField] private PlayerDataSO playerData;

    [Header("Настройки движения")]
    private float baseMoveSpeed = 5f;
    private float sprintMultiplier = 1.5f;

    [Header("Компоненты")]
    private Rigidbody2D rb;
    private Flashlight flashlight;

    [Header("Состояния")]
    private Vector2 lastMovementDirection = Vector2.down;
    private bool isMoving = false;
    private bool isRunning = false;
    private bool isSprinting = false;
    private float minMovingThreshold = 0.1f;

    // Свойства
    public Vector2 LastMovementDirection => lastMovementDirection;
    public bool IsMoving => isMoving;
    public bool IsRunning => isRunning;
    public bool IsSprint => isSprinting;
    public bool IsSprinting => isSprinting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        

        rb = GetComponent<Rigidbody2D>();
        flashlight = GetComponentInChildren<Flashlight>();

        InitializeFromData();
        Debug.Log("Player1 инициализирован (новая система)");
    }

    private void InitializeFromData()
    {
        if (playerData != null)
        {
            baseMoveSpeed = playerData.MoveSpeed;
            sprintMultiplier = playerData.SprintMultiplier;

            if (flashlight != null)
            {
                flashlight.gameObject.SetActive(playerData.HasFlashlight);
            }
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (GameInput.Instance == null) return;

        Vector2 inputVector = GameInput.Instance.GetMovement();
        isMoving = inputVector.magnitude > minMovingThreshold;
        isRunning = isMoving;
        isSprinting = GameInput.Instance.IsSprintPressed();

        if (isMoving)
        {
            lastMovementDirection = inputVector.normalized;
        }

        float currentSpeed = baseMoveSpeed;
        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }

        if (rb != null)
        {
            rb.MovePosition(rb.position + inputVector * (currentSpeed * Time.fixedDeltaTime));
        }
    }

    // Фонарик
    public void SetFlashlight(Flashlight newFlashlight)
    {
        flashlight = newFlashlight;
        if (playerData != null) playerData.HasFlashlight = true;
    }

    public Flashlight GetFlashlight() => flashlight;
    public bool HasFlashlight() => playerData?.HasFlashlight ?? false;

    // Утилиты
    public bool IsMovingUp() => lastMovementDirection.y > 0.5f;
    public bool IsMovingDown() => lastMovementDirection.y < -0.5f;
    public bool IsMovingLeft() => lastMovementDirection.x < -0.5f;
    public bool IsMovingRight() => lastMovementDirection.x > 0.5f;
    public bool Isrunning() => isRunning;

    public Vector3 GetPlayerScreenPosition()
    {
        return Camera.main != null ?
            Camera.main.WorldToScreenPoint(transform.position) :
            transform.position;
    }


    // Чекпоинты
    public void SaveCheckpoint(string sceneName)
    {
        if (playerData != null)
        {
            playerData.LastCheckpointPosition = transform.position;
            playerData.LastCheckpointScene = sceneName;
        }
    }

    public void LoadCheckpoint()
    {
        if (playerData != null && playerData.LastCheckpointPosition != Vector2.zero)
        {
            transform.position = playerData.LastCheckpointPosition;
            playerData.RestoreFullHealth();
        }
    }
}
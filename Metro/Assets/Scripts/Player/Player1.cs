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
    private bool isMoving;
    private bool isRunning;
    private bool isSprinting;
    private float minMovingThreshold = 0.1f;

    public Vector2 LastMovementDirection => lastMovementDirection;
    public bool IsMoving => isMoving;
    public bool IsRunning => isRunning;
    public bool IsSprint => isSprinting;
    public bool IsSprinting => isSprinting;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        flashlight = GetComponentInChildren<Flashlight>();

        InitializeFromData();
    }

    private bool TryInitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return false;
        }

        Instance = this;
        return true;
    }

    private void InitializeFromData()
    {
        if (playerData == null)
        {
            return;
        }

        baseMoveSpeed = playerData.MoveSpeed;
        sprintMultiplier = playerData.SprintMultiplier;

        if (flashlight != null)
        {
            flashlight.gameObject.SetActive(playerData.HasFlashlight);
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (GameInput.Instance == null || rb == null)
        {
            return;
        }

        Vector2 input = GameInput.Instance.GetMovement();
        bool movingNow = input.magnitude > minMovingThreshold;

        isMoving = movingNow;
        isRunning = movingNow;
        isSprinting = movingNow && GameInput.Instance.IsSprintPressed();

        if (movingNow)
        {
            lastMovementDirection = input.normalized;
        }

        float currentSpeed = isSprinting ? baseMoveSpeed * sprintMultiplier : baseMoveSpeed;
        rb.MovePosition(rb.position + input * (currentSpeed * Time.fixedDeltaTime));
    }

    public void SetFlashlight(Flashlight newFlashlight)
    {
        flashlight = newFlashlight;
        if (playerData != null)
        {
            playerData.HasFlashlight = true;
        }
    }

    public Flashlight GetFlashlight() => flashlight;
    public bool HasFlashlight() => playerData?.HasFlashlight ?? false;

    public bool IsMovingUp() => lastMovementDirection.y > 0.5f;
    public bool IsMovingDown() => lastMovementDirection.y < -0.5f;
    public bool IsMovingLeft() => lastMovementDirection.x < -0.5f;
    public bool IsMovingRight() => lastMovementDirection.x > 0.5f;
    public bool Isrunning() => isRunning;

    public Vector3 GetPlayerScreenPosition()
    {
        if (Camera.main == null)
        {
            return transform.position;
        }

        return Camera.main.WorldToScreenPoint(transform.position);
    }

    public void SaveCheckpoint(string sceneName)
    {
        if (playerData == null)
        {
            return;
        }

        playerData.LastCheckpointPosition = transform.position;
        playerData.LastCheckpointScene = sceneName;
    }

    public void LoadCheckpoint()
    {
        if (playerData == null || playerData.LastCheckpointPosition == Vector2.zero)
        {
            return;
        }

        transform.position = playerData.LastCheckpointPosition;
        playerData.RestoreFullHealth();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

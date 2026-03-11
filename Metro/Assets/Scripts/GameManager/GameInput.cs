using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    private PlayerInputActions playerInputActions;

    // Для обнаружения нажатия (не зажатия)
    private bool interactWasPressed = false;
    private bool flashlightWasPressed = false;
    private bool saveWasPressed = false;
    private bool loadWasPressed = false;

    // События для UI уведомлений
    public event System.Action OnSavePressed;
    public event System.Action OnLoadPressed;
    public event System.Action OnNewGamePressed;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameInput instance in scene!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        Debug.Log("GameInput initialized with save/load hotkeys");
    }

    private void Update()
    {
        CheckSaveLoadInputs();
    }

    private void CheckSaveLoadInputs()
    {
        // === СОХРАНЕНИЕ (F5) ===
        float saveValue = playerInputActions.Player.Save.ReadValue<float>();
        bool savePressedNow = saveValue > 0.1f;

        if (savePressedNow && !saveWasPressed)
        {
            saveWasPressed = true;
            OnSaveKeyPressed();
        }

        if (!savePressedNow)
        {
            saveWasPressed = false;
        }

        // === ЗАГРУЗКА (F9) ===
        float loadValue = playerInputActions.Player.Load.ReadValue<float>();
        bool loadPressedNow = loadValue > 0.1f;

        if (loadPressedNow && !loadWasPressed)
        {
            loadWasPressed = true;
            OnLoadKeyPressed();
        }

        if (!loadPressedNow)
        {
            loadWasPressed = false;
        }
    }

    private void OnSaveKeyPressed()
    {
        Debug.Log("💾 Save key pressed (Input System)");

        if (DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.SaveGame();
        }

        OnSavePressed?.Invoke();
    }

    private void OnLoadKeyPressed()
    {
        Debug.Log("🔄 Load key pressed (Input System)");

        if (DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.LoadGame();
        }

        OnLoadPressed?.Invoke();
    }

    // ===== ОРИГИНАЛЬНЫЕ МЕТОДЫ (сохраняем) =====

    public Vector2 GetMovement()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        return mousePos;
    }

    public bool IsSprintPressed()
    {
        return playerInputActions.Player.Sprint.ReadValue<float>() > 0.1f;
    }

    public bool IsInteractPressed()
    {
        float currentValue = playerInputActions.Player.Interact.ReadValue<float>();
        bool isPressedNow = currentValue > 0.1f;

        if (isPressedNow && !interactWasPressed)
        {
            interactWasPressed = true;
            return true;
        }

        if (!isPressedNow)
        {
            interactWasPressed = false;
        }

        return false;
    }

    public bool IsInteractHeld()
    {
        return playerInputActions.Player.Interact.ReadValue<float>() > 0.1f;
    }

    public bool IsFlashlightToggled()
    {
        float currentValue = playerInputActions.Player.Flashlight.ReadValue<float>();
        bool isPressedNow = currentValue > 0.1f;

        if (isPressedNow && !flashlightWasPressed)
        {
            flashlightWasPressed = true;
            return true;
        }

        if (!isPressedNow)
        {
            flashlightWasPressed = false;
        }

        return false;
    }

    // ===== НОВЫЕ МЕТОДЫ ДЛЯ СОХРАНЕНИЯ =====

    public bool IsSavePressed()
    {
        float currentValue = playerInputActions.Player.Save.ReadValue<float>();
        bool isPressedNow = currentValue > 0.1f;

        if (isPressedNow && !saveWasPressed)
        {
            saveWasPressed = true;
            return true;
        }

        if (!isPressedNow)
        {
            saveWasPressed = false;
        }

        return false;
    }

    public bool IsLoadPressed()
    {
        float currentValue = playerInputActions.Player.Load.ReadValue<float>();
        bool isPressedNow = currentValue > 0.1f;

        if (isPressedNow && !loadWasPressed)
        {
            loadWasPressed = true;
            return true;
        }

        if (!isPressedNow)
        {
            loadWasPressed = false;
        }

        return false;
    }

    // Метод для принудительного сохранения/загрузки из кода
    public void TriggerSave()
    {
        OnSaveKeyPressed();
    }

    public void TriggerLoad()
    {
        OnLoadKeyPressed();
    }

    private void OnDestroy()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Disable();
            playerInputActions.Dispose();
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Метод для отладки
    public string GetInputStatus()
    {
        float interactValue = playerInputActions.Player.Interact.ReadValue<float>();
        float saveValue = playerInputActions.Player.Save.ReadValue<float>();
        float loadValue = playerInputActions.Player.Load.ReadValue<float>();

        return $"Interact: {interactValue:F2}, Save: {saveValue:F2}, Load: {loadValue:F2}";
    }
}
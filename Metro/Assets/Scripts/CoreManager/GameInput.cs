using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private const float InputThreshold = 0.1f;

    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private bool interactWasPressed;
    private bool flashlightWasPressed;
    private bool saveWasPressed;
    private bool loadWasPressed;

    public event System.Action OnSavePressed;
    public event System.Action OnLoadPressed;
    public event System.Action OnNewGamePressed;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    private bool TryInitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate GameInput on '{gameObject.name}', keeping '{Instance.gameObject.name}'.");
            Destroy(this);
            return false;
        }

        Instance = this;
        return true;
    }

    private void Update()
    {
        CheckSaveLoadInputs();
    }

    private void CheckSaveLoadInputs()
    {
        if (ConsumeButtonPress(playerInputActions.Player.Save.ReadValue<float>(), ref saveWasPressed))
        {
            OnSaveKeyPressed();
        }

        if (ConsumeButtonPress(playerInputActions.Player.Load.ReadValue<float>(), ref loadWasPressed))
        {
            OnLoadKeyPressed();
        }
    }

    private static bool ConsumeButtonPress(float inputValue, ref bool wasPressed)
    {
        bool pressedNow = inputValue > InputThreshold;

        if (pressedNow && !wasPressed)
        {
            wasPressed = true;
            return true;
        }

        if (!pressedNow)
        {
            wasPressed = false;
        }

        return false;
    }

    private void OnSaveKeyPressed()
    {
        DataCoordinator.Instance?.SaveGame();
        OnSavePressed?.Invoke();
    }

    private void OnLoadKeyPressed()
    {
        DataCoordinator.Instance?.LoadGame();
        OnLoadPressed?.Invoke();
    }

    public Vector2 GetMovement()
    {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector3 GetMousePosition()
    {
        if (Mouse.current == null)
        {
            return Vector3.zero;
        }

        return Mouse.current.position.ReadValue();
    }

    public bool IsSprintPressed()
    {
        return playerInputActions.Player.Sprint.ReadValue<float>() > InputThreshold;
    }

    public bool IsInteractPressed()
    {
        return ConsumeButtonPress(playerInputActions.Player.Interact.ReadValue<float>(), ref interactWasPressed);
    }

    public bool IsInteractHeld()
    {
        return playerInputActions.Player.Interact.ReadValue<float>() > InputThreshold;
    }

    public bool IsFlashlightToggled()
    {
        return ConsumeButtonPress(playerInputActions.Player.Flashlight.ReadValue<float>(), ref flashlightWasPressed);
    }

    public bool IsSavePressed()
    {
        return ConsumeButtonPress(playerInputActions.Player.Save.ReadValue<float>(), ref saveWasPressed);
    }

    public bool IsLoadPressed()
    {
        return ConsumeButtonPress(playerInputActions.Player.Load.ReadValue<float>(), ref loadWasPressed);
    }

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
            playerInputActions = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public string GetInputStatus()
    {
        float interactValue = playerInputActions.Player.Interact.ReadValue<float>();
        float saveValue = playerInputActions.Player.Save.ReadValue<float>();
        float loadValue = playerInputActions.Player.Load.ReadValue<float>();

        return $"Interact: {interactValue:F2}, Save: {saveValue:F2}, Load: {loadValue:F2}";
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoorTeleport : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    
    [Header("Camera Settings")]
    [Tooltip("Target virtual camera. Use MonoBehaviour to be type-agnostic (fixes compile errors).")]
    [SerializeField] private MonoBehaviour virtualCamera;

    [Header("Transition (Optional)")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnId;
    [SerializeField] private bool useForceMove = true;
    [SerializeField] private Vector2 transitionForceDirection = Vector2.up;

    [Header("Key Requirements")]
    [SerializeField] private bool requireKey = false;
    [SerializeField] private ItemDataSO requiredKey;
    [SerializeField] private string lockedMessage = "Key required!";

    [Header("Audio")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private AudioClip lockedSound;

    [Header("Diagnostics")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private float doorCooldown = 1.0f;

    private float lastTeleportTime;
    private BoxCollider2D doorCollider;
    private bool isExternallyLocked = false;
    private bool isLocalBusy = false;

    private enum DoorState { Ready, Busy, Locked, OnCooldown }

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        // Reset state when room is re-enabled to prevent "Busy" lock
        isLocalBusy = false;
    }

    private void Start()
    {
        RegisterInTeleportManager();
    }

    private void RegisterInTeleportManager()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.RegisterDoor(this);
        }
    }

    private void UnregisterFromTeleportManager()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.UnregisterDoor(this);
        }
    }

    private DoorState GetCurrentState()
    {
        if (isExternallyLocked) return DoorState.Locked;
        if (isLocalBusy) return DoorState.Busy;
        if (Time.time - lastTeleportTime < doorCooldown) return DoorState.OnCooldown;
        return DoorState.Ready;
    }

    public void Interact()
    {
        if (Player1.Instance == null) return;
        
        DoorState state = GetCurrentState();
        if (state != DoorState.Ready)
        {
            if (state == DoorState.Locked) PlayLockedFeedback();
            if (debugLogs) Debug.Log("[Door " + name + "] Can't interact: " + state);
            return;
        }

        if (requireKey && !HasRequiredKey())
        {
            PlayLockedFeedback();
            return;
        }

        if (TeleportManager.Instance != null && TeleportManager.Instance.CanTeleport())
        {
            isLocalBusy = true;
            lastTeleportTime = Time.time;
            
            PlayTeleportSound(transform.position);

            // Если указана сцена — делаем переход
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                if (CoreManager.Instance != null && CoreManager.Instance.Fader != null)
                {
                    CoreManager.Instance.Fader.LoadSceneWithFade(targetSceneName, true, targetSpawnId, useForceMove ? transitionForceDirection : Vector2.zero);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
                }
            }
            else
            {
                // Иначе — обычный телепорт внутри сцены
                TeleportManager.Instance.StartTeleportSequence(
                    Player1.Instance.gameObject, 
                    targetPosition, 
                    fadeInDuration, 
                    fadeOutDuration, 
                    virtualCamera, 
                    this,
                    useForceMove ? transitionForceDirection : Vector2.zero
                );
            }

            // Cooldown routine might get cut off by LocationSwitch, hence OnEnable reset
            StartCoroutine(LocalCooldownRoutine());
        }
        else
        {
             if (debugLogs) Debug.Log("[Door " + name + "] Global teleport is busy.");
        }
    }

    private IEnumerator LocalCooldownRoutine()
    {
        yield return new WaitForSeconds(doorCooldown + fadeInDuration + fadeOutDuration);
        isLocalBusy = false;
    }

    private bool HasRequiredKey()
    {
        return true; // Simplified
    }

    private void PlayTeleportSound(Vector3 position)
    {
        if (teleportSound != null) AudioSource.PlayClipAtPoint(teleportSound, position);  
    }

    private void PlayLockedFeedback()
    {
        if (lockedSound != null) AudioSource.PlayClipAtPoint(lockedSound, transform.position);
    }

    public void SetExternalLock(bool locked)
    {
        isExternallyLocked = locked;
    }

    private void OnDisable()
    {
         UnregisterFromTeleportManager();
    }

    private void OnGUI()
    {
        if (!Debug.isDebugBuild || !debugLogs) return;

        DoorState state = GetCurrentState();
        GUI.Label(new Rect(10, 190, 400, 30), "Door " + name + ": " + state);
    }
}

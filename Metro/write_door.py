
import os
content = r"""using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemamachine; // ространство имен для Cinemamachine 3 в Unity 6!

public class DoorTeleport : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    
    [Header("Camera Settings (Unity 6)")]
    [Tooltip("сли не назначено, скрипт попытается найти активную камеру автоматически")]
    [SerializeField] private CinemamachineCamera virtualCamera;

    [Header("UI Reference")]
    [SerializeField] private Image fadePanel;

    [Header("Key Requirements")]
    [SerializeField] private bool requireKey = false;
    [SerializeField] private ItemDataSO requiredKey;
    [SerializeField] private string lockedMessage = "Требуется ключ!";

    [Header("Audio")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private AudioClip lockedSound;

    [Header("Diagnostics")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private float doorCooldown = 1.5f;

    private float lastTeleportTime;
    private BoxCollider2D doorCollider;
    private Coroutine activeTeleportCoroutine;
    private bool isExternallyLocked = false;

    private enum DoorState { Ready, Busy, Locked, OnCooldown }
    private DoorState currentState = DoorState.Ready;

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        InitializeFadePanel();
    }

    private void Start()
    {
        RegisterInTeleportManager();
    }

    private void InitializeFadePanel()
    {
        if (fadePanel == null)
        {
            Debug.LogError("[верь " + name + "] Fade Panel не назначен! Создай UI Image на Canvas.");
            return;
        }

        fadePanel.color = Color.clear;
        fadePanel.gameObject.SetActive(false);
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

    public void Interact()
    {
        if (Player1.Instance == null) return;
        
        if (currentState != DoorState.Ready || isExternallyLocked)
        {
            if (currentState == DoorState.Locked || isExternallyLocked)
            {
                PlayLockedFeedback();
            }
            return;
        }

        if (requireKey && !HasRequiredKey())
        {
            PlayLockedFeedback();
            return;
        }

        if (activeTeleportCoroutine == null)
        {
            activeTeleportCoroutine = StartCoroutine(TeleportCoroutine(Player1.Instance.gameObject));
        }
    }

    private bool HasRequiredKey()
    {
        return true; // ременно упрощено
    }

    private IEnumerator TeleportCoroutine(GameObject player)
    {
        currentState = DoorState.Busy;
        lastTeleportTime = Time.time;

        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.StartGlobalTeleport(this);
        }

        // 1. атемнение
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        // 2. одготовка
        Vector3 previousPosition = player.transform.position;
        PlayTeleportSound(previousPosition);
        
        // 3. Щ   Ы
        player.transform.position = targetPosition.position;

        //  Unity 6 Cinemamachine 3 мы должны сообщить активной камере о варпе
        WarpCamera(player.transform, targetPosition.position - previousPosition);

        // 4. роявление
        if (fadePanel != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadePanel.gameObject.SetActive(false);
        }

        CompleteTeleport();
    }

    private void WarpCamera(Transform playerTransform, Vector3 delta)
    {
        // 1. спользуем назначенную камеру
        if (virtualCamera != null)
        {
            virtualCamera.OnTargetObjectWarped(playerTransform, delta);
            if (debugLogs) Debug.Log("[верь " + name + "] амера варпнута (назначенная)");
            return;
        }

        // 2. щем активную в системе Cinemamachine
        var activeCamera = CinemamachineCore.Instance.GetActiveCinemamachineCamera(0);
        if (activeCamera != null)
        {
            if (activeCamera is CinemamachineCamera cmCam)
            {
                cmCam.OnTargetObjectWarped(playerTransform, delta);
                if (debugLogs) Debug.Log("[верь " + name + "] амера варпнута (активная)");
            }
        }
    }

    private void PlayTeleportSound(Vector3 position)
    {
        if (teleportSound != null) AudioSource.PlayClipAtPoint(teleportSound, position);  
    }

    private void PlayLockedFeedback()
    {
        if (lockedSound != null) AudioSource.PlayClipAtPoint(lockedSound, transform.position);
    }

    private void CompleteTeleport()
    {
        activeTeleportCoroutine = null;
        currentState = DoorState.OnCooldown;

        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.EndGlobalTeleport(this);
        }

        StartCoroutine(CooldownTimer());
    }

    private IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(doorCooldown);
        currentState = DoorState.Ready;
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

        string stateText = currentState.ToString();
        if (isExternallyLocked) stateText += " (LOCKED)";

        GUI.Label(new Rect(10, 190, 400, 30), $"Door {name}: {stateText}");
        if (currentState == DoorState.OnCooldown)
        {
            float timeLeft = doorCooldown - (Time.time - lastTeleportTime);
            if (timeLeft > 0f) GUI.Label(new Rect(10, 220, 400, 30), $"CD: {timeLeft:F1}s");
        }
    }
}
"""
filepath = r"f:/GitHub/Metro/Metro/Assets/Scripts/Map/DoorTeleport.cs"
with open(filepath, "w", encoding="utf-8") as f:
    f.write(content)


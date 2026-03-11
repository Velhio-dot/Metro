using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DoorTeleport : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float cameraCatchUpTime = 0.4f;

    [Header("UI Reference")]
    [SerializeField] private Image fadePanel;

    

    [Header("Key Requirements")]
    [SerializeField] private bool requireKey = false;
    [SerializeField] private ItemDataSO requiredKey;
    [SerializeField] private string lockedMessage = "Требуется ключ!";

    [Header("Door Settings")]
    [SerializeField] private float doorCooldown = 1.5f;
    [SerializeField] private bool debugLogs = true;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem teleportParticles;
    [SerializeField] private AudioClip teleportSound;



    private enum DoorState { Ready, Teleporting, OnCooldown }
    private DoorState currentState = DoorState.Ready;

    private bool isExternallyLocked = false;
    private float lastTeleportTime = 0f;
    private Player1 player;
    private Collider2D doorCollider;
    private Coroutine activeTeleportCoroutine;

    void Start()
    {
        player = Player1.Instance;
        doorCollider = GetComponent<Collider2D>();



        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.RegisterDoor(this);
            Log($"Зарегистрирована в TeleportManager");
        }

        

        InitializeFadePanel();
    }

    void OnDestroy()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.UnregisterDoor(this);
        }

        if (activeTeleportCoroutine != null)
        {
            StopCoroutine(activeTeleportCoroutine);
        }
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            Log("Взаимодействие отклонено");
            return;
        }

        StartTeleportProcess();
    }

    private bool CanInteract()
    {
        if (isExternallyLocked)
        {
            Log("Заблокирована TeleportManager");
            return false;
        }

        if (currentState != DoorState.Ready)
        {
            Log($"Не готово. Состояние: {currentState}");
            return false;
        }

        if (TeleportManager.Instance != null && !TeleportManager.Instance.CanStartTeleport())
        {
            Log("Глобальная блокировка TeleportManager");
            return false;
        }



        if (player == null)
        {
            Log("Игрок не найден");
            return false;
        }

        if (targetPosition == null)
        {
            LogError("Target Position не назначен!");
            return false;
        }

        return true;
    }

    // ОБНОВЛЕННЫЙ МЕТОД ДЛЯ ПРОВЕРКИ КЛЮЧА


    private void StartTeleportProcess()
    {
        Log($"=== НАЧАЛО ТЕЛЕПОРТАЦИИ {name} ===");

        currentState = DoorState.Teleporting;
        lastTeleportTime = Time.time;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (TeleportManager.Instance != null)
            TeleportManager.Instance.StartGlobalTeleport(this);

        if (activeTeleportCoroutine != null)
            StopCoroutine(activeTeleportCoroutine);

        activeTeleportCoroutine = StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        Log("[1] Начало последовательности телепортации");

        // 1. Затемнение
        yield return StartCoroutine(PerformFade(0f, 1f, fadeInDuration));

        if (currentState != DoorState.Teleporting)
        {
            Log("Прервано после затемнения");
            yield break;
        }

        // 2. Телепортация
        PerformTeleport();

        // 3. Пауза
        yield return new WaitForSeconds(cameraCatchUpTime);

        // 4. Осветление
        yield return StartCoroutine(PerformFade(1f, 0f, fadeOutDuration));

        // 5. Завершение
        CompleteTeleport();
    }

    private IEnumerator PerformFade(float fromAlpha, float toAlpha, float duration)
    {
        if (fadePanel == null)
        {
            LogError("Fade Panel не назначен!");
            yield break;
        }

        fadePanel.color = new Color(0, 0, 0, fromAlpha);
        fadePanel.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, toAlpha);
        if (toAlpha == 0f)
            fadePanel.gameObject.SetActive(false);

        Log($"Fade завершён: {fromAlpha} → {toAlpha}");
    }

    private void PerformTeleport()
    {
        if (player == null || targetPosition == null) return;

        bool playerWasEnabled = player.enabled;
        player.enabled = false;

        Vector3 oldPosition = player.transform.position;
        PlayTeleportEffects(oldPosition);

        // Телепортация игрока
        player.transform.position = targetPosition.position;
        Log($"Игрок телепортирован: {oldPosition} → {targetPosition.position}");

        // Телепортация камеры
        





        player.enabled = playerWasEnabled;
    }



    

    private void PlayTeleportEffects(Vector3 position)
    {
        if (teleportParticles != null)
        {
            teleportParticles.Play();
            Log("Запущены частицы телепортации");
        }

        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, position);
            Log("Проигран звук телепортации");
        }
    }

    private void CompleteTeleport()
    {
        Log($"=== ТЕЛЕПОРТАЦИЯ {name} УСПЕШНО ЗАВЕРШЕНА ===");

        if (doorCollider != null)
            doorCollider.enabled = true;

        currentState = DoorState.OnCooldown;

        if (TeleportManager.Instance != null)
            TeleportManager.Instance.EndGlobalTeleport(this);

        StartCoroutine(CooldownTimer());
    }

    private IEnumerator CooldownTimer()
    {
        float cooldownEndTime = Time.time + doorCooldown;
        while (Time.time < cooldownEndTime) yield return null;
        currentState = DoorState.Ready;
        Log("Кулдаун завершён, дверь готова к использованию");
    }

    public void SetExternalLock(bool locked)
    {
        isExternallyLocked = locked;
        Log(locked ? "Внешне заблокирована" : "Внешно разблокирована");
    }

    private void InitializeFadePanel()
    {
        if (fadePanel != null)
        {
            fadePanel.color = Color.clear;
            fadePanel.gameObject.SetActive(false);
            Log("Fade panel инициализирован");
        }
        else
        {
            LogError("Fade Panel не назначен! Создай UI Image на Canvas.");
        }
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log($"[Дверь {name}] {message}");
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[Дверь {name}] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[Дверь {name}] {message}");
    }

    void OnDisable()
    {
        ForceReset();
    }

    private void ForceReset()
    {
        if (fadePanel != null)
        {
            fadePanel.color = Color.clear;
            fadePanel.gameObject.SetActive(false);
        }

        if (activeTeleportCoroutine != null)
        {
            StopCoroutine(activeTeleportCoroutine);
            activeTeleportCoroutine = null;
        }

        if (doorCollider != null)
            doorCollider.enabled = true;

        currentState = DoorState.Ready;

        if (TeleportManager.Instance != null)
            TeleportManager.Instance.EndGlobalTeleport(this);
    }

    void OnGUI()
    {
        if (Debug.isDebugBuild && debugLogs)
        {
            string stateText = currentState.ToString();
            if (isExternallyLocked) stateText += " (LOCKED)";

            GUI.Label(new Rect(10, 190, 400, 30), $"🚪 {name}: {stateText}");

            if (currentState == DoorState.OnCooldown)
            {
                float timeLeft = doorCooldown - (Time.time - lastTeleportTime);
                if (timeLeft > 0)
                {
                    GUI.Label(new Rect(10, 220, 400, 30), $"⏱️ Кулдаун: {timeLeft:F1}с");
                }
            }
        }
    }
}
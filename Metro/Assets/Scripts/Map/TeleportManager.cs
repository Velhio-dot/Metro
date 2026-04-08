using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance { get; private set; }

    [Header("Settings")]
    public float globalCooldown = 0.5f;
    public float safetyTimeout = 5.0f; 

    private List<DoorTeleport> allDoors = new List<DoorTeleport>();
    private bool isGlobalTeleportActive = false;
    private float lastGlobalTeleportTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isGlobalTeleportActive && Time.time - lastGlobalTeleportTime > safetyTimeout)
        {
            Debug.LogWarning("[TeleportManager] Глобальная блокировка зависла! Принудительный сброс.");
            ResetGlobalLock();
        }
    }

    public void RegisterDoor(DoorTeleport door)
    {
        if (door != null && !allDoors.Contains(door)) allDoors.Add(door);
    }

    public void UnregisterDoor(DoorTeleport door)
    {
        if (allDoors.Contains(door)) allDoors.Remove(door);
    }

    public bool CanTeleport()
    {
        if (isGlobalTeleportActive) return false;
        return (Time.time - lastGlobalTeleportTime >= globalCooldown);
    }

    public void StartTeleportSequence(GameObject player, Transform target, float fadeIn, float fadeOut, MonoBehaviour camRef, DoorTeleport initiatingDoor, Vector2 forceDirection = default)
    {
        if (!CanTeleport()) return;
        StartCoroutine(GlobalTeleportCoroutine(player, target, fadeIn, fadeOut, camRef, initiatingDoor, forceDirection));
    }

    private IEnumerator GlobalTeleportCoroutine(GameObject player, Transform target, float fadeIn, float fadeOut, MonoBehaviour camRef, DoorTeleport initiatingDoor, Vector2 forceDirection)
    {
        isGlobalTeleportActive = true;
        lastGlobalTeleportTime = Time.time;

        SceneFader fader = (CoreManager.Instance != null) ? CoreManager.Instance.Fader : FindFirstObjectByType<SceneFader>();

        // 0. Захват управления и автоматический шаг
        if (Player1.Instance != null)
        {
            Player1.Instance.SetControl(false);
            if (forceDirection != Vector2.zero)
            {
                Player1.Instance.SetForceMove(forceDirection);
                yield return new WaitForSeconds(0.3f); // Длительность «шага» в дверь
                Player1.Instance.SetForceMove(Vector2.zero);
            }
        }

        Debug.Log("[TeleportManager] СТАРТ: Затемнение.");

        // 1. Fade Out
        if (fader != null)
        {
            yield return fader.FadeOut();
        }

        Debug.Log("[TeleportManager] ПРЫЖОК: Перемещение игрока и камеры.");

        // 2. Teleport
        if (player != null && target != null)
        {
            Vector3 prevPos = player.transform.position;
            Vector3 targetPos = target.position;
            player.transform.position = targetPos;

            // Warp Camera
            if (camRef != null)
            {
                try {
                    var method = camRef.GetType().GetMethod("OnTargetObjectWarped", new System.Type[] { typeof(Transform), typeof(Vector3) });
                    if (method != null) method.Invoke(camRef, new object[] { player.transform, targetPos - prevPos });
                } catch (System.Exception e) {
                    Debug.LogWarning("[TeleportManager] Ошибка варпа камеры: " + e.Message);
                }
            }
        }
        else
        {
            Debug.LogError("[TeleportManager] ОШИБКА: Игрок или Цель потеряны во время телепорта!");
        }

        // Ждем один фрейм для стабилизации физики/камер после прыжка
        yield return new WaitForEndOfFrame();

        Debug.Log("[TeleportManager] ФИНАЛ: Проявление экрана.");

        // 3. Fade In
        if (fader != null)
        {
            yield return fader.FadeIn();
        }

        // 4. Возврат управления
        if (Player1.Instance != null)
        {
            Player1.Instance.SetControl(true);
        }

        ResetGlobalLock();
        Debug.Log("[TeleportManager] ПОЛНОЕ ЗАВЕРШЕНИЕ.");
    }

    public void ResetGlobalLock()
    {
        isGlobalTeleportActive = false;
        lastGlobalTeleportTime = Time.time;
        
        // Разблокируем все двери, которые еще живы
        for (int i = allDoors.Count - 1; i >= 0; i--)
        {
            if (allDoors[i] != null) allDoors[i].SetExternalLock(false);
        }
    }
}

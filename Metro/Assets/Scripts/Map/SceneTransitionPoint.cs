using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionPoint : MonoBehaviour
{
    [Header("Настройки перехода")]
    [SerializeField] private string targetSceneName; // Сцена назначения
    [SerializeField] private string targetSpawnId; // ID спавна в целевой сцене

    [Header("Визуал")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private bool isTransitionLocked = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitionLocked)
        {
            InitiateTransition();
        }
    }

    private void InitiateTransition()
    {
        Debug.Log($"Переход: {targetSceneName} -> спавн {targetSpawnId}");

        // Сохраняем целевой спавн в DataCoordinator
        if (DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.SaveGame();
            DataCoordinator.Instance.SetTargetSpawn(targetSpawnId);
        }

        // Используем существующий SceneLoader для перехода
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(targetSceneName, true);
        }
        else
        {
            // Fallback
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)collider.offset, collider.size);

            // Рисуем стрелку
            Vector3 startPos = transform.position + (Vector3)collider.offset;
            Vector3 endPos = startPos + Vector3.up * 1.5f;
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawLine(endPos, endPos + Vector3.left * 0.3f + Vector3.down * 0.3f);
            Gizmos.DrawLine(endPos, endPos + Vector3.right * 0.3f + Vector3.down * 0.3f);

            // Пишем название целевой сцены
#if UNITY_EDITOR
            UnityEditor.Handles.Label(startPos + Vector3.up * 1.8f,
                $"{targetSceneName}\n[{targetSpawnId}]");
#endif
        }
    }

    public void LockTransition() => isTransitionLocked = true;
    public void UnlockTransition() => isTransitionLocked = false;
}
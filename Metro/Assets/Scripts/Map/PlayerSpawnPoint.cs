using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Идентификатор спавна")]
    [SerializeField] private string spawnId = "default";

    public string SpawnId => spawnId;

    private void Start()
    {
        // При старте сцены проверяем, должны ли мы здесь заспавниться
        if (DataCoordinator.Instance != null &&
            DataCoordinator.Instance.TargetSpawnId == spawnId)
        {
            // Передвигаем игрока на эту точку
            StartCoroutine(MovePlayerToSpawn());
        }
    }

    private System.Collections.IEnumerator MovePlayerToSpawn()
    {
        // Ждем кадр чтобы Player успел заспавниться
        yield return null;

        if (Player1.Instance != null)
        {
            Player1.Instance.transform.position = transform.position;
            Debug.Log($"Игрок перемещен на спавн {spawnId}");

            // Сбрасываем целевой спавн
            if (DataCoordinator.Instance != null)
            {
                DataCoordinator.Instance.ClearTargetSpawn();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.3f);

        // Рисуем направление взгляда
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);

        // Пишем ID спавна
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
            $"Spawn: {spawnId}");
#endif
    }
}
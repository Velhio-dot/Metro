using UnityEngine;

public class LevelExit : MonoBehaviour, IInteractable
{
    [Header("Exit Settings")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool requireKey = false;
    [SerializeField] private ItemDataSO requiredKey;
    [SerializeField] private string lockedMessage = "Требуется ключ для выхода!";

    [Header("Visuals")]
    [SerializeField] private GameObject lockedIndicator;
    [SerializeField] private GameObject unlockedIndicator;

   

    // Публичное свойство для доступа к имени следующей сцены
    public string NextSceneName => nextSceneName;

    void Start()
    {
        // Получаем ссылку на инвентарь
        

        UpdateVisuals();
    }

    public void Interact()
    {
        if (requireKey && !HasRequiredKey())
        {
            Debug.Log(lockedMessage);
            return;
        }

        ExitLevel();
    }

    private bool HasRequiredKey()
    {
        if (!requireKey) return true;
        if (requiredKey == null) return true;

       
        

        Debug.LogError("LevelExit: Не найдена система инвентаря!");
        return false;
    }

    private void ExitLevel()
    {
        Debug.Log($"Выход из уровня в сцену: {nextSceneName}");

        // Использовать ключ если нужно
        

        // Загрузка сцены
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Сохраняем информацию о последнем выходе
            SaveLastExitInfo();

            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    

    private void SaveLastExitInfo()
    {
        // Сохраняем информацию для TransitionManager
        PlayerPrefs.SetString("LastExitID", gameObject.name);
        PlayerPrefs.SetString("LastExitScene", nextSceneName);
        PlayerPrefs.Save();
    }

    // МЕТОД, КОТОРЫЙ ИЩЕТ TransitionManager
    public string GetLastExitScene()
    {
        // Просто возвращаем имя следующей сцены
        return nextSceneName;
    }

    // Альтернативный метод для получения всей информации
    public ExitInfo GetExitInfo()
    {
        return new ExitInfo
        {
            exitName = gameObject.name,
            nextScene = nextSceneName,
            requiresKey = requireKey,
            requiredKeyId = requiredKey?.itemId
        };
    }

    private void UpdateVisuals()
    {
        if (lockedIndicator != null)
            lockedIndicator.SetActive(requireKey && !HasRequiredKey());

        if (unlockedIndicator != null)
            unlockedIndicator.SetActive(!requireKey || HasRequiredKey());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UpdateVisuals();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = requireKey ? Color.yellow : Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1f);

        if (requireKey && requiredKey != null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                $"🔑 {requiredKey.itemName}");
#endif
        }
    }
}

// Структура для хранения информации о выходе
[System.Serializable]
public class ExitInfo
{
    public string exitName;
    public string nextScene;
    public bool requiresKey;
    public string requiredKeyId;
}
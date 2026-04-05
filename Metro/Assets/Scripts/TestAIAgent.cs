using UnityEngine;

public class TestAIAgent : MonoBehaviour
{
    // Этот метод вызывается автоматически при запуске игры, даже если скрипта нет на сцене!
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInit()
    {
        // ИИ сам создает пустышку на сцене и вешает на нее этот скрипт
        GameObject go = new GameObject("TestAIAgent_AutoCreated");
        go.AddComponent<TestAIAgent>();
    }

    void Start()
    {
        Debug.Log("Привет! Это тестовый скрипт от ИИ. Я смог сам добавить себя на сцену! 🚀🤖");
    }
}

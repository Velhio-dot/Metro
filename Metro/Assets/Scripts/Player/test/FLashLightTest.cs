using UnityEngine;
using UnityEngine.Rendering.Universal;
//[ExecuteAlways]
public class FlashlightTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private float testRange = 5f;
    [SerializeField] private float testAngle = 45f;

    [Header("References")]
    [SerializeField] private Light2D freeformLight;
    private SpriteRenderer testSprite;

    private void Awake()
    {
        CreateTestSprite();
    }

    private void CreateTestSprite()
    {
        GameObject testObj = new GameObject("TestConeSprite");
        testObj.transform.SetParent(transform);
        testObj.transform.localPosition = Vector3.zero;

        testSprite = testObj.AddComponent<SpriteRenderer>();
        testSprite.sprite = CreateConeSprite();
        testSprite.color = new Color(1f, 0f, 0f, 0.3f); // Красный полупрозрачный
        testSprite.sortingOrder = 10;
    }

    private Sprite CreateConeSprite()
    {
        int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);

        // Очищаем
        for (int y = 0; y < textureSize; y++)
            for (int x = 0; x < textureSize; x++)
                texture.SetPixel(x, y, Color.clear);

        Vector2 tip = new Vector2(textureSize * 0.5f, 0);
        Vector2 leftBase = new Vector2(0, textureSize - 1);
        Vector2 rightBase = new Vector2(textureSize - 1, textureSize - 1);

        // Заполняем треугольник
        for (int y = 0; y < textureSize; y++)
        {
            float t = (float)y / (textureSize - 1);
            float width = Mathf.Lerp(0, textureSize, t);
            int startX = Mathf.RoundToInt((textureSize - width) * 0.5f);
            int endX = Mathf.RoundToInt(startX + width);

            for (int x = startX; x < endX; x++)
            {
                if (x >= 0 && x < textureSize)
                    texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0f));
    }

    private void Update()
    {
        UpdateTestSprite();
    }

    private void UpdateTestSprite()
    {
        if (testSprite != null)
        {
            // Масштабируем под текущие настройки
            testSprite.transform.localScale = new Vector3(testRange, testRange, 1f);

            // Можно добавить поворот если нужно
             testSprite.transform.rotation = Quaternion.Euler(0, 0, -90);
        }
    }

    [ContextMenu("Apply To Freeform")]
    private void ApplyToFreeform()
    {
        Debug.Log($"Настройки для Freeform: Range={testRange}, Angle={testAngle}");
        Debug.Log("Вручную настрой Freeform shape в инспекторе чтобы совпадал с красным конусом");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 direction = transform.up;
        Vector3 endPos = transform.position + direction * testRange;

        Gizmos.DrawLine(transform.position, endPos);

        Vector3 leftBound = Quaternion.Euler(0, 0, testAngle / 2) * direction * testRange;
        Vector3 rightBound = Quaternion.Euler(0, 0, -testAngle / 2) * direction * testRange;

        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
    }
}
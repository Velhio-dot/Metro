using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private float lightRange = 30f;
    [SerializeField] private float lightAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Light Reference")]
    [SerializeField] private Light2D freeformLight; // Перетащи сюда настроенный префаб

    private bool isActive = true;
    private Vector2 direction = Vector2.down;

    public bool IsActive => isActive;
    public Vector2 Direction => direction;

    private void Update()
    {
        UpdateFlashlightDirection();
        UpdateLightRotation();
        Debug.DrawRay(transform.position, direction * 5f, Color.yellow);
        if (GameInput.Instance.IsFlashlightToggled())
        {
            ToggleFlashlight();
            Debug.Log($"Flashlight toggled to: {isActive}");
        }
    }

    private void UpdateFlashlightDirection()
    {
        Vector3 mouseScreenPos = GameInput.Instance.GetMousePosition();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f));
        Vector3 playerWorldPos = Player1.Instance.transform.position;

        direction = (mouseWorldPos - playerWorldPos).normalized;
        if (direction.magnitude < 0.1f) direction = Vector2.down;
    }

    private void UpdateLightRotation()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        freeformLight.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Свет работает только если фонарик разблокирован В ПРОГРЕССЕ
        bool canUse = ProgressManager.Instance != null && ProgressManager.Instance.HasFlashlight;
        
        if (freeformLight.enabled != (isActive && canUse))
        {
            freeformLight.enabled = isActive && canUse;
            Debug.Log($"[Flashlight] Визуальное состояние света: {freeformLight.enabled}");
        }
    }

    public void ToggleFlashlight()
    {
        if (ProgressManager.Instance != null && !ProgressManager.Instance.HasFlashlight)
        {
            Debug.Log("[Flashlight] Попытка включения без фонарика в инвентаре!");
            isActive = false;
            return;
        }

        isActive = !isActive;
        Debug.Log($"[Flashlight] Состояние изменено: {(isActive ? "ВКЛ" : "ВЫКЛ")}");
    }

    public bool IsEnemyInLight(Transform enemyTransform)
    {
        if (!isActive) return false;

        Vector2 toEnemy = enemyTransform.position - transform.position;
        float distanceToEnemy = toEnemy.magnitude;
        if (distanceToEnemy > lightRange)  return false;

        float angleToEnemy = Vector2.Angle(direction, toEnemy.normalized);
        if (angleToEnemy > lightAngle / 2f) return false;
        if (angleToEnemy < lightAngle || distanceToEnemy < lightRange) 
        {
            // Debug.Log("Враг в свету");
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toEnemy.normalized, distanceToEnemy, obstacleMask);
        return hit.collider == null;
    }
}
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackTimeout = 3f; // Таймаут атаки

    [Header("Damage Type")]
    [SerializeField] private bool instantKill = false;
    [SerializeField] private float instantKillDamage = 999f;

    [Header("Visual Reference")]
    [SerializeField] private FoxVisual foxVisual;

    private float lastAttackTime;
    private bool isAttacking = false;
    private float attackStartTime; // Время начала атаки

    void Start()
    {
        // Автоматически находим FoxVisual если не назначен
        if (foxVisual == null)
        {
            foxVisual = GetComponentInChildren<FoxVisual>();
           
        }
    }

    void Update()
    {
        // Автоматический сброс атаки по таймауту
        if (isAttacking && Time.time > attackStartTime + attackTimeout)
        {
            Debug.LogWarning("EnemyAttack: Таймаут атаки! Принудительный сброс.");
            isAttacking = false;
        }
    }

    public bool CanAttack()
    {
        if (isAttacking)
        {
            
            return false;
        }
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("EnemyAttack: Атака на перезарядке");
            return false;
        }
        if (Player1.Instance == null)
        {
            Debug.Log("EnemyAttack: Игрок не найден");
            return false;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player1.Instance.transform.position);
        bool inRange = distanceToPlayer <= attackRange;


        return inRange;
    }

    // Вызывается из FoxVisual когда начинается анимация атаки
    public void StartAttackFromVisual()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        attackStartTime = Time.time; // Запоминаем время начала атаки
        Debug.Log("EnemyAttack: Атака начата из FoxVisual");
    }

    // Вызывается из FoxVisual в середине анимации атаки
    public void CheckAttackHit()
    {
        if (Player1.Instance == null)
        {
            isAttacking = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player1.Instance.transform.position);


        if (distanceToPlayer <= attackRange * 1.2f)
        {
            SuccessfulAttack();
        }
        else
        {
            Debug.Log("EnemyAttack: Атака промахнулась");
            isAttacking = false;
        }
    }

    void SuccessfulAttack()
    {
        Debug.Log("EnemyAttack: Успешная атака!");

        // Проигрываем анимацию успешной атаки через FoxVisual
        if (foxVisual != null)
        {
            foxVisual.PlaySuccessfulAttack();
        }

        // Наносим урон игроку
        if (Player1.Instance.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            if (instantKill)
            {
                playerHealth.TakeDamage(instantKillDamage);
                Debug.Log("EnemyAttack: Мгновенная смерть! 💀");
            }
            else
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"EnemyAttack: Нанесен урон - {attackDamage}");
            }
        }
    }

    // Вызывается в конце анимации атаки
    public void OnAttackFinished()
    {
        isAttacking = false;
        Debug.Log("EnemyAttack: Атака завершена");
    }

    // Метод для изменения типа атаки во время игры
    public void SetInstantKill(bool isInstantKill)
    {
        instantKill = isInstantKill;
        Debug.Log($"EnemyAttack: Враг теперь {(isInstantKill ? "УБИВАЕТ МГНОВЕННО" : "наносит обычный урон")}");
    }

    // Для визуализации радиуса атаки
    void OnDrawGizmosSelected()
    {
        Gizmos.color = instantKill ? Color.magenta : Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
using UnityEngine;
using System;

public class FoxVisual : MonoBehaviour
{
    [SerializeField] private EnemyAI _enemyAI;
    [SerializeField] private EnemyAttack _enemyAttack;
    private Animator _animator;

    private const string RUNNING = "Running";
    private const string ATTACK = "Attack";
    private const string ATTACK_SUCCESS = "AttackSuccess";
    private const string ISDIE = "IsDie";
    private const string CHASINGSPEED = "chasingSpeedMulti";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        SubscribeToEvents();
    }
    public void StartAttackAnimation()
    {
        _animator.SetTrigger(ATTACK);
        Debug.Log("FoxVisual: Анимация атаки запущена");

        if (_enemyAttack != null)
        {
            _enemyAttack.StartAttackFromVisual();
        }
    }

    private void SubscribeToEvents()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnEnemyAttack += OnEnemyAttackHandler;
            Debug.Log("FoxVisual: Подписка на событие атаки выполнена");
        }
        else
        {
            Debug.LogError("FoxVisual: EnemyAI не назначен в инспекторе!");
        }
    }

    private void Update()
    {
        _animator.SetBool(RUNNING, _enemyAI.IsRunning);
        _animator.SetFloat(CHASINGSPEED, _enemyAI.GetRoamingAnimationSpeed());
    }

    private void OnEnemyAttackHandler(object sender, EventArgs e)
    {
        Debug.Log("FoxVisual: Событие атаки получено!");

        // Проверка аниматора и параметров
        if (_animator == null)
        {
            Debug.LogError("FoxVisual: Animator is NULL!");
            return;
        }

        Debug.Log($"FoxVisual: Animator is valid, setting trigger: {ATTACK}");

        // Запускаем анимацию атаки
        _animator.SetTrigger(ATTACK);
        Debug.Log("FoxVisual: Запущена анимация атаки");

        // Немедленная проверка состояния аниматора
        Debug.Log($"FoxVisual: Current state: {GetCurrentAnimatorState()}");

        // Сообщаем EnemyAttack о начале атаки
        if (_enemyAttack != null)
        {
            _enemyAttack.StartAttackFromVisual();
        }
        else
        {
            Debug.LogError("FoxVisual: EnemyAttack не назначен!");
        }
    }

    // Метод для получения текущего состояния аниматора
    private string GetCurrentAnimatorState()
    {
        if (_animator == null) return "No Animator";

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return $"Layer 0: {stateInfo.fullPathHash} (IsName Attack: {stateInfo.IsName(ATTACK)})";
    }

    // Вызывается в СЕРЕДИНЕ анимации атаки (через Animation Event)
    public void OnAttackAnimationMiddle()
    {
        Debug.Log("FoxVisual: Проверка попадания в середине атаки");

        // Просим EnemyAttack проверить попадание
        if (_enemyAttack != null)
        {
            _enemyAttack.CheckAttackHit();
        }
    }

    // Вызывается в КОНЦЕ анимации атаки (через Animation Event)  
    public void OnAttackAnimationEnd()
    {
        Debug.Log("FoxVisual: Анимация атаки завершена");

        if (_enemyAttack != null)
        {
            _enemyAttack.OnAttackFinished();
        }
    }

    // Вызывается из EnemyAttack при успешной атаке
    public void PlaySuccessfulAttack()
    {
        _animator.SetTrigger(ATTACK_SUCCESS);
        Debug.Log("FoxVisual: Запущена анимация успешной атаки");
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void UnsubscribeFromEvents()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnEnemyAttack -= OnEnemyAttackHandler;
        }
    }
}
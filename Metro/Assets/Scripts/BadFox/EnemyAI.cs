using Metro.Utils;
using UnityEngine;
using Unity.Collections;
using System;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private State _startingState;
    [SerializeField] private float _roamingDistanceMax = 7f;
    [SerializeField] private float _roamingDistanceMin = 3f;
    [SerializeField] private float _roamingTimerMax = 2f;
    [SerializeField] private bool _isChasingEnemy = false;
    [SerializeField] private float _chasingDistance = 10f;
    [SerializeField] private float _chasingSpeedMulti = 2f;
    [SerializeField] private bool _isAttackingEnemy = false;
    [SerializeField] private bool _reactToLight = true;
    private LightReaction _lightReaction;

    [SerializeField] private float _attackingDistance = 2f;
    private NavMeshAgent _agent;
    private State _currentstate;
    private float _roamingTime;
    private Vector3 _roamPos;
    private Vector3 _startingPos;
    private float _WalkingSpeed;
    private float _chasingSpeed;
    public event EventHandler OnEnemyAttack;
    private float _attackRating = 2f;
    private float _nextAttackTime = 0f;
    private float _checkDirection = 0f;
    private float _NextcheckDirection = 0.1f;
    private Vector2 _lastPosition;
    private Vector3 _lastFleePosition; // Запоминаем куда бежим
    [SerializeField] private EnemyAttack _enemyAttack;
    
    [Header("Memory System")]
    private Vector3 _lastKnownPlayerPosition;
    private float _searchTimer;
    [SerializeField] private float _searchWaitTime = 3f;
    private bool _wasInLightLastFrame = false;
    private bool _isPlayerInMeleeRange = false;

    public bool IsRunning
    {
        get
        {
            if (_agent.velocity == Vector3.zero)
            {
                return false;
            }
            else { return true; }
        }
    }

    public enum State
    {
        Idle,
        Roaming,
        chasing,
        Attacking,
        Death,
        Fleeing,
        Searching
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _currentstate = _startingState;

        _WalkingSpeed = _agent.speed;
        _chasingSpeed = _agent.speed * _chasingSpeedMulti;
        _lightReaction = GetComponent<LightReaction>();

        // Поиск EnemyAttack
        if (_enemyAttack == null)
        {
            _enemyAttack = GetComponent<EnemyAttack>();
            Debug.Log($"{gameObject.name}: EnemyAttack найден автоматически - {_enemyAttack != null}");
        }
        else
        {
            Debug.Log($"{gameObject.name}: EnemyAttack назначен в инспекторе");
        }
    }


    private void CheckCurrentState()
    {
        State previousState = _currentstate;
        State newState = CalculateNewState();

        if (newState != _currentstate)
        {
            _currentstate = newState;
            HandleStateTransition(previousState, newState);
        }
    }


    private State CalculateNewState()
    {
        if (Player1.Instance == null) return State.Roaming;

        // ПУНКТ 0: Если игрок физически вошел в триггер атаки (кусаем сразу)
        if (_isPlayerInMeleeRange)
        {
            return State.Attacking;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player1.Instance.transform.position);

        // 1. Проверка реакции на свет
        if (_reactToLight && _lightReaction != null)
        {
            Flashlight playerFlashlight = Player1.Instance.GetFlashlight();
            if (playerFlashlight != null && playerFlashlight.IsEnemyInLight(transform))
            {
                switch (_lightReaction.reactionType)
                {
                    case LightReaction.LightReactionType.Fear:
                        return State.Fleeing;
                    case LightReaction.LightReactionType.Aggressive:
                        // Агрессивный враг атакует если может, иначе преследует
                        return (distanceToPlayer <= _attackingDistance) ? State.Attacking : State.chasing;
                    case LightReaction.LightReactionType.Neutral:
                        break; // Продолжаем обычную логику
                }
            }
        }

        // 2. Обычная логика AI
        if (_isChasingEnemy)
        {
            if (distanceToPlayer <= _attackingDistance)
                return State.Attacking;
            else if (distanceToPlayer <= _chasingDistance)
                return State.chasing;
        }

        // 3. Проверка перехода в режим поиска, если свет пропал
        if (_wasInLightLastFrame && _currentstate == State.chasing)
        {
            // Если мы гнались за игроком из-за света, и свет пропал - идем искать
            return State.Searching;
        }

        // Если мы уже ищем, и не нашли игрока, продолжаем искать (пока не истечет время в stateHandler)
        if (_currentstate == State.Searching)
        {
            return State.Searching;
        }

        return State.Roaming;
    }

    private void Update()
    {
        State previousState = _currentstate;
        
        // Обновляем состояние света перед расчетами
        bool isInLight = false;
        if (Player1.Instance != null) {
            Flashlight playerFlashlight = Player1.Instance.GetFlashlight();
            if (playerFlashlight != null && playerFlashlight.IsEnemyInLight(transform)) {
                isInLight = true;
                _lastKnownPlayerPosition = Player1.Instance.transform.position;
            }
        }

        CheckCurrentState();
        stateHandler();
        MovementDirectionHandler();

        _wasInLightLastFrame = isInLight;

        // Отладка изменения состояний
        if (previousState != _currentstate)
        {
            Debug.Log($"{gameObject.name} state changed: {previousState} → {_currentstate}");
        }
    }

    private void HandleStateTransition(State fromState, State toState)
    {
        switch (toState)
        {
            case State.chasing:
                _agent.ResetPath();
                _agent.speed = _chasingSpeed;
                break;
            case State.Roaming:
                _roamingTime = 0f;
                _agent.speed = _WalkingSpeed;
                _agent.ResetPath(); // Останавливаемся когда перестаем бояться
                break;
            case State.Attacking:
                _agent.ResetPath();
                break;
            case State.Fleeing:
                _agent.speed = _WalkingSpeed * 1.5f;
                CalculateFleePosition(); // Сразу вычисляем куда бежать
                break;
            case State.Searching:
                _agent.speed = _WalkingSpeed;
                _agent.SetDestination(_lastKnownPlayerPosition);
                _searchTimer = 0f;
                break;
        }
    }

    private void stateHandler()
    {
        switch (_currentstate)
        {
            case State.Roaming:
                _roamingTime -= Time.deltaTime;
                if (_roamingTime < 0)
                {
                    Roaming();
                    _roamingTime = _roamingTimerMax;
                }
                break;
            case State.chasing:
                chasingTarget();
                break;
            case State.Attacking:
                AttackingTarget();
                break;
            case State.Fleeing:
                HandleFleeing();
                break;
            case State.Searching:
                HandleSearching();
                break;
            case State.Death:
                break;
            default:
            case State.Idle:
                break;
        }
    }

    private void HandleSearching()
    {
        // Если дошли до точки или почти дошли
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _searchTimer += Time.deltaTime;

            if (_searchTimer >= _searchWaitTime)
            {
                // Время вышло, возвращаемся в Roaming (CalculateNewState сделает это автоматом, если мы сбросим состояние)
                _currentstate = State.Roaming;
            }
        }
        else
        {
            // На всякий случай обновляем путь (хотя он задан в HandleStateTransition)
            _agent.SetDestination(_lastKnownPlayerPosition);
        }
    }

    private void HandleFleeing()
    {
        if (Player1.Instance == null) return;

        // Если уже достиг точки побега или игрок приблизился - вычисляем новую точку
        float distanceToFleePoint = Vector3.Distance(transform.position, _lastFleePosition);
        float distanceToPlayer = Vector3.Distance(transform.position, Player1.Instance.transform.position);

        if (distanceToFleePoint < 1f || distanceToPlayer < 3f)
        {
            CalculateFleePosition();
        }

        // Продолжаем бежать к текущей точке
        _agent.SetDestination(_lastFleePosition);
    }

    private void CalculateFleePosition()
    {
        if (Player1.Instance == null) return;

        // Бежим в противоположную от игрока сторону
        Vector3 directionFromPlayer = (transform.position - Player1.Instance.transform.position).normalized;

        // Добавляем немного случайности чтобы не бежать всегда прямо
        Vector3 randomDirection = new Vector3(
            directionFromPlayer.x + UnityEngine.Random.Range(-0.5f, 0.5f),
            directionFromPlayer.y + UnityEngine.Random.Range(-0.5f, 0.5f),
            0
        ).normalized;

        // Позиция для побега - на 8-12 единиц от текущей позиции
        Vector3 fleePosition = transform.position + randomDirection * UnityEngine.Random.Range(8f, 12f);

        // Убедимся что позиция доступна для NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, 5f, NavMesh.AllAreas))
        {
            _lastFleePosition = hit.position;
        }
        else
        {
            // Если не нашли валидную позицию, бежим в случайном направлении
            Vector3 randomFleePos = transform.position + Utils.GetRadnomDir() * 10f;
            if (NavMesh.SamplePosition(randomFleePos, out hit, 3f, NavMesh.AllAreas))
            {
                _lastFleePosition = hit.position;
            }
        }
    }

    private void MovementDirectionHandler()
    {
        if (Time.time > _NextcheckDirection)
        {
            Vector3 currentPosition = transform.position;

            // Определяем куда смотреть в зависимости от состояния
            if (_currentstate == State.chasing || _currentstate == State.Attacking || _currentstate == State.Fleeing)
            {
                // В боевых состояниях смотрим на игрока
                ChangeFacingDir(currentPosition, Player1.Instance.transform.position);
            }
            else if (IsRunning)
            {
                // В движении смотрим в направлении движения
                ChangeFacingDir(_lastPosition, currentPosition);
            }
            // В остальных случаях не меняем направление

            _lastPosition = currentPosition;
            _NextcheckDirection = Time.time + _checkDirection;
        }
    }

    private void AttackingTarget()
    {
        if (_enemyAttack != null && _enemyAttack.CanAttack())
        {
            Debug.Log("EnemyAI: Starting attack");

            // Проверяем есть ли подписчики
            if (OnEnemyAttack != null)
            {
                Debug.Log($"EnemyAI: Вызываю событие атаки. Подписчиков: {OnEnemyAttack.GetInvocationList().Length}");
                OnEnemyAttack?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Debug.LogError("EnemyAI: Нет подписчиков на событие атаки!");
            }

            _nextAttackTime = Time.time + _attackRating;
        }
    }

    private void chasingTarget()
    {
        _agent.SetDestination(Player1.Instance.transform.position);
    }

    public float GetRoamingAnimationSpeed()
    {
        return _agent.speed / _WalkingSpeed;
    }

    private void Roaming()
    {
        _startingPos = transform.position;
        _roamPos = GetRoamingPos();
        _agent.SetDestination(_roamPos);
    }

    private Vector3 GetRoamingPos()
    {
        return _startingPos + Utils.GetRadnomDir() * UnityEngine.Random.Range(_roamingDistanceMin, _roamingDistanceMax);
    }

    private void ChangeFacingDir(Vector3 sourcePos, Vector3 targetPos)
    {
        if (sourcePos.x > targetPos.x)
        {
            transform.rotation = Quaternion.Euler(0, -180, 0);
        }
        else transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    // Добавляем публичный метод для получения текущего состояния
    public State GetCurrentState()
    {
        return _currentstate;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInMeleeRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInMeleeRange = false;
        }
    }
}

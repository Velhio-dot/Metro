using UnityEngine;
using Unity.Collections;
using System;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private const string IS_RUNNING = "Ifrunning";
    private const string DIRECTION_X = "DirectionX";
    private const string DIRECTION_Y = "DirectionY";
    private const string HIT = "Hit"; // ← ДОБАВЬ
    private const string DEATH = "Die"; // ← ДОБАВЬ

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return;
        UpdatePlayerVisuals();
    }

    private void UpdatePlayerVisuals()
    {
        PlayerFacingDirection();
        UpdateAnimationParameters();
    }

    private void UpdateAnimationParameters()
    {
        animator.SetBool(IS_RUNNING, Player1.Instance.IsMoving);

        Vector2 lastDirection = Player1.Instance.LastMovementDirection;

        float directionX = 0f;
        float directionY = 0f;

        if (Mathf.Abs(lastDirection.x) >= Mathf.Abs(lastDirection.y))
        {
            directionX = Mathf.Sign(lastDirection.x);
            directionY = 0f;
        }
        else
        {
            directionX = 0f;
            directionY = Mathf.Sign(lastDirection.y);
        }

        animator.SetFloat(DIRECTION_X, directionX);
        animator.SetFloat(DIRECTION_Y, directionY);
    }

    private void PlayerFacingDirection()
    {
        Vector3 mousepose = GameInput.Instance.GetMousePosition();
        Vector3 playerposition = Player1.Instance.GetPlayerScreenPosition();
        //spriteRenderer.flipX = playerposition.x > mousepose.x;
    }

    // ↓↓↓ ДОБАВЬ ЭТИ МЕТОДЫ ↓↓↓

    public void PlayHitAnimation()
    {
        animator.SetTrigger(HIT);
        Debug.Log("PlayerVisual: Анимация получения урона");
    }

    public void PlayDeathAnimation()
    {
        animator.SetTrigger(DEATH);
        Debug.Log("PlayerVisual: Анимация смерти");
    }
}
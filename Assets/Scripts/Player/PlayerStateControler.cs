using System;
using System.Collections;
using System.Net;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    public enum State
    {
        Idle,
        Walk,
        Dash,
        Attack,
        Down,
        Jump,
        Hurt,
    }


    private State currentState;
    private SpriteRenderer spriteRenderer;
    private Animator animator=> Player.instance.animator;
    private Rigidbody2D rb; // ...existing code... (moved initialization to Start)
    private PlayerController playerController;

    [Header("需要用到的Animator")] [SerializeField]
    private AnimatorController[] _animators;

    public Action EndAttack;

    private void Start()
    {
        
        rb = Player.instance.rb; // cache rb here instead of at field init
        playerController = Player.instance.playerController; // cache playerController for shorter access
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Helper properties to make long expressions concise and readable
    private float MoveInputX => playerController != null ? playerController.moveInputVector2.x : 0f;

    private void OnEnable()
    {
        ChangeState(State.Idle);
    }

    private void OnDisable()
    {
        OnDisableAll();
    }

    public void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        PlayAnimationByState(newState);
    }

    private void PlayAnimationByState(State state)
    {
        OnDisableAll();
        animator.SetBool(state.ToString(), true);
    }

    private void OnDisableAll()
    {
        foreach (State state in Enum.GetValues(typeof(State)))
        {
            switch (state)
            {
                case State.Dash:
                case State.Attack:
                case State.Hurt:
                    continue;
            }

            animator.SetBool(state.ToString(), false);
        }
    }

    public State GetCurrentState()
    {
        return currentState;
    }

    public void DisableState(State state)
    {
        animator.SetBool(state.ToString(), false);
    }

    public void ChangeAnimator(bool havesword)
    {
        if (havesword)
        {
            animator.runtimeAnimatorController = _animators[1];
        }
        else
        {
            animator.runtimeAnimatorController = _animators[0];
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    #region Effect

    public enum Effect
    {
        SpeedDown,
    }

    public void PlayEffect(Effect effect, bool enable)
    {
        switch (effect)
        {
            case Effect.SpeedDown:
                Debug.Log("Effect SpeedDown: " + enable);
                break;
            default:
                break;
        }
    }

    #endregion

    public void OnEndAttack()
    {
        EndAttack?.Invoke();
    }

    public void HurtAnimation()
    {
        // 1. 动画
        ChangeState(State.Hurt);

        // 2. 闪白
        // StartCoroutine(Flash(spriteRenderer, 0.2f));
        Player.instance.playerStateController.ChangeState(State.Hurt);
    }
    
    
    /// <summary>
    /// 逻辑冲突，暂时弃用
    /// </summary>
    /// <param name="sr"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator Flash(SpriteRenderer sr, float time)
    {
        Debug.Log("Flash Coroutine Started");
        Color original = sr.color;
        sr.color = new Color(255f, 255f, 255f,0.1f); ;
        yield return new WaitForSeconds(time);
        sr.color = original;
    }
}
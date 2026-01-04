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
    }
    

    private State currentState;
    private Animator animator;
    private Rigidbody2D rb; // ...existing code... (moved initialization to Start)
    private PlayerController playerController;
    
    [Header("需要用到的Animator")]
    [SerializeField] private AnimatorController[] _animators;

    public Action EndAttack;

    private void Start()
    {
        animator = Player.instance.animator;
        rb = Player.instance.rb; // cache rb here instead of at field init
        playerController = Player.instance.playerController; // cache playerController for shorter access
    }

    // Helper properties to make long expressions concise and readable
    private float MoveInputX => playerController != null ? playerController.moveInputVector2.x : 0f;


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
            // 冲刺动画保证最高优先级，不会被其他动画打断
            if (state == State.Dash|| state == State.Attack)
            {
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
        Debug.Log("Disable State: " + state);
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
    }

    #region Effect

    public enum Effect
    {
        SpeedDown,
    }
    public void PlayEffect(Effect effect,bool enable)
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
    
    
    
}

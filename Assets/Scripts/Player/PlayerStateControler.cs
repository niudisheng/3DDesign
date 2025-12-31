using System;
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
    [Header("需要用到的Animator")]
    [SerializeField] private AnimatorController[] _animators;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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
        // Debug.Log("Player State: " + state);
        animator.SetBool(state.ToString(), true);
    }

    private void OnDisableAll()
    {
        foreach (State state in Enum.GetValues(typeof(State)))
        {
            // 冲刺动画保证最高优先级，不会被其他动画打断
            if (state == State.Dash)
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
}
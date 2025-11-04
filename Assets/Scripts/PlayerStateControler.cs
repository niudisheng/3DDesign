using System;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    public static PlayerStateController Instance;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        animator = GetComponent<Animator>();
        ChangeState(State.Idle);
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
}
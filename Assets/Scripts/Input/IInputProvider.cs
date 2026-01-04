using System;
using UnityEngine;

public interface IInputProvider
{
    Vector2 Move { get; }

    event Action<Vector2> OnMovePerformed;
    event Action OnMoveCanceled;

    event Action OnJumpPressed;
    event Action OnDashPressed;
    event Action OnInteractPressed;
    event Action OnAttackPressed;
}


using UnityEngine;


public abstract class SceneItem : MonoBehaviour
{
    protected Rigidbody2D Rb;

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
    }
}

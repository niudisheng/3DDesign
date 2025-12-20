using UnityEngine;

// Small test helper: attach to a GameObject in scene with Enemy sibling to call the new APIs.
public class EnemyTest : MonoBehaviour
{
    public Enemy enemy;
    public GameObject actor;

    void Start()
    {
        if (enemy == null)
        {
            Debug.LogWarning("EnemyTest: enemy not assigned");
            return;
        }

        if (actor == null)
        {
            actor = this.gameObject;
        }

        Debug.Log("EnemyTest: Calling OnPlayerEnter");
        enemy.OnPlayerEnter(actor);

        Debug.Log("EnemyTest: Calling Interact");
        enemy.Interact(actor);

        Debug.Log("EnemyTest: Calling Hurt");
        enemy.Hurt(actor);

        Debug.Log("EnemyTest: Calling OnPlayerExit");
        enemy.OnPlayerExit(actor);
    }
}


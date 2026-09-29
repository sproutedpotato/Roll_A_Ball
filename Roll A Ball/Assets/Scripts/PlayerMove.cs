using UnityEngine;
using Fusion;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private Rigidbody2D rb;

    [Networked]
    public int SpawnIndex { get; set; }

    private float x;

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<GameplayInput>(out var input))
            return;

        x = input.Horizontal;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        /**
        if (x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(x), 1, 1);
        }
        **/

        rb.linearVelocity = new Vector2(
            moveSpeed * x,
            rb.linearVelocity.y
        );
    }
}
using UnityEngine;
using Fusion;
using Unity.VisualScripting;

public class PlayerJump : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D playerCollider;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpHeight = 2.0f;
    [SerializeField] private float gravity = -20f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpClip;

    private bool isGrounded;
    private bool isAboveWall;

    private bool jumpRequested;
    private float verticalVelocity;
    private int jumpCount = 0;

    [Networked]
    private NetworkButtons PreviousButtons { get; set; }

    public int currentJumpCount => jumpCount;

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<GameplayInput>(out var input))
            return;

        if (input.Buttons.WasPressed(PreviousButtons, EInputButton.Jump))
        {
            jumpRequested = true;
        }

        PreviousButtons = input.Buttons;
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        CheckAboveWall();
        Jump();
        ApplyGravity();
    }

    private void CheckGrounded()
    {
        Bounds bounds = playerCollider.bounds;

        isGrounded = Physics2D.BoxCast(bounds.center, new Vector2(bounds.size.x * 0.9f, 0.1f), 0f, Vector2.down, bounds.extents.y, groundLayer);
    }

    private void CheckAboveWall()
    {
        Bounds bounds = playerCollider.bounds;

        isAboveWall = Physics2D.BoxCast(bounds.center, new Vector2(bounds.size.x * 0.9f, 0.1f), 0f, Vector2.up, bounds.extents.y + 0.2f, groundLayer);
    }

    private void Jump()
    {
        if (!jumpRequested)
            return;

        jumpRequested = false;

        if (jumpCount >= 2)
            return;

        if (jumpCount == 0 && isGrounded)
        {
            jumpCount += 1;

            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (jumpCount == 1 && !isGrounded)
        {
            jumpCount += 1;

            verticalVelocity = Mathf.Sqrt(jumpHeight * -1.5f * gravity);
        }

        audioSource.clip = jumpClip;
        audioSource.Play();
    }

    public void SetVerticalVelocity(float newVerticalVelocity)
    {
        verticalVelocity = newVerticalVelocity;
    }

    private void ApplyGravity()
    {
        if (isAboveWall && verticalVelocity > 0)
        {
            verticalVelocity = -0.1f;
        }

        if (isGrounded && verticalVelocity < 0.01f)
        {
            verticalVelocity = 0f;

            if (jumpCount > 0)
            {
                jumpCount = 0;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }

        verticalVelocity = Mathf.Max(verticalVelocity, -25f);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalVelocity);
    }
}
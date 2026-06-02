using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Input System이 자동으로 호출
    void OnMove(InputValue value)
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!canMove)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
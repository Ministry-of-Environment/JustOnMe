using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput;

    private bool canMove = true;

    // 마지막 방향 저장
    private Vector2 lastMoveDir = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Input System이 자동 호출
    void OnMove(InputValue value)
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();

        // 방향 입력이 있을 때만 마지막 방향 갱신
        if (moveInput != Vector2.zero)
        {
            lastMoveDir = moveInput;
        }
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;

            animator.SetBool("isWalk", false);

            return;
        }

        // 이동 처리
        rb.linearVelocity = moveInput.normalized * moveSpeed;

        // 이동 여부
        bool isWalking = moveInput != Vector2.zero;

        animator.SetBool("isWalk", isWalking);

        // 방향값 전달
        animator.SetFloat("DirectionX", lastMoveDir.x);
        animator.SetFloat("DirectionY", lastMoveDir.y);
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!canMove)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;

            animator.SetBool("isWalk", false);
        }
    }
}
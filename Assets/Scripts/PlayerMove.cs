using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMove : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Input System이 자동으로 호출해주는 함수
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}

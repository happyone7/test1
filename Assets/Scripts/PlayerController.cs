using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 6f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.6f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // groundLayer가 설정되지 않은 경우 Default 레이어 사용
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Default");
    }

    void Update()
    {
        // 바닥 체크
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // 이동 입력
        float h = Input.GetAxisRaw("Horizontal"); // A/D, 좌/우 방향키
        float v = Input.GetAxisRaw("Vertical");   // W/S, 상/하 방향키

        Vector3 moveDir = new Vector3(h, 0f, v).normalized;
        rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

        // 점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // ESC로 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}

using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("ジャンプ設定")]
    [SerializeField] private float jumpPower = 10f;

    private Rigidbody2D rb;

    // 地面にいるか
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Move();
        Jump();
    }

    // 左右移動
    private void Move()
    {
        float input = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            input * moveSpeed,
            rb.linearVelocity.y
        );

        // プレイヤーの向きを変更
        if (input > 0)
        {
            // 右向き
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (input < 0)
        {
            // 左向き
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    // ジャンプ
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpPower
            );

            isGrounded = false;
        }
    }

    // 地面に着いたらジャンプ可能
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
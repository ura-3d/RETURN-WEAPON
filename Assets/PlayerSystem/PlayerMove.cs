using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("ˆÚ“®")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("ƒWƒƒƒ“ƒv")]
    [SerializeField] private float jumpPower = 10f;

    private Rigidbody2D rb;

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

    private void Move()
    {
        float input = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            input * moveSpeed,
            rb.linearVelocity.y
        );

        //Œü‚«•ÏX
        if(input > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if(input < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
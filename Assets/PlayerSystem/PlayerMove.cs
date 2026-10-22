using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("ジャンプ設定")]
    [SerializeField] private float jumpPower = 10f;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;

    // 現在右向きか（他のスクリプトから参照できる）
    public bool IsFacingRight { get; private set; } = true;

    // 接触している地面を管理
    private HashSet<GameObject> groundObjects =
        new HashSet<GameObject>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 開始時の向きを、現在のスケールから決める
        IsFacingRight = transform.localScale.x >= 0f;
    }

    private void Update()
    {
        Move();
        Jump();
        UpdateFacing();
        UpdateAnimation();
    }

    // 移動
    private void Move()
    {
        float input = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            input * moveSpeed,
            rb.linearVelocity.y
        );
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

            groundObjects.Clear();
            isGrounded = false;
        }
    }

    // キー入力で向きを変える（A/D・矢印キー）
    private void UpdateFacing()
    {
        float input = Input.GetAxisRaw("Horizontal");

        // 入力があったときだけ向きを更新する（入力がなければ今の向きを保つ）
        if (input > 0f)
        {
            IsFacingRight = true;
        }
        else if (input < 0f)
        {
            IsFacingRight = false;
        }

        transform.localScale = new Vector3(
            IsFacingRight ? 1f : -1f,
            1f,
            1f
        );
    }

    // アニメーション更新
    private void UpdateAnimation()
    {
        if (animator == null || rb == null)
            return;

        bool isRunning =
            Mathf.Abs(rb.linearVelocity.x) > 0.01f;

        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsGrounded", isGrounded);
    }

    // 地面に接触したとき
    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateGroundCollision(collision);
    }

    // 地面に接触し続けているとき
    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateGroundCollision(collision);
    }

    // 地面から離れたとき
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
            return;

        groundObjects.Remove(collision.gameObject);
        isGrounded = groundObjects.Count > 0;
    }

    // 地面との接触状態を更新
    private void UpdateGroundCollision(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
            return;

        bool touchingTop = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                touchingTop = true;
                break;
            }
        }

        if (touchingTop && rb.linearVelocity.y <= 0.01f)
        {
            groundObjects.Add(collision.gameObject);
        }
        else if (rb.linearVelocity.y > 0.01f)
        {
            groundObjects.Remove(collision.gameObject);
        }

        isGrounded = groundObjects.Count > 0;
    }
}
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("飛行設定")]
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private float throwUpPower = 7f;

    [Header("戻る設定")]
    [SerializeField] private float returnSpeed = 12f;

    [Header("攻撃設定")]
    [SerializeField] private int damage = 10;

    [Header("壁反射設定")]
    [SerializeField] private float bounceTime = 0.2f;

    [Header("拾う設定")]
    [SerializeField] private float pickupDistance = 1.5f;


    private Rigidbody2D rb;

    // プレイヤー
    private Transform player;

    // 投げた状態
    private bool isReturning = false;

    // 壁に当たって反射している状態
    private bool isBouncing = false;

    // 床に落ちた状態
    private bool isOnGround = false;

    // 壁反射時間
    private float bounceTimer = 0f;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    // ========================================
    // 武器を投げる
    // ========================================
    public void Throw(Vector2 direction, Transform playerTransform)
    {
        player = playerTransform;

        isReturning = false;
        isBouncing = false;
        isOnGround = false;

        // 重力を有効にする
        rb.gravityScale = 3f;

        // 前方 + 上方向へ投げる
        Vector2 throwVelocity = new Vector2(
            direction.x * throwSpeed,
            throwUpPower
        );

        rb.linearVelocity = throwVelocity;
    }


    private void Update()
    {
        if (player == null)
            return;


        // ========================================
        // 床に落ちている
        // ========================================
        if (isOnGround)
        {
            // その場で停止
            rb.linearVelocity = Vector2.zero;

            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position
                );

            // プレイヤーが近い
            if (distance <= pickupDistance)
            {
                // Eキーで拾う
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickupWeapon();
                }
            }

            return;
        }


        // ========================================
        // 壁で反射中
        // ========================================
        if (isBouncing)
        {
            bounceTimer -= Time.deltaTime;

            if (bounceTimer <= 0f)
            {
                isBouncing = false;

                // 反射が終わったらプレイヤーへ戻る
                isReturning = true;

                // 戻るときは重力を無効
                rb.gravityScale = 0f;
            }

            return;
        }


        // ========================================
        // プレイヤーへ戻る
        // ========================================
        if (isReturning)
        {
            Vector2 direction =
                (player.position - transform.position).normalized;

            rb.linearVelocity =
                direction * returnSpeed;


            // プレイヤーまで戻った
            if (Vector2.Distance(
                transform.position,
                player.position
            ) < 0.5f)
            {
                Debug.Log("武器がプレイヤーに戻った");

                Destroy(gameObject);
            }

            return;
        }
    }


    // ========================================
    // 敵との当たり判定
    // ========================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 床に落ちている場合は無視
        if (isOnGround)
            return;


        // 敵に当たった
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("敵に命中");


            EnemyHP enemyHP =
                other.GetComponent<EnemyHP>();


            if (enemyHP != null)
            {
                enemyHP.TakeDamage(damage);
            }


            // 敵に当たったらプレイヤーへ戻る
            isReturning = true;

            // 戻るときは重力を無効
            rb.gravityScale = 0f;
        }
    }


    // ========================================
    // 壁・床・プレイヤーとの衝突
    // ========================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(
            "Weaponが衝突しました："
            + collision.gameObject.name
        );


        // ========================================
        // 床に落ちた
        // ========================================
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("武器が床に落ちました");


            isOnGround = true;

            isReturning = false;
            isBouncing = false;


            // その場で停止
            rb.linearVelocity = Vector2.zero;

            // 重力を無効
            rb.gravityScale = 0f;

            return;
        }


        // ========================================
        // 壁に当たった
        // ========================================
        if (collision.gameObject.CompareTag("wall"))
        {
            Debug.Log("壁に当たった！反射します");


            // 現在の移動方向
            Vector2 currentDirection =
                rb.linearVelocity.normalized;


            // 壁の法線
            Vector2 normal =
                collision.contacts[0].normal;


            // 反射方向を計算
            Vector2 reflectedDirection =
                Vector2.Reflect(
                    currentDirection,
                    normal
                );


            // 反射する
            rb.linearVelocity =
                reflectedDirection.normalized
                * throwSpeed;


            // すぐには戻さない
            isReturning = false;

            // 反射中
            isBouncing = true;

            bounceTimer = bounceTime;

            return;
        }


        // ========================================
        // 戻ってきた武器がプレイヤーに当たった
        // ========================================
        if (isReturning &&
            collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("武器がプレイヤーに戻った");

            Destroy(gameObject);

            return;
        }
    }


    // ========================================
    // 武器を拾う
    // ========================================
    private void PickupWeapon()
    {
        Debug.Log("武器を拾いました");

        Destroy(gameObject);
    }
}
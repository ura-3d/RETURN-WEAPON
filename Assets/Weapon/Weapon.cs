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

    // 状態
    private bool isReturning = false;
    private bool isBouncing = false;
    private bool isOnGround = false;

    // 壁反射時間
    private float bounceTimer = 0f;


    // ========================================
    // 初期化
    // ========================================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    // ========================================
    // 武器を投げる
    // ========================================
    public void Throw(
        Vector2 direction,
        Transform playerTransform)
    {
        player = playerTransform;

        // 状態をリセット
        isReturning = false;
        isBouncing = false;
        isOnGround = false;
        bounceTimer = 0f;

        // Rigidbodyを有効化
        rb.simulated = true;

        // 重力を有効化
        rb.gravityScale = 3f;

        // 前方 + 上方向へ投げる
        Vector2 throwVelocity =
            new Vector2(
                direction.x * throwSpeed,
                throwUpPower
            );

        rb.linearVelocity =
            throwVelocity;
    }


    // ========================================
    // 毎フレーム処理
    // ========================================
    private void Update()
    {
        if (player == null)
            return;


        // ====================================
        // 床に落ちている
        // ====================================
        if (isOnGround)
        {
            rb.linearVelocity =
                Vector2.zero;

            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position
                );

            // プレイヤーが近づいた
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


        // ====================================
        // 壁で反射中
        // ====================================
        if (isBouncing)
        {
            bounceTimer -=
                Time.deltaTime;

            // 反射時間終了
            if (bounceTimer <= 0f)
            {
                isBouncing = false;

                // プレイヤーへ戻る
                isReturning = true;

                // 戻るときは重力OFF
                rb.gravityScale = 0f;
            }

            return;
        }


        // ====================================
        // プレイヤーへ戻る
        // ====================================
        if (isReturning)
        {
            Vector2 direction =
                (
                    player.position -
                    transform.position
                ).normalized;

            // プレイヤー方向へ移動
            rb.linearVelocity =
                direction * returnSpeed;

            // プレイヤーに十分近づいた
            if (Vector2.Distance(
                transform.position,
                player.position
            ) < 0.5f)
            {
                ReturnToPlayer();

                return;
            }
        }
    }


    // ========================================
    // 衝突処理
    // ========================================
    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        Debug.Log(
            "Weaponが衝突しました："
            + collision.gameObject.name
        );


        // ====================================
        // 敵
        // ====================================

        // EnemyHPを親も含めて探す
        EnemyHP enemyHP =
            collision.gameObject
            .GetComponentInParent<EnemyHP>();

        if (enemyHP != null)
        {
            // 床に落ちている場合は攻撃しない
            if (isOnGround)
                return;

            Debug.Log("敵に命中！");

            // ダメージ
            enemyHP.TakeDamage(damage);

            // プレイヤーへ戻る
            isReturning = true;

            // 戻るときは重力OFF
            rb.gravityScale = 0f;

            return;
        }


        // ====================================
        // 床
        // ====================================
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log(
                "武器が床に落ちました"
            );

            isOnGround = true;
            isReturning = false;
            isBouncing = false;

            // その場で停止
            rb.linearVelocity =
                Vector2.zero;

            // 重力OFF
            rb.gravityScale = 0f;

            return;
        }


        // ====================================
        // 壁
        // ====================================
        if (collision.gameObject.CompareTag("wall"))
        {
            Debug.Log(
                "壁に当たった！反射します"
            );

            // 現在の移動方向
            Vector2 currentDirection =
                rb.linearVelocity.normalized;

            // 壁の法線
            Vector2 normal =
                collision.contacts[0].normal;

            // 反射方向
            Vector2 reflectedDirection =
                Vector2.Reflect(
                    currentDirection,
                    normal
                );

            // 反射
            rb.linearVelocity =
                reflectedDirection.normalized
                * throwSpeed;

            // 一時的に反射状態
            isReturning = false;
            isBouncing = true;

            bounceTimer =
                bounceTime;

            return;
        }


        // ====================================
        // プレイヤー
        // ====================================
        if (isReturning &&
            collision.gameObject.CompareTag("Player"))
        {
            ReturnToPlayer();

            return;
        }
    }


    // ========================================
    // 武器を拾う
    // ========================================
    private void PickupWeapon()
    {
        Debug.Log("武器を拾います");

        if (player == null)
            return;

        WeaponSpawner spawner =
            player.GetComponent<WeaponSpawner>();

        if (spawner != null)
        {
            spawner.EquipWeapon(
                gameObject
            );
        }

        // 状態をリセット
        isOnGround = false;
        isReturning = false;
        isBouncing = false;

        rb.linearVelocity =
            Vector2.zero;

        rb.gravityScale = 0f;
    }


    // ========================================
    // プレイヤーへ戻った
    // ========================================
    private void ReturnToPlayer()
    {
        Debug.Log(
            "武器がプレイヤーに戻りました"
        );

        if (player == null)
            return;

        WeaponSpawner spawner =
            player.GetComponent<WeaponSpawner>();

        if (spawner != null)
        {
            spawner.EquipWeapon(
                gameObject
            );
        }

        // 状態をリセット
        isReturning = false;
        isBouncing = false;
        isOnGround = false;
    }
}
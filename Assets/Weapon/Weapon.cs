using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("飛行設定")]
    [SerializeField] private float throwSpeed = 10f;

    [Header("戻る設定")]
    [SerializeField] private float returnSpeed = 12f;

    [Header("攻撃設定")]
    [SerializeField] private int damage = 10;

    [Header("壁反射設定")]
    [SerializeField] private float bounceTime = 0.2f;

    [Header("拾う設定")]
    [SerializeField] private float pickupDistance = 1.5f;

    [Header("落下設定")]
    // このY座標より下に落ちたら、地面に戻す
    [SerializeField] private float fallLimitY = -10f;

    // 地面に戻すとき、地面から少し上に置く（めり込み防止）
    [SerializeField] private float landingHeightOffset = 0.5f;

    // 戻し先の地面を探す距離（真下方向）
    [SerializeField] private float groundSearchDistance = 50f;

    private Rigidbody2D rb;
    private Transform player;

    private bool isReturning;
    private bool isBouncing;
    private bool isOnGround;

    // 投げられて飛行中かどうか
    private bool isFlying;

    // 投げた位置（戻し先が見つからないときの予備）
    private Vector2 throwOrigin;

    private float bounceTimer;

    private HashSet<EnemyHP> hitEnemies =
        new HashSet<EnemyHP>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    // 武器を投げる
    public void Throw(
        Vector2 direction,
        Transform playerTransform)
    {
        if (rb == null)
            return;

        player = playerTransform;

        isReturning = false;
        isBouncing = false;
        isOnGround = false;
        isFlying = true;
        bounceTimer = 0f;

        // 投げた位置を記録
        throwOrigin = transform.position;

        hitEnemies.Clear();

        rb.simulated = true;
        rb.gravityScale = 3f;

        // Enemyとの衝突を有効にする
        SetEnemyCollision(true);

        Vector2 throwDirection =
            direction.normalized;

        rb.linearVelocity =
            throwDirection * throwSpeed;
    }

    private void Update()
    {
        if (player == null)
            return;

        // 飛行方向に武器を向ける
        UpdateWeaponRotation();

        // 穴に落ちたら地面に戻す
        if (CheckFall())
            return;

        if (isOnGround)
        {
            rb.linearVelocity =
                Vector2.zero;

            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position
                );

            if (distance <= pickupDistance)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickupWeapon();
                }
            }

            return;
        }

        // 壁に当たった後の反射
        if (isBouncing)
        {
            bounceTimer -=
                Time.deltaTime;

            if (bounceTimer <= 0f)
            {
                isBouncing = false;
                isReturning = true;
                rb.gravityScale = 0f;
            }

            return;
        }

        // Playerへ戻る
        if (isReturning)
        {
            Vector2 direction =
                (
                    player.position -
                    transform.position
                ).normalized;

            rb.linearVelocity =
                direction * returnSpeed;

            if (Vector2.Distance(
                transform.position,
                player.position
            ) < 0.5f)
            {
                ReturnToPlayer();
            }
        }
    }

    // 穴に落ちたかチェックして、落ちていたら地面に戻す
    // 戻した場合は true を返す
    private bool CheckFall()
    {
        // 飛行中で、まだ地面に落ちておらず、戻っている最中でもないときだけ判定
        if (!isFlying || isOnGround || isReturning)
            return false;

        if (transform.position.y >= fallLimitY)
            return false;

        LandOnGroundNearPlayer();
        return true;
    }

    // Playerの近くの地面に武器を置く
    private void LandOnGroundNearPlayer()
    {
        Vector2 landingPosition;

        // まずPlayerの真下の地面を探す
        if (!TryFindGroundBelow(player.position, out landingPosition))
        {
            // 見つからなければ、投げた位置の真下を探す
            if (!TryFindGroundBelow(throwOrigin, out landingPosition))
            {
                // それでも無ければ、投げた位置に戻す
                landingPosition = throwOrigin;
            }
        }

        Debug.Log(
            "武器が穴に落ちたので地面に戻します"
        );

        transform.position = new Vector3(
            landingPosition.x,
            landingPosition.y + landingHeightOffset,
            transform.position.z
        );

        // 床に落ちたときと同じ状態にする（Eキーで拾える）
        isOnGround = true;
        isFlying = false;
        isReturning = false;
        isBouncing = false;

        rb.linearVelocity =
            Vector2.zero;

        rb.gravityScale = 0f;
        rb.rotation = 0f;

        // Enemyとの衝突を無効にする
        SetEnemyCollision(false);
    }

    // 指定位置の真下にある Ground タグの地面を探す
    private bool TryFindGroundBelow(
        Vector2 origin,
        out Vector2 groundPoint)
    {
        RaycastHit2D[] hits =
            Physics2D.RaycastAll(
                origin,
                Vector2.down,
                groundSearchDistance
            );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider.CompareTag("Ground"))
            {
                groundPoint = hit.point;
                return true;
            }
        }

        groundPoint = Vector2.zero;
        return false;
    }

    // 飛行方向に武器を向ける
    private void UpdateWeaponRotation()
    {
        if (rb == null)
            return;

        if (rb.linearVelocity.sqrMagnitude < 0.01f)
            return;

        float targetAngle =
            Mathf.Atan2(
                rb.linearVelocity.y,
                rb.linearVelocity.x
            ) * Mathf.Rad2Deg;

        Quaternion targetRotation =
            Quaternion.Euler(
                0f,
                0f,
                targetAngle
            );

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                15f * Time.deltaTime
            );
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        Debug.Log(
            "Weaponが衝突しました：" +
            collision.gameObject.name
        );

        // 敵に当たった
        EnemyHP enemyHP =
            collision.gameObject
            .GetComponentInParent<EnemyHP>();

        if (enemyHP != null)
        {
            if (isOnGround)
                return;

            if (!hitEnemies.Contains(enemyHP))
            {
                hitEnemies.Add(enemyHP);

                Debug.Log(
                    "敵に命中！"
                );

                enemyHP.TakeDamage(
                    damage
                );
            }

            isReturning = true;
            isBouncing = false;

            rb.gravityScale = 0f;

            return;
        }

        // 地面に落ちた
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log(
                "武器が床に落ちました"
            );

            isOnGround = true;
            isFlying = false;
            isReturning = false;
            isBouncing = false;

            rb.linearVelocity =
                Vector2.zero;

            rb.gravityScale = 0f;

            // Enemyとの衝突を無効にする
            SetEnemyCollision(false);

            return;
        }

        // 壁に当たった
        if (collision.gameObject.CompareTag("wall"))
        {
            Debug.Log(
                "壁に当たった！反射します"
            );

            Vector2 currentDirection;

            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                currentDirection =
                    rb.linearVelocity.normalized;
            }
            else
            {
                currentDirection =
                    Vector2.right;
            }

            Vector2 normal =
                collision.contacts[0].normal;

            Vector2 reflectedDirection =
                Vector2.Reflect(
                    currentDirection,
                    normal
                );

            rb.gravityScale = 0f;

            rb.linearVelocity =
                reflectedDirection.normalized
                * throwSpeed;

            isReturning = false;
            isBouncing = true;

            bounceTimer =
                bounceTime;

            return;
        }

        // 戻っている武器がPlayerに当たった
        if (isReturning &&
            collision.gameObject.CompareTag("Player"))
        {
            ReturnToPlayer();

            return;
        }
    }

    // Enemyとの衝突設定
    private void SetEnemyCollision(bool enabled)
    {
        Collider2D weaponCollider =
            GetComponent<Collider2D>();

        if (weaponCollider == null)
            return;

        EnemyHP[] enemies =
            FindObjectsByType<EnemyHP>(
                FindObjectsSortMode.None
            );

        foreach (EnemyHP enemy in enemies)
        {
            if (enemy == null)
                continue;

            Collider2D[] enemyColliders =
                enemy.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D enemyCollider in enemyColliders)
            {
                if (enemyCollider == null)
                    continue;

                Physics2D.IgnoreCollision(
                    weaponCollider,
                    enemyCollider,
                    !enabled
                );
            }
        }
    }

    // 武器を拾う
    private void PickupWeapon()
    {
        Debug.Log(
            "武器を拾います"
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

        ResetWeaponState();
    }

    // 武器がPlayerに戻る
    private void ReturnToPlayer()
    {
        Debug.Log(
            "武器がPlayerに戻りました"
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

        ResetWeaponState();
    }

    // 武器の状態をリセット
    private void ResetWeaponState()
    {
        isReturning = false;
        isBouncing = false;
        isOnGround = false;
        isFlying = false;
        bounceTimer = 0f;

        rb.linearVelocity =
            Vector2.zero;

        rb.gravityScale = 0f;

        rb.rotation = 0f;

        // Enemyとの衝突を再び有効にする
        SetEnemyCollision(true);

        hitEnemies.Clear();
    }

    // Sceneビューで落下ラインを赤線で表示（このオブジェクトを選択中のみ）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-1000f, fallLimitY, 0f),
            new Vector3(1000f, fallLimitY, 0f)
        );
    }
}
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("飛行設定")]
    [SerializeField] private float throwSpeed = 10f;

    // 投げているときの重力の強さ
    [SerializeField] private float throwGravityScale = 3f;

    // 狙った位置へ投げるときの、最短の飛行時間（近すぎる位置で速度が極端になるのを防ぐ）
    [SerializeField] private float minFlightTime = 0.15f;

    [Header("戻る設定")]
    [SerializeField] private float returnSpeed = 12f;

    // 戻り始めてからこの秒数が過ぎたら、引っかかっていても強制的に手元へ戻す
    [SerializeField] private float returnTimeout = 3f;

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

    // 地面に置かれた武器が空中で止まっていないかを調べるときの余裕
    [SerializeField] private float groundCheckMargin = 0.2f;

    // 戻っている間は、地面との衝突を無視する時間（戻り切るまで十分長く）
    private const float ReturnIgnoreGroundTime = 60f;

    private Rigidbody2D rb;
    private Transform player;

    private bool isReturning;
    private bool isBouncing;
    private bool isOnGround;

    // 投げられて飛行中かどうか
    private bool isFlying;

    // 投げた位置（戻し先が見つからないときの予備）
    private Vector2 throwOrigin;

    // 狙った位置へ飛んでいる最中か（この間は地面との衝突を無視する）
    private bool isTargetFlight;
    private float targetFlightTimer;

    // 戻っている時間（引っかかって戻れなくなったときの保険）
    private float returnTimer;

    // 衝突を無視している地面のコライダー
    private List<Collider2D> ignoredGroundColliders =
        new List<Collider2D>();

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
        rb.gravityScale = throwGravityScale;

        // Enemyとの衝突を有効にする
        SetEnemyCollision(true);

        Vector2 throwDirection =
            direction.normalized;

        rb.linearVelocity =
            throwDirection * throwSpeed;
    }

    // 狙った位置（レティクルの位置）に届くように投げる
    public void ThrowToTarget(
        Vector2 targetPosition,
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
        rb.gravityScale = throwGravityScale;

        // Enemyとの衝突を有効にする
        SetEnemyCollision(true);

        Vector2 startPosition = transform.position;
        Vector2 toTarget = targetPosition - startPosition;
        float distance = toTarget.magnitude;

        // 飛行時間：距離 ÷ 投げる速度（短すぎる場合は最小時間にする）
        float flightTime =
            Mathf.Max(minFlightTime, distance / throwSpeed);

        // 重力による加速度（下向きなので負の値）
        float gravityY =
            Physics2D.gravity.y * throwGravityScale;

        // 飛行時間ちょうどで目標の位置を通るような初速を求める
        float velocityX = toTarget.x / flightTime;
        float velocityY =
            toTarget.y / flightTime
            - 0.5f * gravityY * flightTime;

        rb.linearVelocity =
            new Vector2(velocityX, velocityY);

        // 狙った位置に着くまでは、地面に当たっても着地扱いにしない
        BeginIgnoreGround(flightTime);

        Debug.Log(
            "狙った位置へ投げます。飛行時間：" + flightTime
        );
    }

    private void Update()
    {
        if (player == null)
            return;

        // 飛行方向に武器を向ける
        UpdateWeaponRotation();

        // 狙った位置に着いたら、地面との衝突を元に戻す
        if (isTargetFlight)
        {
            targetFlightTimer -= Time.deltaTime;

            if (targetFlightTimer <= 0f)
            {
                RestoreGroundCollision();
            }
        }

        // 穴に落ちたら地面に戻す
        if (CheckFall())
            return;

        if (isOnGround)
        {
            // 空中で止まっていたら落下させる
            if (!HasGroundBelow())
            {
                DropFromAir();
                return;
            }

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

                // 戻っている間は地面に引っかからないようにする
                BeginIgnoreGround(ReturnIgnoreGroundTime);
            }

            return;
        }

        // Playerへ戻る
        if (isReturning)
        {
            returnTimer += Time.deltaTime;

            // 地形などに引っかかって戻れないときは、強制的に手元へ戻す
            if (returnTimer >= returnTimeout)
            {
                Debug.Log(
                    "武器が戻れなかったため、強制的に手元へ戻します"
                );

                ReturnToPlayer();
                return;
            }

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

    // 地面に触れ続けているとき
    // （側面を滑り落ちて床に着いたときなど、Enterだけでは着地を拾えないため）
    private void OnCollisionStay2D(
        Collision2D collision)
    {
        if (!isFlying || isOnGround)
            return;

        if (!collision.gameObject.CompareTag("Ground"))
            return;

        TryLandOnGround(collision);
    }

    // 地面に着地する（上向きの面に触れたときだけ）
    private void TryLandOnGround(Collision2D collision)
    {
        if (isOnGround)
            return;

        bool touchingFloor = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                touchingFloor = true;
                break;
            }
        }

        // 側面や下面に当たっただけのときは着地扱いにしない
        // （空中で止まってしまうのを防ぐ）
        if (!touchingFloor)
            return;

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
    }

    // 真下の近くに地面があるか
    private bool HasGroundBelow()
    {
        Collider2D weaponCollider =
            GetComponent<Collider2D>();

        if (weaponCollider == null)
            return true;

        Bounds bounds = weaponCollider.bounds;

        float distance =
            bounds.extents.y + groundCheckMargin;

        RaycastHit2D[] hits =
            Physics2D.RaycastAll(
                bounds.center,
                Vector2.down,
                distance
            );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider == weaponCollider)
                continue;

            if (hit.collider.CompareTag("Ground"))
                return true;
        }

        return false;
    }

    // 空中で止まっていた武器を落下させる
    private void DropFromAir()
    {
        Debug.Log(
            "武器が空中で止まっていたので、落下させます"
        );

        isOnGround = false;
        isFlying = true;
        isReturning = false;
        isBouncing = false;

        rb.gravityScale = throwGravityScale;
    }

    // 指定した時間のあいだ、Groundタグの地面との衝突を無視する
    private void BeginIgnoreGround(float duration)
    {
        // 念のため、前回の設定を戻しておく
        RestoreGroundCollision();

        Collider2D weaponCollider =
            GetComponent<Collider2D>();

        if (weaponCollider == null)
            return;

        GameObject[] grounds =
            GameObject.FindGameObjectsWithTag("Ground");

        foreach (GameObject ground in grounds)
        {
            Collider2D[] colliders =
                ground.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D groundCollider in colliders)
            {
                if (groundCollider == null)
                    continue;

                Physics2D.IgnoreCollision(
                    weaponCollider,
                    groundCollider,
                    true
                );

                ignoredGroundColliders.Add(groundCollider);
            }
        }

        isTargetFlight = true;
        targetFlightTimer = duration;
    }

    // 無視していた地面との衝突を元に戻す
    private void RestoreGroundCollision()
    {
        Collider2D weaponCollider =
            GetComponent<Collider2D>();

        if (weaponCollider != null)
        {
            foreach (Collider2D groundCollider in ignoredGroundColliders)
            {
                if (groundCollider == null)
                    continue;

                Physics2D.IgnoreCollision(
                    weaponCollider,
                    groundCollider,
                    false
                );
            }
        }

        ignoredGroundColliders.Clear();
        isTargetFlight = false;
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

            // 戻っている間は地面に引っかからないよう、地面との衝突を無視する
            BeginIgnoreGround(ReturnIgnoreGroundTime);

            isReturning = true;
            isBouncing = false;

            rb.gravityScale = 0f;

            return;
        }

        // 地面に落ちた
        if (collision.gameObject.CompareTag("Ground"))
        {
            TryLandOnGround(collision);
            return;
        }

        // 壁に当たった
        if (collision.gameObject.CompareTag("wall"))
        {
            RestoreGroundCollision();

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
        RestoreGroundCollision();

        returnTimer = 0f;

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
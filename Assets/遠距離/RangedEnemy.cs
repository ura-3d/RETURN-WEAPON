using UnityEngine;

public class RangedEnemy : EnemyBase
{
    protected RangedEnemyData m_rangedData;

    [Header("遠距離攻撃")]
    [SerializeField] private Transform m_shootPoint;
    [SerializeField] private GameObject m_projectilePrefab;

    [Header("行動パターン")]
    [SerializeField] private float m_minAttackDistance = 4f;
    [SerializeField] private float m_recoveryTime = 0.5f;

    private float m_attackTimer;

    private bool m_isAttacking;
    private bool m_isRecovering;

    private float m_recoveryTimer;

    protected override void Awake()
    {
        base.Awake();

        m_rangedData =
            m_data as RangedEnemyData;

        if (m_rangedData == null)
        {
            Debug.LogError(
                "RangedEnemyにRangedEnemyDataが設定されていません。"
            );
        }
    }

    protected override void EnemyUpdate()
    {
        if (m_rangedData == null)
            return;

        if (m_player == null)
            return;

        // =================================
        // 攻撃中
        // =================================
        if (m_isAttacking)
        {
            ChangeState(
                EnemyAIState.Attack
            );

            StopMove();

            return;
        }

        // =================================
        // 攻撃後の硬直
        // =================================
        if (m_isRecovering)
        {
            ChangeState(
                EnemyAIState.Recovery
            );

            StopMove();

            m_recoveryTimer -=
                Time.deltaTime;

            if (m_recoveryTimer <= 0f)
            {
                m_isRecovering = false;

                Debug.Log(
                    "遠距離敵の硬直終了"
                );
            }

            return;
        }

        // =================================
        // 攻撃クールタイム
        // =================================
        if (m_attackTimer > 0f)
        {
            m_attackTimer -=
                Time.deltaTime;
        }

        float distance =
            GetPlayerDistance();

        // =================================
        // 検知範囲外
        // =================================
        if (distance >
            m_rangedData.detectDistance)
        {
            ChangeState(
                EnemyAIState.Return
            );

            MoveToStartPosition(
                m_rangedData.moveSpeed
            );

            // 初期位置に戻った
            if (IsAtStartPosition())
            {
                StopMove();

                ChangeState(
                    EnemyAIState.Idle
                );

                Debug.Log(
                    "遠距離敵が初期位置へ戻りました。"
                );
            }

            return;
        }

        // =================================
        // Return中
        // =================================
        if (m_currentState ==
            EnemyAIState.Return)
        {
            // Playerが再び検知範囲に入った
            if (distance <=
                m_rangedData.detectDistance)
            {
                ChangeState(
                    EnemyAIState.Chase
                );

                return;
            }

            MoveToStartPosition(
                m_rangedData.moveSpeed
            );

            if (IsAtStartPosition())
            {
                StopMove();

                ChangeState(
                    EnemyAIState.Idle
                );

                Debug.Log(
                    "遠距離敵が初期位置へ戻りました。"
                );
            }

            return;
        }

        // =================================
        // 近すぎる
        // =================================
        if (distance <
            m_minAttackDistance)
        {
            ChangeState(
                EnemyAIState.Chase
            );

            MoveAwayFromPlayer();

            return;
        }

        // =================================
        // 射撃可能距離
        // =================================
        if (distance <=
            m_rangedData.attackDistance)
        {
            ChangeState(
                EnemyAIState.Attack
            );

            StopMove();

            Vector2 direction =
                GetPlayerDirection();

            LookAtPlayer(direction);

            if (m_attackTimer <= 0f)
            {
                Shoot();
            }

            return;
        }

        // =================================
        // 射撃距離まで近づく
        // =================================
        ChangeState(
            EnemyAIState.Chase
        );

        MoveToPlayer();
    }

    // =================================
    // プレイヤーへ近づく
    // =================================
    private void MoveToPlayer()
    {
        Vector2 direction =
            GetPlayerDirection();

        m_rb.linearVelocity =
            new Vector2(
                direction.x *
                m_rangedData.moveSpeed,
                m_rb.linearVelocity.y
            );

        LookAtPlayer(direction);
    }

    // =================================
    // プレイヤーから離れる
    // =================================
    private void MoveAwayFromPlayer()
    {
        Vector2 direction =
            GetPlayerDirection();

        Vector2 moveDirection =
            -direction;

        m_rb.linearVelocity =
            new Vector2(
                moveDirection.x *
                m_rangedData.moveSpeed,
                m_rb.linearVelocity.y
            );

        LookAtPlayer(direction);
    }

    // =================================
    // 移動停止
    // =================================
    private void StopMove()
    {
        m_rb.linearVelocity =
            new Vector2(
                0f,
                m_rb.linearVelocity.y
            );
    }

    // =================================
    // 射撃
    // =================================
    private void Shoot()
    {
        if (m_shootPoint == null)
        {
            Debug.LogError(
                "ShootPointが設定されていません！"
            );

            return;
        }

        if (m_projectilePrefab == null)
        {
            Debug.LogError(
                "Projectile Prefabが設定されていません！"
            );

            return;
        }

        Vector2 direction =
            GetPlayerDirection();

        GameObject projectile =
            Instantiate(
                m_projectilePrefab,
                m_shootPoint.position,
                Quaternion.identity
            );

        EnemyProjectile enemyProjectile =
            projectile.GetComponent<EnemyProjectile>();

        if (enemyProjectile != null)
        {
            enemyProjectile.Initialize(
                direction,
                m_rangedData.projectileSpeed,
                m_rangedData.projectileLifeTime,
                m_rangedData.attackDamage
            );
        }

        Debug.Log(
            "遠距離敵が弾を発射！"
        );

        // クールタイム開始
        m_attackTimer =
            m_rangedData.attackInterval;

        // 攻撃状態終了
        m_isAttacking = false;

        // Recovery開始
        m_isRecovering = true;

        m_recoveryTimer =
            m_recoveryTime;

        Debug.Log(
            "遠距離敵が射撃 → Recovery"
        );
    }

    // =================================
    // オブジェクト無効化時
    // =================================
    private void OnDisable()
    {
        CancelInvoke();
    }
}
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

    [Header("アニメーション")]
    [SerializeField] private float m_attackAnimationTime = 0.3f;

    private Animator m_animator;

    private float m_attackTimer;
    private float m_recoveryTimer;
    private float m_attackAnimationTimer;

    private bool m_isAttacking;
    private bool m_isRecovering;

    private Vector2 m_startPosition;

    protected override void Awake()
    {
        base.Awake();

        m_rangedData = m_data as RangedEnemyData;
        m_animator = GetComponent<Animator>();

        m_startPosition = transform.position;

        if (m_rangedData == null)
        {
            Debug.LogError(
                "RangedEnemyにRangedEnemyDataが設定されていません。"
            );
        }

        if (m_animator == null)
        {
            Debug.LogWarning(
                "RangedEnemyにAnimatorが設定されていません。"
            );
        }
    }

    protected override void EnemyUpdate()
    {
        if (m_rangedData == null || m_player == null)
            return;

        UpdateAttackAnimationTimer();

        if (m_isAttacking)
        {
            ChangeState(EnemyAIState.Attack);
            StopMove();
            UpdateAnimation();
            return;
        }

        if (m_isRecovering)
        {
            ChangeState(EnemyAIState.Recovery);
            StopMove();

            m_recoveryTimer -= Time.deltaTime;

            if (m_recoveryTimer <= 0f)
            {
                m_isRecovering = false;
                Debug.Log("遠距離敵の硬直終了");
            }

            UpdateAnimation();
            return;
        }

        if (m_attackTimer > 0f)
        {
            m_attackTimer -= Time.deltaTime;
        }

        float distance = GetPlayerDistance();

        // 検知範囲外なら初期位置へ戻る
        if (distance > m_rangedData.detectDistance)
        {
            ChangeState(EnemyAIState.Return);
            MoveToStartPosition();

            if (IsAtStartPosition())
            {
                StopMove();
                ChangeState(EnemyAIState.Idle);
            }

            UpdateAnimation();
            return;
        }

        // 初期位置へ帰還中
        if (m_currentState == EnemyAIState.Return)
        {
            ChangeState(EnemyAIState.Chase);
        }

        // プレイヤーが近すぎる場合は後退する
        if (distance < m_minAttackDistance)
        {
            ChangeState(EnemyAIState.Chase);
            MoveAwayFromPlayer();

            UpdateAnimation();
            return;
        }

        // 射撃可能距離
        if (distance <= m_rangedData.attackDistance)
        {
            ChangeState(EnemyAIState.Attack);
            StopMove();

            Vector2 direction = GetPlayerDirection();
            LookAtPlayer(direction);

            if (m_attackTimer <= 0f)
            {
                Shoot();
            }

            UpdateAnimation();
            return;
        }

        // 射撃距離まで近づく
        ChangeState(EnemyAIState.Chase);
        MoveToPlayer();

        UpdateAnimation();
    }

    // プレイヤーへ近づく
    private void MoveToPlayer()
    {
        Vector2 direction = GetPlayerDirection();

        m_rb.linearVelocity = new Vector2(
            direction.x * m_rangedData.moveSpeed,
            m_rb.linearVelocity.y
        );

        LookAtPlayer(direction);
    }

    // プレイヤーから離れる
    private void MoveAwayFromPlayer()
    {
        Vector2 direction = GetPlayerDirection();
        Vector2 moveDirection = -direction;

        m_rb.linearVelocity = new Vector2(
            moveDirection.x * m_rangedData.moveSpeed,
            m_rb.linearVelocity.y
        );

        LookAtPlayer(direction);
    }

    // 初期位置へ戻る
    private void MoveToStartPosition()
    {
        float differenceX =
            m_startPosition.x - transform.position.x;

        if (Mathf.Abs(differenceX) < 0.05f)
        {
            StopMove();
            return;
        }

        Vector2 direction =
            new Vector2(Mathf.Sign(differenceX), 0f);

        m_rb.linearVelocity = new Vector2(
            direction.x * m_rangedData.moveSpeed,
            m_rb.linearVelocity.y
        );

        LookAtPlayer(direction);
    }

    // 初期位置に到着したか
    private bool IsAtStartPosition()
    {
        return Mathf.Abs(
            transform.position.x - m_startPosition.x
        ) < 0.05f;
    }

    // 移動停止
    private void StopMove()
    {
        if (m_rb == null)
            return;

        m_rb.linearVelocity = new Vector2(
            0f,
            m_rb.linearVelocity.y
        );
    }

    // 弾を発射する
    private void Shoot()
    {
        if (m_shootPoint == null)
        {
            Debug.LogError("ShootPointが設定されていません！");
            return;
        }

        if (m_projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefabが設定されていません！");
            return;
        }

        Vector2 direction = GetPlayerDirection();
        LookAtPlayer(direction);

        GameObject projectile = Instantiate(
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

        Debug.Log("遠距離敵が弾を発射！");

        m_attackTimer = m_rangedData.attackInterval;

        // 攻撃アニメーションを開始する
        m_isAttacking = true;
        m_attackAnimationTimer = m_attackAnimationTime;
    }

    // 攻撃アニメーションの再生時間を管理する
    private void UpdateAttackAnimationTimer()
    {
        if (!m_isAttacking)
            return;

        m_attackAnimationTimer -= Time.deltaTime;

        if (m_attackAnimationTimer <= 0f)
        {
            m_isAttacking = false;
            m_isRecovering = true;
            m_recoveryTimer = m_recoveryTime;
        }
    }

    // アニメーションを更新する
    private void UpdateAnimation()
    {
        if (m_animator == null || m_rb == null)
            return;

        bool isMoving =
            Mathf.Abs(m_rb.linearVelocity.x) > 0.01f;

        m_animator.SetBool("IsMoving", isMoving);
        m_animator.SetBool("IsAttacking", m_isAttacking);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
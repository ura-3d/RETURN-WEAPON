using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] private BossData m_data;

    [Header("Boss HP")]
    [SerializeField] private BossHP m_bossHP;

    [Header("Attack Point")]
    [SerializeField] private BossAttackPoint m_attackPoint;

    [Header("Player")]
    [SerializeField] private Transform m_player;

    [Header("遠距離攻撃")]
    [SerializeField] private Transform m_shootPoint;
    [SerializeField] private GameObject m_projectilePrefab;

    private Rigidbody2D m_rb;

    private BossAIState m_currentState;

    private float m_meleeAttackTimer;
    private float m_rangedAttackTimer;
    private float m_recoveryTimer;

    public BossAIState CurrentState =>
        m_currentState;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();

        // BossHPを自動取得
        if (m_bossHP == null)
        {
            m_bossHP =
                GetComponent<BossHP>();
        }

        // AttackPointを自動取得
        if (m_attackPoint == null)
        {
            m_attackPoint =
                GetComponentInChildren<BossAttackPoint>();
        }

        // ShootPointを自動取得
        if (m_shootPoint == null)
        {
            Transform shootPoint =
                transform.Find("ShootPoint");

            if (shootPoint != null)
            {
                m_shootPoint = shootPoint;
            }
        }
    }

    private void Start()
    {
        // Playerを取得
        if (m_player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                m_player =
                    playerObject.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Playerタグのオブジェクトが見つかりません。"
                );
            }
        }

        if (m_data == null)
        {
            Debug.LogError(
                "BossAIにBossDataが設定されていません！"
            );

            return;
        }

        ChangeState(
            BossAIState.Idle
        );
    }

    private void Update()
    {
        if (m_player == null)
            return;

        if (m_data == null)
            return;

        // HPが0なら死亡
        if (m_bossHP != null &&
            m_bossHP.CurrentHP <= 0)
        {
            ChangeState(
                BossAIState.Dead
            );
        }

        UpdateState();
    }

    // =========================================
    // State
    // =========================================

    private void UpdateState()
    {
        switch (m_currentState)
        {
            case BossAIState.Idle:
                UpdateIdle();
                break;

            case BossAIState.Chase:
                UpdateChase();
                break;

            case BossAIState.Attack:
                UpdateAttack();
                break;

            case BossAIState.Recovery:
                UpdateRecovery();
                break;

            case BossAIState.SpecialAttack:
                UpdateSpecialAttack();
                break;

            case BossAIState.PhaseChange:
                UpdatePhaseChange();
                break;

            case BossAIState.Dead:
                UpdateDead();
                break;
        }
    }

    // =========================================
    // Idle
    // =========================================

    private void UpdateIdle()
    {
        StopMove();

        float distance =
            GetPlayerDistance();

        if (distance <=
            m_data.detectDistance)
        {
            ChangeState(
                BossAIState.Chase
            );
        }
    }

    // =========================================
    // Chase
    // =========================================

    private void UpdateChase()
    {
        float distance =
            GetPlayerDistance();

        // 検知範囲外
        if (distance >
            m_data.detectDistance)
        {
            StopMove();

            ChangeState(
                BossAIState.Idle
            );

            return;
        }

        // 近接攻撃範囲
        if (distance <=
            m_data.attackDistance)
        {
            StopMove();

            ChangeState(
                BossAIState.Attack
            );

            return;
        }

        // 遠距離攻撃範囲
        if (distance <=
            m_data.rangedAttackDistance)
        {
            StopMove();

            ChangeState(
                BossAIState.SpecialAttack
            );

            return;
        }

        // それより遠い場合
        // Playerへ近づく
        MoveToPlayer();
    }

    // =========================================
    // 近接攻撃
    // =========================================

    private void UpdateAttack()
    {
        StopMove();

        LookAtPlayer();

        if (m_meleeAttackTimer > 0f)
        {
            m_meleeAttackTimer -=
                Time.deltaTime;

            return;
        }

        Debug.Log(
            "Bossが近接攻撃！"
        );

        // AttackPointを有効化
        if (m_attackPoint != null)
        {
            m_attackPoint.Activate(
                m_data.attackDamage,
                m_data.attackDuration
            );
        }

        // 近接攻撃クールタイム
        m_meleeAttackTimer =
            m_data.attackInterval;

        // 攻撃後Recovery
        m_recoveryTimer =
            m_data.recoveryTime;

        ChangeState(
            BossAIState.Recovery
        );
    }

    // =========================================
    // 遠距離攻撃
    // =========================================

    private void UpdateSpecialAttack()
    {
        StopMove();

        LookAtPlayer();

        if (m_rangedAttackTimer > 0f)
        {
            m_rangedAttackTimer -=
                Time.deltaTime;

            return;
        }

        ShootProjectile();

        // 遠距離攻撃クールタイム
        m_rangedAttackTimer =
            m_data.rangedAttackInterval;

        // 攻撃後Recovery
        m_recoveryTimer =
            m_data.recoveryTime;

        ChangeState(
            BossAIState.Recovery
        );
    }

    // =========================================
    // Projectile発射
    // =========================================

    private void ShootProjectile()
    {
        if (m_shootPoint == null)
        {
            Debug.LogError(
                "BossのShootPointが設定されていません！"
            );

            return;
        }

        if (m_projectilePrefab == null)
        {
            Debug.LogError(
                "BossのProjectile Prefabが設定されていません！"
            );

            return;
        }

        // Player方向
        Vector2 direction =
            GetPlayerDirection();

        // 弾を生成
        GameObject projectile =
            Instantiate(
                m_projectilePrefab,
                m_shootPoint.position,
                Quaternion.identity
            );

        // EnemyProjectile取得
        EnemyProjectile enemyProjectile =
            projectile.GetComponent<EnemyProjectile>();

        if (enemyProjectile != null)
        {
            enemyProjectile.Initialize(
                direction,
                m_data.projectileSpeed,
                m_data.projectileLifeTime,
                m_data.rangedAttackDamage
            );
        }

        Debug.Log(
            "Bossが遠距離攻撃を発射！"
        );
    }

    // =========================================
    // Recovery
    // =========================================

    private void UpdateRecovery()
    {
        StopMove();

        m_recoveryTimer -=
            Time.deltaTime;

        if (m_recoveryTimer <= 0f)
        {
            ChangeState(
                BossAIState.Chase
            );
        }
    }

    // =========================================
    // Phase Change
    // =========================================

    private void UpdatePhaseChange()
    {
        StopMove();

        Debug.Log(
            "Boss Phase Change!"
        );

        ChangeState(
            BossAIState.Chase
        );
    }

    // =========================================
    // Dead
    // =========================================

    private void UpdateDead()
    {
        StopMove();

        Debug.Log(
            "Boss死亡"
        );

        enabled = false;
    }

    // =========================================
    // Playerへ移動
    // =========================================

    private void MoveToPlayer()
    {
        if (m_rb == null)
            return;

        Vector2 direction =
            GetPlayerDirection();

        m_rb.linearVelocity =
            new Vector2(
                direction.x *
                m_data.moveSpeed,
                m_rb.linearVelocity.y
            );

        LookAtPlayer();
    }

    // =========================================
    // 移動停止
    // =========================================

    private void StopMove()
    {
        if (m_rb == null)
            return;

        m_rb.linearVelocity =
            new Vector2(
                0f,
                m_rb.linearVelocity.y
            );
    }

    // =========================================
    // Playerとの距離
    // =========================================

    private float GetPlayerDistance()
    {
        if (m_player == null)
            return Mathf.Infinity;

        return Vector2.Distance(
            transform.position,
            m_player.position
        );
    }

    // =========================================
    // Playerの方向
    // =========================================

    private Vector2 GetPlayerDirection()
    {
        if (m_player == null)
            return Vector2.zero;

        return (
            m_player.position -
            transform.position
        ).normalized;
    }

    // =========================================
    // Playerの方向を向く
    // =========================================

    private void LookAtPlayer()
    {
        Vector2 direction =
            GetPlayerDirection();

        if (direction.x > 0f)
        {
            transform.localScale =
                new Vector3(
                    1f,
                    1f,
                    1f
                );
        }
        else if (direction.x < 0f)
        {
            transform.localScale =
                new Vector3(
                    -1f,
                    1f,
                    1f
                );
        }
    }

    // =========================================
    // State変更
    // =========================================

    private void ChangeState(
        BossAIState newState)
    {
        if (m_currentState == newState)
            return;

        m_currentState =
            newState;

        Debug.Log(
            "Boss AI State : " +
            m_currentState
        );
    }
}
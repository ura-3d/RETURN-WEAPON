using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    protected MeleeEnemyData m_meleeData;

    private float m_attackTimer;
    private bool m_isAttacking;
    private bool m_isRecovering;

    [Header("行動パターン")]
    [SerializeField] private float m_recoveryTime = 0.5f;

    private float m_recoveryTimer;

    [Header("アニメーション")]
    private Animator m_animator;

    protected override void Awake()
    {
        base.Awake();

        m_meleeData = m_data as MeleeEnemyData;
        m_animator = GetComponent<Animator>();

        if (m_meleeData == null)
        {
            Debug.LogError(
                "MeleeEnemyにMeleeEnemyDataが設定されていません。"
            );
        }

        if (m_animator == null)
        {
            Debug.LogWarning(
                "MeleeEnemyにAnimatorが設定されていません。"
            );
        }
    }

    protected override void EnemyUpdate()
    {
        if (m_meleeData == null)
            return;

        if (m_player == null)
            return;

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

                Debug.Log("近接敵の硬直終了");
            }

            UpdateAnimation();
            return;
        }

        if (m_attackTimer > 0f)
        {
            m_attackTimer -= Time.deltaTime;
        }

        float distance = GetPlayerDistance();

        if (distance > m_meleeData.detectDistance)
        {
            ChangeState(EnemyAIState.Idle);
            StopMove();
            UpdateAnimation();
            return;
        }

        if (distance <= m_meleeData.attackDistance)
        {
            ChangeState(EnemyAIState.Attack);
            StopMove();

            Vector2 direction = GetPlayerDirection();
            LookAtPlayer(direction);

            if (m_attackTimer <= 0f)
            {
                Attack();
            }

            UpdateAnimation();
            return;
        }

        ChangeState(EnemyAIState.Chase);
        MoveToPlayer();
        UpdateAnimation();
    }

    // プレイヤーに向かって移動
    private void MoveToPlayer()
    {
        Vector2 direction = GetPlayerDirection();

        m_rb.linearVelocity = new Vector2(
            direction.x * m_meleeData.moveSpeed,
            m_rb.linearVelocity.y
        );

        LookAtPlayer(direction);
    }

    // 移動を停止
    private void StopMove()
    {
        if (m_rb == null)
            return;

        m_rb.linearVelocity = new Vector2(
            0f,
            m_rb.linearVelocity.y
        );
    }

    // 攻撃開始
    private void Attack()
    {
        Debug.Log("近接敵が攻撃！");

        m_isAttacking = true;
        m_attackTimer = m_meleeData.attackInterval;

        UpdateAnimation();

        Invoke(
            nameof(EndAttack),
            m_meleeData.attackDuration
        );
    }

    // 攻撃終了
    private void EndAttack()
    {
        m_isAttacking = false;
        m_isRecovering = true;
        m_recoveryTimer = m_recoveryTime;

        UpdateAnimation();

        Debug.Log("近接敵が攻撃終了 → Recovery");
    }

    // アニメーション更新
    private void UpdateAnimation()
    {
        if (m_animator == null || m_rb == null)
            return;

        bool isMoving =
            Mathf.Abs(m_rb.linearVelocity.x) > 0.01f;

        m_animator.SetBool("IsMoving", isMoving);
        m_animator.SetBool("IsAttacking", m_isAttacking);
    }

    // プレイヤーに攻撃が命中したとき
    public void AttackHit(GameObject target)
    {
        if (target == null)
            return;

        if (!target.CompareTag("Player"))
            return;

        PlayerHP playerHP =
            target.GetComponentInParent<PlayerHP>();

        if (playerHP != null)
        {
            playerHP.TakeDamage(
                m_meleeData.attackDamage
            );

            Debug.Log("近接攻撃がPlayerに命中！");
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
        m_isAttacking = false;
        m_isRecovering = false;
    }
}
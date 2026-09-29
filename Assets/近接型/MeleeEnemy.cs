using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    protected MeleeEnemyData m_meleeData;

    private float m_attackTimer;
    private bool m_isAttacking;
    private bool m_isRecovering;

    [Header("s“®ƒpƒ^[ƒ“")]
    [SerializeField] private float m_recoveryTime = 0.5f;

    private float m_recoveryTimer;

    protected override void Awake()
    {
        base.Awake();

        m_meleeData =
            m_data as MeleeEnemyData;

        if (m_meleeData == null)
        {
            Debug.LogError(
                "MeleeEnemy‚ÉMeleeEnemyData‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñB"
            );
        }
    }

    protected override void EnemyUpdate()
    {
        if (m_meleeData == null)
            return;

        if (m_player == null)
            return;

        // =================================
        // UŒ‚’†
        // =================================
        if (m_isAttacking)
        {
            ChangeState(EnemyAIState.Attack);

            StopMove();

            return;
        }

        // =================================
        // UŒ‚Œã‚Ìd’¼
        // =================================
        if (m_isRecovering)
        {
            ChangeState(EnemyAIState.Recovery);

            StopMove();

            m_recoveryTimer -= Time.deltaTime;

            if (m_recoveryTimer <= 0f)
            {
                m_isRecovering = false;

                Debug.Log(
                    "‹ßÚ“G‚Ìd’¼I—¹"
                );
            }

            return;
        }

        // =================================
        // UŒ‚ƒN[ƒ‹ƒ^ƒCƒ€
        // =================================
        if (m_attackTimer > 0f)
        {
            m_attackTimer -= Time.deltaTime;
        }

        float distance =
            GetPlayerDistance();

        // =================================
        // ŒŸ’m”ÍˆÍŠO
        // =================================
        if (distance >
            m_meleeData.detectDistance)
        {
            ChangeState(EnemyAIState.Idle);

            StopMove();

            return;
        }

        // =================================
        // UŒ‚‹——£
        // =================================
        if (distance <=
            m_meleeData.attackDistance)
        {
            ChangeState(EnemyAIState.Attack);

            StopMove();

            Vector2 direction =
                GetPlayerDirection();

            LookAtPlayer(direction);

            if (m_attackTimer <= 0f)
            {
                Attack();
            }

            return;
        }

        // =================================
        // ƒvƒŒƒCƒ„[‚ğ’ÇÕ
        // =================================
        ChangeState(EnemyAIState.Chase);

        MoveToPlayer();
    }

    // =================================
    // ƒvƒŒƒCƒ„[‚ÖˆÚ“®
    // =================================
    private void MoveToPlayer()
    {
        Vector2 direction =
            GetPlayerDirection();

        m_rb.linearVelocity =
            new Vector2(
                direction.x *
                m_meleeData.moveSpeed,
                m_rb.linearVelocity.y
            );

        LookAtPlayer(direction);
    }

    // =================================
    // ˆÚ“®’â~
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
    // UŒ‚ŠJn
    // =================================
    private void Attack()
    {
        Debug.Log(
            "‹ßÚ“G‚ªUŒ‚I"
        );

        m_isAttacking = true;

        m_attackTimer =
            m_meleeData.attackInterval;

        Invoke(
            nameof(EndAttack),
            m_meleeData.attackDuration
        );
    }

    // =================================
    // UŒ‚I—¹
    // =================================
    private void EndAttack()
    {
        m_isAttacking = false;

        // UŒ‚Œãd’¼ŠJn
        m_isRecovering = true;

        m_recoveryTimer =
            m_recoveryTime;

        Debug.Log(
            "‹ßÚ“G‚ªUŒ‚I—¹ ¨ Recovery"
        );
    }

    // =================================
    // UŒ‚”»’è
    // =================================
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

            Debug.Log(
                "‹ßÚUŒ‚‚ªPlayer‚É–½’†I"
            );
        }
    }
}
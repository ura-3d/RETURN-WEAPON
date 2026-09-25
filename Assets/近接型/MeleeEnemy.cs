using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    protected MeleeEnemyData m_meleeData;

    private float m_attackTimer;
    private bool m_isAttacking;

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

        // UŒ‚’†
        if (m_isAttacking)
        {
            ChangeState(EnemyAIState.Attack);
            return;
        }

        // UŒ‚ƒN[ƒ‹ƒ^ƒCƒ€
        if (m_attackTimer > 0f)
        {
            m_attackTimer -= Time.deltaTime;
        }

        float distance =
            GetPlayerDistance();

        // =========================
        // Idle
        // =========================
        if (distance >
            m_meleeData.detectDistance)
        {
            ChangeState(EnemyAIState.Idle);

            StopMove();

            return;
        }

        // =========================
        // Attack
        // =========================
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

        // =========================
        // Chase
        // =========================
        ChangeState(EnemyAIState.Chase);

        MoveToPlayer();
    }

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

    private void Attack()
    {
        Debug.Log(
            "‹ßÚ“G‚ªUŒ‚I"
        );

        m_isAttacking = true;

        PlayerHP playerHP =
            m_player.GetComponent<PlayerHP>();

        if (playerHP != null)
        {
            playerHP.TakeDamage(
                m_meleeData.attackDamage
            );
        }

        m_attackTimer =
            m_meleeData.attackInterval;

        Invoke(
            nameof(EndAttack),
            m_meleeData.attackDuration
        );
    }

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

    private void EndAttack()
    {
        m_isAttacking = false;

        Debug.Log(
            "‹ßÚ“G‚ÌUŒ‚I—¹"
        );
    }
}
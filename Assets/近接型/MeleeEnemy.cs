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

    protected override void Awake()
    {
        base.Awake();

        m_meleeData =
            m_data as MeleeEnemyData;

        if (m_meleeData == null)
        {
            Debug.LogError(
                "MeleeEnemyにMeleeEnemyDataが設定されていません。"
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
                    "近接敵の硬直終了"
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
            m_meleeData.detectDistance)
        {
            ChangeState(
                EnemyAIState.Return
            );

            MoveToStartPosition(
                m_meleeData.moveSpeed
            );

            // 初期位置に戻った
            if (IsAtStartPosition())
            {
                StopMove();

                ChangeState(
                    EnemyAIState.Idle
                );

                Debug.Log(
                    "近接敵が初期位置へ戻りました。"
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
                m_meleeData.detectDistance)
            {
                ChangeState(
                    EnemyAIState.Chase
                );

                return;
            }

            MoveToStartPosition(
                m_meleeData.moveSpeed
            );

            if (IsAtStartPosition())
            {
                StopMove();

                ChangeState(
                    EnemyAIState.Idle
                );

                Debug.Log(
                    "近接敵が初期位置へ戻りました。"
                );
            }

            return;
        }

        // =================================
        // 攻撃距離
        // =================================
        if (distance <=
            m_meleeData.attackDistance)
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
                Attack();
            }

            return;
        }

        // =================================
        // プレイヤーを追跡
        // =================================
        ChangeState(
            EnemyAIState.Chase
        );

        MoveToPlayer();
    }

    // =================================
    // プレイヤーへ移動
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
    // 攻撃開始
    // =================================
    private void Attack()
    {
        Debug.Log(
            "近接敵が攻撃！"
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
    // 攻撃終了
    // =================================
    private void EndAttack()
    {
        m_isAttacking = false;

        // 攻撃後硬直開始
        m_isRecovering = true;

        m_recoveryTimer =
            m_recoveryTime;

        Debug.Log(
            "近接敵が攻撃終了 → Recovery"
        );
    }

    // =================================
    // 攻撃判定
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
                "近接攻撃がPlayerに命中！"
            );
        }
    }

    // =================================
    // オブジェクト破棄時
    // =================================
    private void OnDisable()
    {
        CancelInvoke(
            nameof(EndAttack)
        );
    }
}
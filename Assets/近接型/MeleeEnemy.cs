using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    protected MeleeEnemyData m_meleeData;

    private float m_attackTimer;
    private bool m_isAttacking;
    private bool m_isRecovering;

    [Header("行動パターン")]
    [SerializeField] private float m_recoveryTime = 0.5f;

    // 初期位置にこの距離まで近づいたら停止する
    [SerializeField] private float m_returnStopDistance = 0.1f;

    private float m_recoveryTimer;

    // 初期位置
    private Vector2 m_startPosition;

    [Header("アニメーション")]
    private Animator m_animator;

    protected override void Awake()
    {
        base.Awake();

        m_meleeData = m_data as MeleeEnemyData;
        m_animator = GetComponent<Animator>();

        // 初期位置を記録
        m_startPosition = transform.position;

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

        // プレイヤーが離れたら初期位置へ戻る
        if (distance > m_meleeData.detectDistance)
        {
            ReturnToStart();
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

    // 初期位置に戻る
    private void ReturnToStart()
    {
        float diffX = m_startPosition.x - transform.position.x;

        // 初期位置に着いたら待機
        if (Mathf.Abs(diffX) <= m_returnStopDistance)
        {
            ChangeState(EnemyAIState.Idle);
            StopMove();
            return;
        }

        // 専用のステートがあれば Chase を差し替える
        ChangeState(EnemyAIState.Chase);

        float dirX = Mathf.Sign(diffX);

        m_rb.linearVelocity = new Vector2(
            dirX * m_meleeData.moveSpeed,
            m_rb.linearVelocity.y
        );

        LookAtPlayer(new Vector2(dirX, 0f));
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
        {
            Debug.LogWarning(
                "Animator または Rigidbody2D が見つかりません"
            );
            return;
        }

        bool isMoving =
            Mathf.Abs(m_rb.linearVelocity.x) > 0.01f;

        m_animator.SetBool("IsMoving", isMoving);
        m_animator.SetBool("IsAttacking", m_isAttacking);

        Debug.Log(
            "速度：" + m_rb.linearVelocity.x +
            " / 設定値：" + isMoving +
            " / Animator値：" +
            m_animator.GetBool("IsMoving") +
            " / 攻撃値：" +
            m_animator.GetBool("IsAttacking") +
            " / Controller：" +
            (m_animator.runtimeAnimatorController != null
                ? m_animator.runtimeAnimatorController.name
                : "未設定")
        );
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

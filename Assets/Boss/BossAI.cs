using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private BossData m_data;
    [SerializeField] private BossHP m_bossHP;

    [Header("Player")]
    [SerializeField] private Transform m_player;

    private Rigidbody2D m_rb;

    private BossAIState m_currentState;

    private float m_attackTimer;
    private float m_recoveryTimer;

    public BossAIState CurrentState =>
        m_currentState;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();

        if (m_bossHP == null)
        {
            m_bossHP =
                GetComponent<BossHP>();
        }
    }

    private void Start()
    {
        // PlayeréÊìæ
        if (m_player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                m_player =
                    playerObject.transform;
            }
        }

        if (m_data == null)
        {
            Debug.LogError(
                "BossAIÇ…BossDataÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅI"
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

        UpdateState();
    }

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
                StopMove();
                break;
        }
    }

    // =====================================
    // Idle
    // =====================================

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

    // =====================================
    // Chase
    // =====================================

    private void UpdateChase()
    {
        float distance =
            GetPlayerDistance();

        // PlayerÇ™ó£ÇÍÇΩ
        if (distance >
            m_data.detectDistance)
        {
            ChangeState(
                BossAIState.Idle
            );

            return;
        }

        // çUåÇãóó£Ç…ì¸Ç¡ÇΩ
        if (distance <=
            m_data.attackInterval)
        {
            StopMove();

            ChangeState(
                BossAIState.Attack
            );

            return;
        }

        MoveToPlayer();
    }

    // =====================================
    // Attack
    // =====================================

    private void UpdateAttack()
    {
        StopMove();

        LookAtPlayer();

        if (m_attackTimer > 0f)
        {
            m_attackTimer -=
                Time.deltaTime;

            return;
        }

        Debug.Log(
            "BossÇ™í èÌçUåÇÅI"
        );

        m_attackTimer =
            m_data.attackInterval;

        m_recoveryTimer =
            m_data.recoveryTime;

        ChangeState(
            BossAIState.Recovery
        );
    }

    // =====================================
    // Recovery
    // =====================================

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

    // =====================================
    // SpecialAttack
    // =====================================

    private void UpdateSpecialAttack()
    {
        StopMove();

        Debug.Log(
            "Bossì¡éÍçUåÇÅI"
        );

        ChangeState(
            BossAIState.Recovery
        );
    }

    // =====================================
    // PhaseChange
    // =====================================

    private void UpdatePhaseChange()
    {
        StopMove();

        Debug.Log(
            "BossÇÃPhaseÇ™ïœçXÇ≥ÇÍÇ‹ÇµÇΩÅI"
        );

        ChangeState(
            BossAIState.Chase
        );
    }

    // =====================================
    // PlayerÇ÷à⁄ìÆ
    // =====================================

    private void MoveToPlayer()
    {
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

    // =====================================
    // à⁄ìÆí‚é~
    // =====================================

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

    // =====================================
    // PlayerÇ∆ÇÃãóó£
    // =====================================

    private float GetPlayerDistance()
    {
        if (m_player == null)
            return Mathf.Infinity;

        return Vector2.Distance(
            transform.position,
            m_player.position
        );
    }

    // =====================================
    // PlayerÇÃï˚å¸
    // =====================================

    private Vector2 GetPlayerDirection()
    {
        if (m_player == null)
            return Vector2.zero;

        return (
            m_player.position -
            transform.position
        ).normalized;
    }

    // =====================================
    // PlayerÇå©ÇÈ
    // =====================================

    private void LookAtPlayer()
    {
        Vector2 direction =
            GetPlayerDirection();

        if (direction.x > 0f)
        {
            transform.localScale =
                new Vector3(1f, 1f, 1f);
        }
        else if (direction.x < 0f)
        {
            transform.localScale =
                new Vector3(-1f, 1f, 1f);
        }
    }

    // =====================================
    // StateïœçX
    // =====================================

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
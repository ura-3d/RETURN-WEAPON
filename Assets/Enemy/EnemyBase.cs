using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("敵データ")]
    [SerializeField]
    protected EnemyData m_data;

    protected Transform m_player;
    protected Rigidbody2D m_rb;

    // 現在のAI状態
    protected EnemyAIState m_currentState =
        EnemyAIState.Idle;

    public EnemyAIState CurrentState =>
        m_currentState;

    protected virtual void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
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

    protected virtual void Update()
    {
        if (m_player == null)
            return;

        if (m_data == null)
            return;

        EnemyUpdate();
    }

    // 敵ごとのAI処理
    protected abstract void EnemyUpdate();

    // =========================
    // AI状態変更
    // =========================

    protected void ChangeState(
        EnemyAIState newState)
    {
        if (m_currentState == newState)
            return;

        m_currentState = newState;

        Debug.Log(
            gameObject.name +
            " AI状態：" +
            m_currentState
        );
    }

    // =========================
    // Playerとの距離
    // =========================

    protected float GetPlayerDistance()
    {
        if (m_player == null)
            return Mathf.Infinity;

        return Vector2.Distance(
            transform.position,
            m_player.position
        );
    }

    // =========================
    // Playerの方向
    // =========================

    protected Vector2 GetPlayerDirection()
    {
        if (m_player == null)
            return Vector2.zero;

        return (
            m_player.position -
            transform.position
        ).normalized;
    }

    // =========================
    // Playerの方向を向く
    // =========================

    protected void LookAtPlayer(
        Vector2 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale =
                new Vector3(1, 1, 1);
        }
        else if (direction.x < 0)
        {
            transform.localScale =
                new Vector3(-1, 1, 1);
        }
    }

    // =========================
    // 移動停止
    // =========================

    protected void StopMove()
    {
        if (m_rb == null)
            return;

        m_rb.linearVelocity =
            new Vector2(
                0f,
                m_rb.linearVelocity.y
            );
    }
}
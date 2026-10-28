using System.Collections.Generic;
using UnityEngine;

// 動く足場にアタッチする（Sprite、Is Triggerオフの Collider2D も必要）
// ・開始位置から「Move Offset」だけ離れた位置まで、行ったり来たりする
// ・上に乗っているPlayerや武器は、足場と一緒に運ばれる
// ※Tagは「Ground」にすること（Playerのジャンプ判定、武器の着地判定のため）
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("移動")]
    // 開始位置からの移動量（例：(4, 0)なら右へ4、(0, 3)なら上へ3）
    [SerializeField] private Vector2 m_moveOffset = new Vector2(4f, 0f);

    // 移動の速さ
    [SerializeField] private float m_speed = 2f;

    // 端に着いたときの待ち時間（秒）
    [SerializeField] private float m_waitTime = 0.5f;

    private Rigidbody2D m_rb;

    private Vector2 m_startPosition;
    private Vector2 m_endPosition;

    // 終点へ向かっているか
    private bool m_isMovingToEnd = true;

    private float m_waitTimer;

    // 足場の現在の速度（PlayerMoveが、接地の判定に使う）
    private Vector2 m_currentVelocity;
    public Vector2 CurrentVelocity => m_currentVelocity;

    // 1つ前の物理更新での、足場の縦の速度
    private float m_previousVelocityY;

    // 上に乗っているもの
    private HashSet<Rigidbody2D> m_riders =
        new HashSet<Rigidbody2D>();

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();

        // 物理の力で動かされないように、Kinematicにする
        m_rb.bodyType = RigidbodyType2D.Kinematic;

        // 摩擦をなくす（摩擦で、乗っているものが少しずつ引きずられるのを防ぐ）
        // 乗っているものを運ぶ処理は、このスクリプトが自分で行っている
        PhysicsMaterial2D noFriction =
            new PhysicsMaterial2D("PlatformNoFriction");

        noFriction.friction = 0f;
        noFriction.bounciness = 0f;

        foreach (Collider2D platformCollider in GetComponents<Collider2D>())
        {
            platformCollider.sharedMaterial = noFriction;
        }

        m_startPosition = m_rb.position;
        m_endPosition = m_startPosition + m_moveOffset;
    }

    private void FixedUpdate()
    {
        // 今回の移動量
        Vector2 delta = Vector2.zero;

        if (m_waitTimer > 0f)
        {
            // 端で待っているとき
            m_waitTimer -= Time.fixedDeltaTime;
        }
        else
        {
            Vector2 target =
                m_isMovingToEnd ? m_endPosition : m_startPosition;

            Vector2 newPosition =
                Vector2.MoveTowards(
                    m_rb.position,
                    target,
                    m_speed * Time.fixedDeltaTime
                );

            delta = newPosition - m_rb.position;

            m_rb.MovePosition(newPosition);

            // 端に着いたら、向きを反転して待つ
            if (newPosition == target)
            {
                m_isMovingToEnd = !m_isMovingToEnd;
                m_waitTimer = m_waitTime;
            }
        }

        m_currentVelocity = delta / Time.fixedDeltaTime;

        // 上に乗っているものを運ぶ
        MoveRiders(delta);

        m_previousVelocityY = m_currentVelocity.y;
    }

    // 乗っているものを運ぶ
    // ・横：足場が動いた量だけ位置を動かす（Playerは横の速度を自分で決めているため）
    // ・縦：縦の速度を足場に合わせる（浮いたり、離れたりしないようにするため）
    private void MoveRiders(Vector2 delta)
    {
        float platformVelocityY = m_currentVelocity.y;

        // ジャンプ中（足場より大きく上向きの速度）かどうかの判定に使う
        float jumpThreshold =
            Mathf.Max(m_previousVelocityY, platformVelocityY, 0f) + 1f;

        foreach (Rigidbody2D rider in m_riders)
        {
            if (rider == null)
                continue;

            // 止まっているときに眠らないようにする
            rider.WakeUp();

            // 横の移動を足す
            rider.position += new Vector2(delta.x, 0f);

            // ジャンプ中でなければ、縦の速度を足場に合わせる
            Vector2 velocity = rider.linearVelocity;

            if (velocity.y <= jumpThreshold)
            {
                velocity.y = platformVelocityY;
                rider.linearVelocity = velocity;
            }
        }
    }

    // 足場の上に乗ったとき
    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateRider(collision);
    }

    // 足場の上に乗り続けているとき
    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateRider(collision);
    }

    // 足場から離れたとき
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.rigidbody != null)
        {
            m_riders.Remove(collision.rigidbody);
        }
    }

    // 足場の「上」に乗っているものだけを、乗客として記録する
    private void UpdateRider(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.rigidbody;

        if (otherRb == null)
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 足場から見て、相手が上にいるとき、法線は下向きになる
            if (contact.normal.y < -0.5f)
            {
                m_riders.Add(otherRb);
                return;
            }
        }

        // 上に乗っていなければ、乗客から外す
        m_riders.Remove(otherRb);
    }

    // Sceneビューで、移動する経路を表示（このオブジェクトを選択中のみ）
    private void OnDrawGizmosSelected()
    {
        Vector2 start =
            Application.isPlaying ? m_startPosition : (Vector2)transform.position;

        Vector2 end =
            Application.isPlaying ? m_endPosition : start + m_moveOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.2f);
        Gizmos.DrawWireSphere(end, 0.2f);
    }
}
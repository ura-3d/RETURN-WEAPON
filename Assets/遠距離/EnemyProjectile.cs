using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D m_rb;

    private float m_lifeTime;
    private int m_damage;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Vector2 direction,
        float speed,
        float lifeTime,
        int damage)
    {
        m_lifeTime = lifeTime;
        m_damage = damage;

        m_rb.linearVelocity =
            direction.normalized * speed;

        Destroy(gameObject, m_lifeTime);
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHP playerHP =
            other.GetComponentInParent<PlayerHP>();

        if (playerHP != null)
        {
            Debug.Log(
                "ìGÇÃíeÇ™PlayerÇ…ñΩíÜÅI"
            );

            playerHP.TakeDamage(m_damage);
        }

        Destroy(gameObject);
    }
}
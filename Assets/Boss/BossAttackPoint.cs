using UnityEngine;

public class BossAttackPoint : MonoBehaviour
{
    private bool m_isActive;

    private float m_timer;

    private int m_damage;

    private void Update()
    {
        if (!m_isActive)
            return;

        m_timer -=
            Time.deltaTime;

        if (m_timer <= 0f)
        {
            Deactivate();
        }
    }

    public void Activate(
        int damage,
        float activeTime)
    {
        m_damage = damage;

        m_timer = activeTime;

        m_isActive = true;
    }

    public void Deactivate()
    {
        m_isActive = false;
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!m_isActive)
            return;

        PlayerHP playerHP =
            other.GetComponentInParent<PlayerHP>();

        if (playerHP == null)
            return;

        playerHP.TakeDamage(
            m_damage
        );

        Debug.Log(
            "Boss‚Ì‹ßÚUŒ‚‚ªPlayer‚É–½’†I"
        );

        Deactivate();
    }
}
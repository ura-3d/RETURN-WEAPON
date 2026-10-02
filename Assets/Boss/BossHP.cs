using UnityEngine;

public class BossHP : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] private BossData m_data;

    private int m_currentHP;

    private int m_currentPhase = 1;

    public int CurrentHP =>
        m_currentHP;

    public int MaxHP =>
        m_data != null ?
        m_data.MaxHP :
        0;

    public int CurrentPhase =>
        m_currentPhase;

    private void Start()
    {
        if (m_data == null)
        {
            Debug.LogError(
                "BossHPにBossDataが設定されていません！"
            );

            return;
        }

        m_currentHP =
            m_data.MaxHP;

        m_currentPhase = 1;

        Debug.Log(
            "Boss HP：" +
            m_currentHP +
            "/" +
            m_data.MaxHP
        );
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        if (m_currentHP <= 0)
            return;

        m_currentHP -= damage;

        if (m_currentHP < 0)
        {
            m_currentHP = 0;
        }

        Debug.Log(
            "Bossが " +
            damage +
            " ダメージを受けた！ HP：" +
            m_currentHP +
            "/" +
            m_data.MaxHP
        );

        CheckPhase();

        if (m_currentHP <= 0)
        {
            Die();
        }
    }

    private void CheckPhase()
    {
        if (m_currentPhase == 1 &&
            m_currentHP <=
            m_data.phase2HP)
        {
            m_currentPhase = 2;

            Debug.Log(
                "Boss Phase 2 開始！"
            );

            return;
        }

        if (m_currentPhase == 2 &&
            m_currentHP <=
            m_data.phase3HP)
        {
            m_currentPhase = 3;

            Debug.Log(
                "Boss Phase 3 開始！"
            );
        }
    }

    private void Die()
    {
        Debug.Log(
            "Bossを倒した！"
        );
    }
}
using Unity.VisualScripting;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] private BossData m_data;

    [Header("Boss HP")]
    [SerializeField] private BossHP m_bossHP;

    [Header("Player")]
    [SerializeField] private Transform m_player;

    private Rigidbody2D m_rb;

    private void Awake()
    {
        m_rb= GetComponent<Rigidbody2D>();

        if(m_bossHP == null)
        {
            m_bossHP = GetComponent<BossHP>();
        }
    }

    private void Start()
    {
        if(m_player == null)
        {
            GameObject playerobject =
                GameObject.FindGameObjectWithTag("Player");

            if(playerobject != null )
            {
                m_player =
                    playerobject.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Playerタグのオブジェクトが見つかりません。"
                );
            }
        }

        // BossData確認
        if (m_data == null)
        {
            Debug.LogError(
                "BossEnemyにBossDataが設定されていません！"
            );
        }

        // BossHP確認
        if (m_bossHP == null)
        {
            Debug.LogError(
                "BossEnemyにBossHPが設定されていません！"
            );
        }
    }

    private void Update()
    {
        if (m_player == null)
            return;

        if (m_data == null)
            return;

        // 現在のBoss Phaseを確認
        if (m_bossHP != null)
        {
            int phase =
                m_bossHP.CurrentPhase;

            Debug.Log(
                "Boss Phase：" +
                phase
            );
        }
    }

    public Transform GetPlayer()
    {
        return m_player;
    }

    public BossData GetBossData()
    {
        return m_data;
    }

    public BossHP GetBossHP()
    {
        return m_bossHP;
    }
}

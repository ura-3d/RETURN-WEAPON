using UnityEngine;

// 落下したらGameOverManagerにゲームオーバーを伝えるスクリプト
// ・判定エリア(Trigger)に触れたとき
// ・Playerの高さ(Y座標)が一定より下になったとき
// のどちらかでゲームオーバーになる
public class GameOverZone : MonoBehaviour
{
    [Header("ゲームオーバー管理")]
    // 空のままなら、シーン内のGameOverManagerを自動で探す
    [SerializeField] private GameOverManager m_gameOverManager;

    [Header("落下判定")]
    // このY座標より下に落ちたらゲームオーバー
    [SerializeField] private float m_fallLimitY = -10f;

    private Transform m_player;
    private bool m_isGameOver;

    private void Start()
    {
        if (m_gameOverManager == null)
        {
            m_gameOverManager = FindFirstObjectByType<GameOverManager>();
        }

        if (m_gameOverManager == null)
        {
            Debug.LogWarning("シーン内にGameOverManagerが見つかりません。");
        }

        // Playerを探しておく
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            m_player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Playerタグのオブジェクトが見つかりません。");
        }
    }

    private void Update()
    {
        if (m_isGameOver)
            return;

        if (m_player == null)
            return;

        // 一定の高さより下に落ちたらゲームオーバー
        if (m_player.position.y < m_fallLimitY)
        {
            TriggerGameOver();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_isGameOver)
            return;

        // Player本体、またはその子オブジェクトのコライダーか判定
        bool isPlayer =
            other.CompareTag("Player") ||
            other.GetComponentInParent<PlayerHP>() != null;

        if (!isPlayer)
            return;

        TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        m_isGameOver = true;

        Debug.Log("落下によるゲームオーバー");

        if (m_gameOverManager != null)
        {
            m_gameOverManager.GameOver();
        }
    }

    // Sceneビューで落下ラインを赤線で表示（このオブジェクトを選択中のみ）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-1000f, m_fallLimitY, 0f),
            new Vector3(1000f, m_fallLimitY, 0f)
        );
    }
}
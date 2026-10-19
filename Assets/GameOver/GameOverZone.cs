using UnityEngine;

// 落下したらゲームオーバーにするスクリプト
// ・判定エリア(Trigger)に触れたとき
// ・Playerの高さ(Y座標)が一定より下になったとき
// のどちらかでゲームオーバーになる
public class GameOverZone : MonoBehaviour
{
    [Header("ゲームオーバーUI")]
    [SerializeField] private GameObject m_gameOverUI;

    [Header("落下判定")]
    // このY座標より下に落ちたらゲームオーバー
    [SerializeField] private float m_fallLimitY = -10f;

    private Transform m_player;
    private bool m_isGameOver;

    private void Start()
    {
        // 最初はゲームオーバー表示を隠しておく
        if (m_gameOverUI != null)
        {
            m_gameOverUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameOverZoneにゲームオーバーUIが設定されていません。");
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
            GameOver();
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

        GameOver();
    }

    private void GameOver()
    {
        m_isGameOver = true;

        Debug.Log("ゲームオーバー（落下）");

        if (m_gameOverUI != null)
        {
            m_gameOverUI.SetActive(true);
        }

        // ゲームを一時停止
        Time.timeScale = 0f;
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

    private void OnDestroy()
    {
        // シーン再読み込み時に止まったままにならないよう戻しておく
        Time.timeScale = 1f;
    }
}
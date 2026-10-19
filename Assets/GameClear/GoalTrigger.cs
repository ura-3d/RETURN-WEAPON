using UnityEngine;

// ゴールオブジェクトにアタッチする
// Playerが触れたらゲームクリア表示を出して、ゲームを止める
public class GoalTrigger : MonoBehaviour
{
    [Header("ゲームクリアUI")]
    [SerializeField] private GameObject m_gameClearUI;

    private bool m_isCleared;

    private void Start()
    {
        // 最初はクリア表示を隠しておく
        if (m_gameClearUI != null)
        {
            m_gameClearUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GoalTriggerにゲームクリアUIが設定されていません。");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_isCleared)
            return;

        // Player本体、またはその子オブジェクトのコライダーか判定
        bool isPlayer =
            other.CompareTag("Player") ||
            other.GetComponentInParent<PlayerHP>() != null;

        if (!isPlayer)
            return;

        GameClear();
    }

    private void GameClear()
    {
        m_isCleared = true;

        Debug.Log("ゲームクリア！");

        if (m_gameClearUI != null)
        {
            m_gameClearUI.SetActive(true);
        }

        // ゲームを一時停止
        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        // シーン再読み込み時に止まったままにならないよう戻しておく
        Time.timeScale = 1f;
    }
}
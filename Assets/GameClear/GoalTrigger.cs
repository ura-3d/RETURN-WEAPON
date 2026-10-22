
using UnityEngine;

// ゴールに触れたらゲームクリア処理を呼び出す
public class GoalTrigger : MonoBehaviour
{
    [Header("タイマー管理")]
    [SerializeField] private TimerManager timerManager;

    private bool m_isCleared;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_isCleared)
            return;

        // Player本体、またはその子オブジェクトか判定
        bool isPlayer =
            other.CompareTag("Player") ||
            other.GetComponentInParent<PlayerHP>() != null;

        if (!isPlayer)
            return;

        m_isCleared = true;

        Debug.Log("ゴールに到達しました");

        if (timerManager != null)
        {
            timerManager.ReachGoal();
        }
        else
        {
            Debug.LogError(
                "GoalTriggerにTimerManagerが設定されていません。"
            );
        }
    }
}

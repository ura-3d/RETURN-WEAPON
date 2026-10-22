
using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [Header("制限時間")]
    [SerializeField] private float limitTime = 60f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("ゲームオーバー")]
    [SerializeField] private GameOverManager gameOverManager;

    [Header("ゲームクリア")]
    [SerializeField] private GameClearManager gameClearManager;

    private float remainingTime;
    private bool isTimeUp = false;
    private bool isFinished = false;

    private void Start()
    {
        remainingTime = limitTime;
        UpdateTimerText();
    }

    private void Update()
    {
        if (isFinished)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isTimeUp = true;
            isFinished = true;

            UpdateTimerText();

            if (gameOverManager != null)
            {
                gameOverManager.GameOver();
            }
            else
            {
                Debug.LogError("GameOverManagerが設定されていません。");
            }

            return;
        }

        UpdateTimerText();
    }

    // ゴールしたとき
    public void ReachGoal()
    {
        if (isFinished)
            return;

        isFinished = true;
        UpdateTimerText();

        if (gameClearManager != null)
        {
            gameClearManager.GameClear();
        }
        else
        {
            Debug.LogError("GameClearManagerが設定されていません。");
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = $"TIME: {minutes:00}:{seconds:00}";
    }
}

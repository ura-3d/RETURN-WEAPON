using UnityEngine;
using UnityEngine.SceneManagement;

// ゲームオーバー画面を管理する
// ※Panelとは別の「常に表示されているオブジェクト」にアタッチすること
//   （Panelと同じオブジェクトに付けると、非表示の間はスクリプトが動かない）
public class GameOverManager : MonoBehaviour
{
    [Header("ゲームオーバー画面")]
    [SerializeField] private GameObject gameOverPanel;

    private bool isGameOver = false;

    private void Start()
    {
        Time.timeScale = 1f;

        // 最初はPanelを非表示にしておく
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "GameOverPanelが設定されていません。"
            );
        }
    }

    // ゲームオーバー
    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("GAME OVER");

        if (gameOverPanel != null)
        {
            // 非表示になっていたPanelを表示する
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "GameOverPanelが設定されていません！"
            );
        }

        Time.timeScale = 0f;
    }

    // リトライ
    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // タイトルへ戻る
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("TitleScene");
    }
}
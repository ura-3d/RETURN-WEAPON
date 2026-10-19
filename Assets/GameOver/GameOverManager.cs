using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("ゲームオーバー画面")]
    [SerializeField] private GameObject gameOverPanel;

    private bool isGameOver = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
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

        SceneManager.LoadScene("Title");
    }
}
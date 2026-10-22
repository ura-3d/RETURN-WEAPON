using UnityEngine;
using UnityEngine.SceneManagement;

// ゲームクリア画面を管理する
// ※Panelとは別の「常に表示されているオブジェクト」にアタッチすること
//   （Panelと同じオブジェクトに付けると、非表示の間はスクリプトが動かない）
public class GameClearManager : MonoBehaviour
{
    [Header("ゲームクリア画面")]
    [SerializeField] private GameObject gameClearPanel;

    private bool isGameClear = false;

    private void Start()
    {
        Time.timeScale = 1f;

        // 最初はPanelを非表示にしておく
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "GameClearPanelが設定されていません。"
            );
        }
    }

    // ゲームクリア
    public void GameClear()
    {
        if (isGameClear)
            return;

        isGameClear = true;

        Debug.Log("GAME CLEAR");

        if (gameClearPanel != null)
        {
            // 非表示になっていたPanelを表示する
            gameClearPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "GameClearPanelが設定されていません！"
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

    // ステージ選択へ
    public void GoToSelectScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("SelectScene");
    }
}
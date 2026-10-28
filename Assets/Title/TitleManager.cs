using UnityEngine;
using UnityEngine.SceneManagement;

// タイトル画面を管理する
// Startボタン → ステージ選択画面へ移動
// Closeボタン → ゲームを終了
// ReturnToTitle → タイトル画面（TitleScene）へ戻る
public class TitleManager : MonoBehaviour
{
    [Header("移動先のシーン名")]
    [SerializeField] private string stageSelectSceneName = "StageSelect";

    // タイトル画面のシーン名（Projectのシーンファイル名と完全に一致させる）
    [SerializeField] private string titleSceneName = "TitleScene";

    private void Start()
    {
        // 前のシーンで止まったままにならないよう戻しておく
        Time.timeScale = 1f;
    }

    // Startボタン：ステージ選択画面へ
    public void StartGame()
    {
        // Build Profiles のシーン一覧に入っていないシーンは読み込めない
        if (!Application.CanStreamedLevelBeLoaded(stageSelectSceneName))
        {
            Debug.LogError(
                "シーン「" + stageSelectSceneName + "」が見つかりません。" +
                "シーン名と、Build Profiles のシーン一覧への追加を確認してください。"
            );
            return;
        }

        Time.timeScale = 1f;

        Debug.Log("ステージ選択画面へ移動");

        SceneManager.LoadScene(stageSelectSceneName);
    }

    // タイトル画面へ戻る
    public void ReturnToTitle()
    {
        // Build Profiles のシーン一覧に入っていないシーンは読み込めない
        if (!Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            Debug.LogError(
                "シーン「" + titleSceneName + "」が見つかりません。" +
                "シーン名と、Build Profiles のシーン一覧への追加を確認してください。"
            );
            return;
        }

        Time.timeScale = 1f;

        Debug.Log("タイトル画面へ戻ります");

        SceneManager.LoadScene(titleSceneName);
    }

    // Closeボタン：ゲームを終了
    public void QuitGame()
    {
        Debug.Log("ゲームを終了します");

#if UNITY_EDITOR
        // エディタ上では再生を停止する（Application.Quitはビルド後のみ有効）
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
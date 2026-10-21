using UnityEngine;
using UnityEngine.SceneManagement;

// タイトル画面を管理する
// Startボタン → ステージ選択画面へ移動
// Closeボタン → ゲームを終了
public class TitleManager : MonoBehaviour
{
    [Header("移動先のシーン名")]
    [SerializeField] private string stageSelectSceneName = "StageSelect";

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
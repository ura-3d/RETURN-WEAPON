using UnityEngine;
using UnityEngine.SceneManagement;

// ステージ選択画面を管理する
// ボタンのOnClickから LoadStage("シーン名") を呼んでステージへ移動する
public class StageSelectManager : MonoBehaviour
{
    private void Start()
    {
        // 前のシーンで止まったままにならないよう戻しておく
        Time.timeScale = 1f;
    }

    // 指定したシーン（ステージ）へ移動
    public void LoadStage(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("シーン名が空です。OnClickでシーン名を入力してください。");
            return;
        }

        // Build Profiles のシーン一覧に入っていないシーンは読み込めない
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "シーン「" + sceneName + "」が見つかりません。" +
                "シーン名と、Build Profiles のシーン一覧への追加を確認してください。"
            );
            return;
        }

        Time.timeScale = 1f;

        Debug.Log("ステージへ移動：" + sceneName);

        SceneManager.LoadScene(sceneName);
    }

    // タイトルへ戻る
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Title");
    }
}
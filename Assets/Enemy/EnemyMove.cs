using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    [Header("敵データ")]
    [SerializeField] private EnemyData enemyData;

    private Transform player;

    private void Start()
    {
        // Playerを探す
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "Playerタグのオブジェクトが見つかりません。"
            );
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        if (enemyData == null)
            return;

        // プレイヤーとの距離
        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        // 検知範囲外なら何もしない
        if (distance > enemyData.detectDistance)
            return;

        // 攻撃距離まで近づいたら停止
        if (distance <= enemyData.attackDistance)
            return;

        // プレイヤーの方向
        Vector2 direction =
            (player.position - transform.position)
            .normalized;

        // 移動
        transform.position +=
            (Vector3)direction
            * enemyData.moveSpeed
            * Time.deltaTime;

        // プレイヤーの方向を向く
        if (direction.x > 0)
        {
            transform.localScale =
                new Vector3(1, 1, 1);
        }
        else if (direction.x < 0)
        {
            transform.localScale =
                new Vector3(-1, 1, 1);
        }
    }
}
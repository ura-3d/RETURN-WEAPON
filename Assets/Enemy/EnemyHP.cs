using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    [Header("敵データ")]
    [SerializeField] private EnemyData enemyData;

    private int currentHP;

    public int CurrentHP => currentHP;
    public int MaxHP => enemyData != null ? enemyData.maxHP : 0;

    private void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError(
                "EnemyHPにEnemyDataが設定されていません。"
            );

            return;
        }

        currentHP = enemyData.maxHP;

        Debug.Log(
            enemyData.enemyName +
            " HP：" +
            currentHP
        );
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        currentHP -= damage;

        Debug.Log(
            enemyData.enemyName +
            " が " +
            damage +
            " ダメージを受けた！ HP：" +
            currentHP +
            "/" +
            enemyData.maxHP
        );

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(
            enemyData.enemyName +
            " を倒した！"
        );

        Destroy(gameObject);
    }
}
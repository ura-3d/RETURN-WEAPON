using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    [Header("HP設定")]
    [SerializeField] private int maxHP = 30;

    private int currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        Debug.Log("敵が " + damage + " ダメージを受けた！ HP：" + currentHP);

        if (currentHP <= 0) 
        {
            Die();
        }
    }

    private void Die() 
    {
        Destroy(gameObject);
    }
}

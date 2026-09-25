using UnityEngine;

public class MeleeAttackPoint : MonoBehaviour
{
    [SerializeField]
    private MeleeEnemy m_enemy;

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (m_enemy == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        m_enemy.AttackHit(
            other.gameObject
        );
    }
}
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyData",
    menuName = "Enemy/EnemyData"
)]
public class EnemyData : ScriptableObject
{
    [Header("基本情報")]
    public string enemyName = "Enemy";

    [Header("HP")]
    public int maxHP = 30;

    [Header("移動")]
    public float moveSpeed = 2f;

    [Header("プレイヤー検知")]
    public float detectDistance = 5f;

    [Header("攻撃")]
    public float attackDistance = 1.2f;
    public int attackDamage = 5;
    public float attackInterval = 1.5f;
}
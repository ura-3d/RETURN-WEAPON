using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName ="Enemy/BossData")]

public class BossData : ScriptableObject
{
    [Header("基本設定")]
    public string bossName = "Boss";

    [Header("HP")]
    public int MaxHP = 500;

    [Header("移動")]
    public float moveSpeed = 2f;

    [Header("検知")]
    public float detectDistance = 10f;

    [Header("攻撃")]
    public float attackDistance = 2f;
    public int attackDamage = 20;
    public float attackInterval = 2f;
    public float attackDuration = 0.5f;

    [Header("攻撃後")]
    public float recoveryTime = 1f;

    [Header("フェーズ")]
    public int phase2HP = 250;
    public int phase3HP = 100;
}

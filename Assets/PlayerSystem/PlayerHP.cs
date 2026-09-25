using UnityEngine;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 100;

    [Header("ダメージ無敵時間")]
    [SerializeField] private float damageInvincibleTime = 0.5f;

    [Header("ダメージ点滅")]
    [SerializeField] private float blinkInterval = 0.1f;

    private int currentHP;
    private float invincibleTimer;

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHP = maxHP;

        Debug.Log(
            "Player HP：" +
            currentHP +
            "/" +
            maxHP
        );
    }

    private void Update()
    {
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        // 無敵時間中
        if (invincibleTimer > 0f)
        {
            Debug.Log(
                "Playerは無敵時間中です"
            );

            return;
        }

        currentHP -= damage;

        if (currentHP < 0)
        {
            currentHP = 0;
        }

        // 無敵時間開始
        invincibleTimer =
            damageInvincibleTime;

        Debug.Log(
            "Playerが " +
            damage +
            " ダメージを受けた！ HP：" +
            currentHP +
            "/" +
            maxHP
        );

        // 点滅開始
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine =
            StartCoroutine(DamageBlink());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageBlink()
    {
        if (spriteRenderer == null)
            yield break;

        float timer = 0f;

        while (timer < damageInvincibleTime)
        {
            spriteRenderer.enabled = false;

            yield return new WaitForSeconds(
                blinkInterval
            );

            spriteRenderer.enabled = true;

            yield return new WaitForSeconds(
                blinkInterval
            );

            timer += blinkInterval * 2f;
        }

        spriteRenderer.enabled = true;

        blinkCoroutine = null;
    }

    private void Die()
    {
        Debug.Log(
            "Playerが倒れた！"
        );
    }
}
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

    // 死亡状態
    private bool m_isDead;

    private Rigidbody2D m_rb;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    public bool IsDead => m_isDead;

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        m_rb =
            GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHP = maxHP;

        m_isDead = false;

        Debug.Log(
            "Player HP：" +
            currentHP +
            "/" +
            maxHP
        );
    }

    private void Update()
    {
        // 死亡していたら何もしない
        if (m_isDead)
            return;

        if (invincibleTimer > 0f)
        {
            invincibleTimer -=
                Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        // 死亡中はダメージを受けない
        if (m_isDead)
            return;

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
            StopCoroutine(
                blinkCoroutine
            );
        }

        blinkCoroutine =
            StartCoroutine(
                DamageBlink()
            );

        // HPが0になった
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

            timer +=
                blinkInterval * 2f;
        }

        spriteRenderer.enabled = true;

        blinkCoroutine = null;
    }

    // =================================
    // 死亡処理
    // =================================
    private void Die()
    {
        if (m_isDead)
            return;

        m_isDead = true;

        Debug.Log(
            "Playerが倒れた！"
        );

        // 無敵時間を解除
        invincibleTimer = 0f;

        // 点滅を停止
        if (blinkCoroutine != null)
        {
            StopCoroutine(
                blinkCoroutine
            );

            blinkCoroutine = null;
        }

        // Spriteを表示
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Rigidbodyの移動を停止
        if (m_rb != null)
        {
            m_rb.linearVelocity =
                Vector2.zero;
        }

        // Playerの操作を停止
        DisablePlayerControl();
    }

    // =================================
    // Player操作停止
    // =================================
    private void DisablePlayerControl()
    {
        PlayerMove playerMove =
            GetComponent<PlayerMove>();

        if (playerMove != null)
        {
            playerMove.enabled = false;
        }
    }
}
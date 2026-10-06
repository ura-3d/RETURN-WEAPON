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

    private PlayerMove playerMove;
    private WeaponSpawner weaponSpawner;

    private bool isDead = false;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        playerMove =
            GetComponent<PlayerMove>();

        weaponSpawner =
            GetComponent<WeaponSpawner>();
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

    // ダメージ処理
    public void TakeDamage(int damage)
    {
        if (isDead)
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

        // ダメージ点滅開始
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine =
            StartCoroutine(
                DamageBlink()
            );

        // HPが0になったら死亡
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ダメージ時の点滅
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

    // プレイヤー死亡
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(
            "Playerが倒れた！"
        );

        // プレイヤー操作停止
        if (playerMove != null)
        {
            playerMove.enabled = false;
        }

        // 武器操作停止
        if (weaponSpawner != null)
        {
            weaponSpawner.enabled = false;
        }

        // Rigidbody停止
        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.simulated = false;
        }

        // ゲームオーバー
        GameOverManager gameOverManager =
            FindFirstObjectByType<GameOverManager>();

        if (gameOverManager != null)
        {
            gameOverManager.GameOver();
        }
        else
        {
            Debug.LogWarning(
                "GameOverManagerがSceneにありません。"
            );
        }
    }
}
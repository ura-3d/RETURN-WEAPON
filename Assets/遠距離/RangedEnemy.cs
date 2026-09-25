using UnityEngine;

public class RangedEnemy : EnemyBase
{
    protected RangedEnemyData m_rangedData;

    [Header("遠距離攻撃")]
    [SerializeField] private Transform m_shootPoint;
    [SerializeField] private GameObject m_projectilePrefab;

    private float m_attackTimer;

    protected override void Awake()
    {
        base.Awake();

        m_rangedData =
            m_data as RangedEnemyData;

        if (m_rangedData == null)
        {
            Debug.LogError(
                "RangedEnemyにRangedEnemyDataが設定されていません。"
            );
        }
    }

    protected override void EnemyUpdate()
    {
        if (m_rangedData == null)
            return;

        if (m_player == null)
            return;

        // クールタイム
        if (m_attackTimer > 0f)
        {
            m_attackTimer -= Time.deltaTime;
        }

        // Playerとの距離
        float distance =
            GetPlayerDistance();

        // =========================
        // 攻撃範囲内
        // =========================
        if (distance <=
            m_rangedData.attackDistance)
        {
            // 移動停止
            StopMove();

            // Playerの方向を向く
            Vector2 direction =
                GetPlayerDirection();

            LookAtPlayer(direction);

            // クールタイムが終わったら発射
            if (m_attackTimer <= 0f)
            {
                Shoot();
            }

            return;
        }

        // =========================
        // 検知範囲内
        // =========================
        if (distance <=
            m_rangedData.detectDistance)
        {
            MoveToPlayer();
            return;
        }

        // =========================
        // 検知範囲外
        // =========================
        StopMove();
    }

    private void MoveToPlayer()
    {
        Vector2 direction =
            GetPlayerDirection();

        m_rb.linearVelocity =
            new Vector2(
                direction.x *
                m_rangedData.moveSpeed,
                m_rb.linearVelocity.y
            );

        LookAtPlayer(direction);
    }

    private void StopMove()
    {
        m_rb.linearVelocity =
            new Vector2(
                0f,
                m_rb.linearVelocity.y
            );
    }

    private void Shoot()
    {
        if (m_shootPoint == null)
        {
            Debug.LogError(
                "ShootPointが設定されていません！"
            );

            return;
        }

        if (m_projectilePrefab == null)
        {
            Debug.LogError(
                "Projectile Prefabが設定されていません！"
            );

            return;
        }

        // Playerへの方向
        Vector2 direction =
            GetPlayerDirection();

        // ShootPointの位置から弾を出す
        GameObject projectile =
            Instantiate(
                m_projectilePrefab,
                m_shootPoint.position,
                Quaternion.identity
            );

        EnemyProjectile enemyProjectile =
            projectile.GetComponent<EnemyProjectile>();

        if (enemyProjectile != null)
        {
            enemyProjectile.Initialize(
                direction,
                m_rangedData.projectileSpeed,
                m_rangedData.projectileLifeTime,
                m_rangedData.attackDamage
            );
        }

        Debug.Log(
            "遠距離敵が弾を発射！"
        );

        // クールタイム
        m_attackTimer =
            m_rangedData.attackInterval;
    }
}
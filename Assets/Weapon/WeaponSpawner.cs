using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("武器")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("武器を持つ位置")]
    [SerializeField] private Transform weaponSpawnPoint;

    // 現在の武器
    private GameObject currentWeapon;


    // ========================================
    // 初期化
    // ========================================
    private void Start()
    {
        // 最初は武器を持っている状態
        if (weaponSpawnPoint != null &&
            weaponPrefab != null)
        {
            EquipNewWeapon();
        }
    }


    // ========================================
    // 武器を投げる
    // ========================================
    private void Update()
    {
        // 左クリック
        if (Input.GetMouseButtonDown(0))
        {
            // 武器を持っている場合
            if (currentWeapon != null &&
                currentWeapon.transform.parent ==
                weaponSpawnPoint)
            {
                ThrowWeapon();
            }
        }
    }


    // ========================================
    // 新しく武器を生成
    // ========================================
    private void EquipNewWeapon()
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning(
                "Weapon Prefabが設定されていません。"
            );

            return;
        }

        if (weaponSpawnPoint == null)
        {
            Debug.LogWarning(
                "Weapon Spawn Pointが設定されていません。"
            );

            return;
        }

        // 武器を生成
        currentWeapon = Instantiate(
            weaponPrefab,
            weaponSpawnPoint.position,
            weaponSpawnPoint.rotation
        );

        // 手元に装備
        EquipWeapon(currentWeapon);
    }


    // ========================================
    // 武器を投げる
    // ========================================
    private void ThrowWeapon()
    {
        if (currentWeapon == null)
            return;

        Weapon weapon =
            currentWeapon.GetComponent<Weapon>();

        if (weapon == null)
        {
            Debug.LogWarning(
                "Weaponコンポーネントが見つかりません。"
            );

            return;
        }

        // Playerの向いている方向
        Vector2 direction;

        if (transform.localScale.x > 0)
        {
            direction = Vector2.right;
        }
        else
        {
            direction = Vector2.left;
        }

        // 親から外す
        currentWeapon.transform.SetParent(null);

        // 武器を投げる
        weapon.Throw(
            direction,
            transform
        );

        Debug.Log("武器を投げました");
    }


    // ========================================
    // 武器を装備する
    // ========================================
    public void EquipWeapon(
        GameObject weapon)
    {
        if (weapon == null)
            return;

        if (weaponSpawnPoint == null)
        {
            Debug.LogWarning(
                "Weapon Spawn Pointが設定されていません。"
            );

            return;
        }

        // 現在の武器として登録
        currentWeapon = weapon;

        // Playerの子にする
        weapon.transform.SetParent(
            weaponSpawnPoint
        );

        // 手元の位置
        weapon.transform.localPosition =
            Vector3.zero;

        // 回転をリセット
        weapon.transform.localRotation =
            Quaternion.identity;

        // Rigidbodyを取得
        Rigidbody2D weaponRb =
            weapon.GetComponent<Rigidbody2D>();

        if (weaponRb != null)
        {
            // 武器を停止
            weaponRb.linearVelocity =
                Vector2.zero;

            // 重力OFF
            weaponRb.gravityScale = 0f;

            // Rigidbodyを無効化
            weaponRb.simulated = false;
        }

        Debug.Log("武器を装備しました");
    }


    // ========================================
    // 現在の武器を取得
    // ========================================
    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
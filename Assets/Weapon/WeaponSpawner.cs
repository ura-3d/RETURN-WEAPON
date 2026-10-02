using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("武器")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("武器を持つ位置")]
    [SerializeField] private Transform weaponSpawnPoint;

    // 現在装備している武器
    private GameObject currentWeapon;


    // ========================================
    // 初期化
    // ========================================
    private void Start()
    {
        if (weaponSpawnPoint != null &&
            weaponPrefab != null)
        {
            EquipNewWeapon();
        }
    }


    // ========================================
    // 入力
    // ========================================
    private void Update()
    {
        // 左クリック
        if (Input.GetMouseButtonDown(0))
        {
            if (CanThrowWeapon())
            {
                ThrowWeapon();
            }
        }
    }


    // ========================================
    // 武器を投げられるか
    // ========================================
    private bool CanThrowWeapon()
    {
        if (currentWeapon == null)
            return false;

        if (weaponSpawnPoint == null)
            return false;

        return currentWeapon.transform.parent ==
               weaponSpawnPoint;
    }


    // ========================================
    // 新しい武器を生成
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

        currentWeapon =
            Instantiate(
                weaponPrefab,
                weaponSpawnPoint.position,
                weaponSpawnPoint.rotation
            );

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


        // ====================================
        // Main Camera確認
        // ====================================
        if (Camera.main == null)
        {
            Debug.LogWarning(
                "MainCameraが見つかりません。"
            );

            return;
        }


        // ====================================
        // マウスの画面座標
        // ====================================
        Vector3 mouseScreenPosition =
            Input.mousePosition;


        // ====================================
        // マウスをワールド座標へ変換
        // ====================================
        Vector3 mouseWorldPosition =
            Camera.main.ScreenToWorldPoint(
                mouseScreenPosition
            );

        mouseWorldPosition.z = 0f;


        // ====================================
        // Playerからマウスへの方向
        // ====================================
        Vector2 direction =
            (
                mouseWorldPosition -
                transform.position
            ).normalized;


        // マウスがPlayerとほぼ同じ位置の場合
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }


        // ====================================
        // 投げる武器を保存
        // ====================================
        GameObject thrownWeapon =
            currentWeapon;

        // Playerの所持武器を解除
        currentWeapon = null;


        // ====================================
        // Playerから外す
        // ====================================
        thrownWeapon.transform.SetParent(
            null
        );


        // ====================================
        // Rigidbodyを有効化
        // ====================================
        Rigidbody2D weaponRb =
            thrownWeapon.GetComponent<Rigidbody2D>();

        if (weaponRb != null)
        {
            weaponRb.simulated = true;
        }


        // ====================================
        // 武器を投げる
        // ====================================
        weapon.Throw(
            direction,
            transform
        );

        Debug.Log(
            "マウス方向へ武器を投げました"
        );
    }


    // ========================================
    // 武器を装備
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


        // ====================================
        // 現在の武器として登録
        // ====================================
        currentWeapon =
            weapon;


        // ====================================
        // Playerの子にする
        // ====================================
        weapon.transform.SetParent(
            weaponSpawnPoint
        );


        // ====================================
        // 手元の位置
        // ====================================
        weapon.transform.localPosition =
            Vector3.zero;


        // ====================================
        // 回転リセット
        // ====================================
        weapon.transform.localRotation =
            Quaternion.identity;


        // ====================================
        // Rigidbody
        // ====================================
        Rigidbody2D weaponRb =
            weapon.GetComponent<Rigidbody2D>();

        if (weaponRb != null)
        {
            weaponRb.linearVelocity =
                Vector2.zero;

            weaponRb.gravityScale =
                0f;

            weaponRb.simulated =
                false;
        }


        Debug.Log(
            "武器を装備しました"
        );
    }


    // ========================================
    // 現在の武器を取得
    // ========================================
    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }


    // ========================================
    // 武器を持っているか
    // ========================================
    public bool HasWeapon()
    {
        return currentWeapon != null;
    }
}
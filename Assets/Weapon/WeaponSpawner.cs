using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("武器")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("武器を持つ位置")]
    [SerializeField] private Transform weaponSpawnPoint;

    private GameObject currentWeapon;

    private Animator animator;

    private void Awake()
    {
        animator =
            GetComponent<Animator>();
    }

    private void Start()
    {
        if (weaponSpawnPoint != null &&
            weaponPrefab != null)
        {
            EquipNewWeapon();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (CanThrowWeapon())
            {
                ThrowWeapon();
            }
        }
    }

    // 武器を投げられるか
    private bool CanThrowWeapon()
    {
        if (currentWeapon == null)
            return false;

        if (weaponSpawnPoint == null)
            return false;

        return currentWeapon.transform.parent ==
               weaponSpawnPoint;
    }

    // 新しい武器を装備
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

    // 武器を投げる
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

        if (Camera.main == null)
        {
            Debug.LogWarning(
                "MainCameraが見つかりません。"
            );

            return;
        }

        Vector3 mouseScreenPosition =
            Input.mousePosition;

        Vector3 mouseWorldPosition =
            Camera.main.ScreenToWorldPoint(
                mouseScreenPosition
            );

        mouseWorldPosition.z = 0f;

        Vector2 direction =
            (
                mouseWorldPosition -
                transform.position
            ).normalized;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        GameObject thrownWeapon =
            currentWeapon;

        currentWeapon = null;

        thrownWeapon.transform.SetParent(null);

        Rigidbody2D weaponRb =
            thrownWeapon.GetComponent<Rigidbody2D>();

        if (weaponRb != null)
        {
            weaponRb.simulated = true;
        }

        // Throwアニメーション
        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }

        weapon.Throw(
            direction,
            transform
        );

        Debug.Log(
            "マウス方向へ武器を投げました"
        );
    }

    // 武器を装備
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

        currentWeapon =
            weapon;

        weapon.transform.SetParent(
            weaponSpawnPoint
        );

        weapon.transform.localPosition =
            Vector3.zero;

        weapon.transform.localRotation =
            Quaternion.identity;

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

    // 現在の武器を取得
    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }

    // 武器を持っているか
    public bool HasWeapon()
    {
        return currentWeapon != null;
    }
}
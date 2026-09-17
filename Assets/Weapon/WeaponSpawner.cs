using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("武器")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("生成位置")]
    [SerializeField] private Transform weaponSpawnPoint;

    // 現在存在している武器
    private GameObject currentWeapon;

    private void Update()
    {
        // 武器が存在している場合は新しく投げない
        if (currentWeapon != null)
            return;

        // 左クリックで武器を投げる
        if (Input.GetMouseButtonDown(0))
        {
            SpawnWeapon();
        }
    }

    private void SpawnWeapon()
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning("Weapon Prefabが設定されていません。");
            return;
        }

        if (weaponSpawnPoint == null)
        {
            Debug.LogWarning("Weapon Spawn Pointが設定されていません。");
            return;
        }

        currentWeapon = Instantiate(
            weaponPrefab,
            weaponSpawnPoint.position,
            weaponSpawnPoint.rotation
        );

        Weapon weapon = currentWeapon.GetComponent<Weapon>();

        if (weapon != null)
        {
            Vector2 direction;

            if (transform.localScale.x > 0)
            {
                direction = Vector2.right;
            }
            else
            {
                direction = Vector2.left;
            }

            weapon.Throw(direction, transform);
        }
    }
}
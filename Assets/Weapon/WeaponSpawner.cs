using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("武器")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("生成位置")]
    [SerializeField] private Transform weaponSpawnPoint;

    private void Update()
    {
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

        GameObject weaponObject = Instantiate(
            weaponPrefab,
            weaponSpawnPoint.position,
            weaponSpawnPoint.rotation
        );

        Weapon weapon = weaponObject.GetComponent<Weapon>();

        if (weapon != null)
        {
            Vector2 direction;

            if (transform.localScale.x > 0)
            {
                // 右向き
                direction = Vector2.right;
            }
            else
            {
                // 左向き
                direction = Vector2.left;
            }

            weapon.Throw(direction, transform);
        }
    }
}
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("武器")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("武器を持つ位置")]
    [SerializeField] private Transform weaponSpawnPoint;

    [Header("レティクル")]
    // 左クリックを押している間、マウスの位置に表示する照準（シーン上に置いたオブジェクト）
    [SerializeField] private GameObject reticle;

    [Header("投げる方向")]
    // オンにすると、Playerの向きと反対側にマウスがあっても、向いている側へ照準を寄せる
    // （上下の位置はマウスのまま、左右だけ向いている側に揃える）
    // オフにすると、マウスの位置へ自由に狙える
    [SerializeField] private bool restrictToFacing = true;

    private GameObject currentWeapon;

    private Animator animator;
    private PlayerMove playerMove;

    // 左クリックを押して狙っている最中か
    private bool isAiming;

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        playerMove =
            GetComponent<PlayerMove>();
    }

    private void Start()
    {
        // 最初はレティクルを隠しておく
        SetReticleVisible(false);

        if (weaponSpawnPoint != null &&
            weaponPrefab != null)
        {
            EquipNewWeapon();
        }
    }

    private void Update()
    {
        UpdateAiming();
    }

    // 狙い：押している間レティクルを表示し、離したらその位置へ投げる
    private void UpdateAiming()
    {
        // 武器を持っていないときは狙えない
        if (!CanThrowWeapon())
        {
            isAiming = false;
            SetReticleVisible(false);
            return;
        }

        // 左クリックを押したら狙い始める
        if (Input.GetMouseButtonDown(0))
        {
            isAiming = true;
        }

        if (!isAiming)
            return;

        Vector2 aimPosition;

        if (!TryGetAimPosition(out aimPosition))
            return;

        // レティクルを照準位置に表示
        SetReticlePosition(aimPosition);

        // 左クリックを離したら、レティクルの位置へ投げる
        if (Input.GetMouseButtonUp(0))
        {
            isAiming = false;
            SetReticleVisible(false);

            ThrowWeapon(aimPosition);
        }
    }

    // 照準の位置（ワールド座標）を求める
    private bool TryGetAimPosition(out Vector2 aimPosition)
    {
        aimPosition = Vector2.zero;

        if (Camera.main == null)
        {
            Debug.LogWarning(
                "MainCameraが見つかりません。"
            );

            return false;
        }

        Vector3 mouseWorldPosition =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );

        aimPosition = mouseWorldPosition;

        // Playerの向いている側に照準を寄せる
        if (restrictToFacing && playerMove != null)
        {
            float facingSign =
                playerMove.IsFacingRight ? 1f : -1f;

            float diffX =
                aimPosition.x - transform.position.x;

            // マウスが向きと反対側なら、Playerを中心に左右だけ反転する
            if (diffX * facingSign < 0f)
            {
                aimPosition.x =
                    transform.position.x - diffX;
            }
        }

        return true;
    }

    // レティクルを指定位置に表示
    private void SetReticlePosition(Vector2 position)
    {
        if (reticle == null)
            return;

        if (!reticle.activeSelf)
        {
            reticle.SetActive(true);
        }

        reticle.transform.position =
            new Vector3(position.x, position.y, 0f);
    }

    // レティクルの表示・非表示
    private void SetReticleVisible(bool visible)
    {
        if (reticle == null)
            return;

        if (reticle.activeSelf != visible)
        {
            reticle.SetActive(visible);
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

    // 武器を投げる（aimPosition：狙う位置）
    private void ThrowWeapon(Vector2 aimPosition)
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

        // 狙う位置が武器に近すぎるときは投げない
        Vector2 toTarget =
            aimPosition -
            (Vector2)currentWeapon.transform.position;

        if (toTarget.sqrMagnitude <= 0.01f)
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
            animator.ResetTrigger("Throw");
            animator.SetTrigger("Throw");
        }

        // レティクルの位置に届くように投げる
        weapon.ThrowToTarget(
            aimPosition,
            transform
        );

        Debug.Log(
            "レティクルの位置へ武器を投げました"
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
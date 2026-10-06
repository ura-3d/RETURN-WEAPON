using UnityEngine;

public class Switch : MonoBehaviour
{
    [Header("スイッチ設定")]
    [SerializeField] private bool isOn = false;

    [Header("連動するドア")]
    [SerializeField] private Door targertDoor;

    public bool IsOn => isOn;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(
            "スイッチに衝突：" +
            collision.gameObject.name
        );

        // 武器か確認
        Weapon weapon =
            collision.gameObject.GetComponent<Weapon>();

        if (weapon != null)
        {
            TurnOn();
            return;
        }

        // Playerは無視
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Playerがスイッチに当たりました");
        }
    }

    // スイッチをONにする
    private void TurnOn()
    {
        if (isOn)
            return;

        isOn = true;

        Debug.Log(
            "スイッチがONになりました！"
        );

        transform.localScale =
            new Vector3(1.2f, 1.2f, 1f);

        if (targertDoor != null) 
        {
            targertDoor.OpenDoor();
        }
        else
        {
            Debug.LogWarning("Doorが設定されてないよ");
        }
    }
}
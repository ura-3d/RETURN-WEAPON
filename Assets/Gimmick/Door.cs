using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("ドア設定")]
    [SerializeField] private bool isOpen = false;

    public bool IsOpen => isOpen;

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        Debug.Log("ドアが開きました");

        gameObject.SetActive(false);
    }
}


using UnityEngine;

public class CloseCanvas : MonoBehaviour
{
    [SerializeField] private GameObject targetCanvas;

    public void Close()
    {
        targetCanvas.SetActive(false);
    }
}


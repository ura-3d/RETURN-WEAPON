using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("ÉJÉÅÉâÇÃí«è]ë¨ìx")]
    [SerializeField] private float smoothSpeed = 5f;

    private float fixedY;

    private void Start()
    {
        fixedY = transform.position.y;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            fixedY,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
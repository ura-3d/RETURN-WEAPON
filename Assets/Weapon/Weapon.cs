using UnityEngine;


public class Weapon : MonoBehaviour
{
    [Header("飛行設定")]
    [SerializeField] private float throwSpeed = 10f;


    [Header("戻る設定")]
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float returnSpeed = 12f;

    private Rigidbody2D rb;

    private Transform player;

    private Vector3 startPosition;

    private bool isReturning = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    public void Throw(Vector2 direction, Transform playerTransform)
    {
        player = playerTransform;

        startPosition = transform.position;

        rb.linearVelocity = direction.normalized * throwSpeed;
    }

    private void Update()
    {
        //いなければなにもしない
        if (player == null)
            return;

        //戻る
        if(isReturning)
        {
            Vector2 direction=
                (player.position - transform.position).normalized;

            rb.linearVelocity = direction * returnSpeed;

            //プレイヤーまで戻ったら削除
            if(Vector2.Distance(transform.position,player.position) <0.5f)
            {
                Destroy(gameObject);
            }

            return;
        }

           //投げた位置から一定距離はなれたら戻る
           float distance =
            Vector2.Distance(startPosition, player.position);

        if(distance >= maxDistance)
        {
            isReturning = true;
        }
    }
}

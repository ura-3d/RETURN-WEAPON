using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("É_ÉÅÅ[ÉVê›íË")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f;

    private Coroutine damageCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHP playerHP = 
            other.GetComponent<PlayerHP>();

        if (playerHP == null)
            return;

        damageCoroutine =
            StartCoroutine(DamagePlayer(playerHP));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        StopDamage();
    }

    private IEnumerator DamagePlayer(PlayerHP playerHP)
    {
        while (true) 
        {
            if (playerHP == null)
                break;

            playerHP.TakeDamage(damage);

            yield return new WaitForSeconds(damageInterval);
            
        }

        damageCoroutine = null;
    }

    private void StopDamage()
    {
        if (damageCoroutine == null)
            return;

        StopCoroutine(damageCoroutine);

        damageCoroutine = null;
    }
}

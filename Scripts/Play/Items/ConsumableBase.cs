using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConsumableBase : MonoBehaviour
{
    public float duration = 5f;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
       
            if (Player.instance != null)
            {
                ActivatePowerUp();
                StartCoroutine(DeactivatePowerUpAfterDuration());
            }
        }
    }

    protected abstract void ActivatePowerUp();

    private IEnumerator DeactivatePowerUpAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        DeactivatePowerUp();
        Destroy(gameObject); // Destroy the power-up after it has been used
    }

    protected abstract void DeactivatePowerUp();
}

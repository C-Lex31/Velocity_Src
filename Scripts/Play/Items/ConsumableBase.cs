using System.Collections;
using UnityEngine;

public abstract class ConsumableBase : MonoBehaviour
{
    public float duration = 5f;
    public Sprite icon; // Icon for the consumable
    public string consumableName; // Unique name for the consumable

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.bFlatlined)
            {
                
                ActivatePowerUp();
                if (duration > 0)
                    PlayManager.instance.ActivateConsumable(consumableName, duration, icon, this);
                gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    protected abstract void ActivatePowerUp();
    public abstract void DeactivatePowerUp();
}

using Unity.Mathematics;
using UnityEngine;

public abstract class CollectibleBase : MonoBehaviour
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.bFlatlined)
            {
                ParticleManager.instance.PlayParticleEffect(ParticleList.item_collect , transform.position ,Quaternion.identity);
                OnCollect();
                Destroy(gameObject);
            }
        }
    }

    protected abstract void OnCollect();
}

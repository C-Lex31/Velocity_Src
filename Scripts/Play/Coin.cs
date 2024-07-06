using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour ,IPoolable
{
    /// <summary>
    /// Tracking State: The isOutOfView is a crucial flag that helps in tracking whether a coin has already been marked as out of view. This can prevent multiple checks and actions on the same coin, as the out-of-view check is performed frequently.
    /// </summary>
    public bool isOutOfView;
    private CoinGenerator generator;

    public void OnSpawn()
    {
        // Reset the coin's state if needed
        isOutOfView = true;
    }

    public void OnDespawn()
    {
        // Clean up the coin's state if needed
        isOutOfView = false;
    }
    private void OnTriggerEnter(Collider collision)
    {

        if (collision.GetComponent<Player>() != null)
        {
           

            PlayManager.instance.coins++;
            SoundManager.Instance.PlayEffect(SoundList.sound_play_common_sfx_coincollect);
            generator.ReturnCoin(this);
        }
    }

    /// <summary>
    /// The primary issue that led to this solution was the shared state across multiple instances when using the static Instance approach. By switching to a direct reference method, each coin reliably communicates with its parent generator, ensuring correct removal and pooling behavior. This approach scales correctly with multiple CoinGenerator instances and avoids conflicts and incorrect references inherent in the singleton pattern for this use case.
    /// </summary>
    /// <param name="gen"></param>
        public void SetGenerator(CoinGenerator gen)
    {
        generator = gen;
    }
}

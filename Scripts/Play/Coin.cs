using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour ,IPoolable
{
    public bool isOutOfView;

    public void OnSpawn()
    {
        // Reset the coin's state if needed
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
            //AudioManager.instance.PlaySFX(0);

            PlayManager.instance.coins++;
            SoundManager.Instance.PlayEffect(SoundList.sound_play_common_sfx_coincollect);
            Destroy(gameObject);
        }
    }
}

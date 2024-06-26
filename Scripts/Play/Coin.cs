using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
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

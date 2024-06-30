using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowUpHelper : MonoBehaviour
{
      public bool use = true;
    public Vector2 localPosA = new Vector2(0, -1);
    public Vector2 localPosB = new Vector2(0, 0);
    public AudioClip showupSound;
    public float speed = 10;

    public float detectPlayerDistance = 3;
    bool isActived = false;

    Vector3 globalPosA, globalPosB;
    void Start()
    {
        if (!use)
        {
            Destroy(this);
            return;
        }

        globalPosA = transform.position + (Vector3)localPosA;
        globalPosB = transform.position + (Vector3)localPosB;

        transform.position = globalPosA;
    }

    private void OnEnable()
    {
       // GameManager.playerRebornEvent += OnPlayerReborn;
    }

    private void OnDisable()
    {
      //  GameManager.playerRebornEvent -= OnPlayerReborn;
    }

    void OnPlayerReborn()
    {
        isActived = false;
        transform.position = globalPosA;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActived)
        {
            transform.position = Vector3.MoveTowards(transform.position, globalPosB, speed * Time.deltaTime);
        }else if (Vector2.Distance(Player.instance.transform.position, transform.position) <= detectPlayerDistance)
        {
            isActived = true;
            SoundManager.Instance.PlayEffect(SoundList.sound_common_sfx_swipe);
        }
    }

    private void OnDrawGizmos()
    {
        if (use)
            Gizmos.DrawWireSphere(transform.position, detectPlayerDistance);
    }
}

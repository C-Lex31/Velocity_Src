using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Player player;
    private bool canDetect = true;

    BoxCollider boxCd => GetComponent<BoxCollider>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
          if(Player.instance != null && canDetect)
            Player.instance.bLedgeDetected = Physics.OverlapSphere(transform.position, radius, whatIsGround).Length>0;

      //  if(enemy != null && canDetect)
          //  enemy.ledgeDetected = Physics2D.OverlapCircle(transform.position, radius, whatIsGround);
    }
      private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            canDetect = false;
    }

    private void OnTriggerExit(Collider collision)
    {
        Collider[] colliders = Physics.OverlapBox(boxCd.bounds.center, boxCd.size,Quaternion.identity);
/*
        foreach (var hit in colliders)
        {
            if (hit.gameObject.GetComponent<PlatformController>() != null)
                return;
        }
*/
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            canDetect = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,radius);
    }
}

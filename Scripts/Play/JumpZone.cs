using UnityEngine;

public class JumpZone : MonoBehaviour
{
    [SerializeField]private float jumpZoneForce=30 ; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            if (Player.instance != null)
            {
                Player.instance.jumpCount =0;
                Player.instance.DoJump(jumpZoneForce);
            }
        }
    }
}

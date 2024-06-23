using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
    
            if (Player.instance != null)
            {
               // Player.instance.StartVault(transform.position);
            }
        }
    }
}

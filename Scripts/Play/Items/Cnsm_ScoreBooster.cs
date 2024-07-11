using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cnsm_ScoreBooster: ConsumableBase
{
    public float magnetRadius = 5f; // Radius within which coins are attracted
    public float magnetForce = 10f; // Speed at which coins move toward the player
    public LayerMask coinLayer; // Layer mask to identify coins
    private bool isMagnetActive = false;

    protected override void ActivatePowerUp()
    {
        isMagnetActive = true;
    }

    public override void DeactivatePowerUp()
    {
        isMagnetActive = false;
    }

    void Update()
    {
        if (isMagnetActive)
        {
           // AttractCoins();
        }
    }

    private void AttractCoins()
    {
        Collider[] coins = Physics.OverlapSphere(Player.instance.transform.position, magnetRadius, coinLayer);
        for (int i = 0; i < coins.Length; i++)
        {
            Coin returnCoin = coins[i].GetComponent<Coin>();
            Vector3 direction = (Player.instance.transform.position + (Vector3.up * 1) - returnCoin.transform.position).normalized;
            returnCoin.transform.position += direction * magnetForce * Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Player.instance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Player.instance.transform.position, magnetRadius);
        }
    }
}

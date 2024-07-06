using UnityEngine;

public class ParkourPickup : ConsumableBase
{
    public AnimationClip parkourAnimation;

    protected override void ActivatePowerUp()
    {
        // Trigger the parkour animation here
        Debug.Log(parkourAnimation);
        Player.instance.TriggerParkourAnimation(parkourAnimation);
    }

    protected override void DeactivatePowerUp()
    {
        // Handle deactivation logic if needed
        // For example, reset any temporary changes made during the power-up
    }
}

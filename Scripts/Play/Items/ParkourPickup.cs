using UnityEngine;

public class ParkourPickup : ConsumableBase
{
    public AnimationClip parkourAnimation;
    public enum MoveType { None, Slide, Jump, Vault }
    [SerializeField] private MoveType moveType = MoveType.None;
    [SerializeField] private float transitionDuration=0.2f;
    [SerializeField] private bool bScaleCapsule;
    protected override void ActivatePowerUp()
    {
        ParticleManager.instance.PlayParticleEffect(ParticleList.item_collect_parkour , transform.position ,Quaternion.identity);
        if (moveType == MoveType.Slide && Player.instance.bIsSliding) return;
        // Trigger the parkour animation here
        Player.instance.TriggerParkourAnimation(parkourAnimation, transitionDuration, bScaleCapsule);
    }

    public override void DeactivatePowerUp()
    {
        // Handle deactivation logic if needed
        // For example, reset any temporary changes made during the power-up
    }
}

using UnityEngine;

public class ParkourPickup : MonoBehaviour
{
    public AnimationClip parkourAnimation;
    public enum MoveType { None, Slide, Jump, Vault }
    [SerializeField] private MoveType moveType = MoveType.None;
    [SerializeField] private float transitionDuration=0.2f;
    [SerializeField] private bool bScaleCapsule;



     private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.bFlatlined)
            {
                
              Player.instance.currentParkourPickup =this;
            }
        }
    }
  private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.bFlatlined)
            {
                
              Player.instance.currentParkourPickup =null;
            }
        }
    }
    public void ActivateParkour()
    {
        Player.instance.currentParkourPickup =null;
        ParticleManager.instance.PlayParticleEffect(ParticleList.item_collect_parkour , transform.position ,Quaternion.identity);
      //  if (moveType == MoveType.Slide && Player.instance.bIsSliding) return;
        // Trigger the parkour animation here
        Player.instance.TriggerParkourAnimation(parkourAnimation, transitionDuration, bScaleCapsule);
    }

   
}

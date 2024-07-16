
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Range(0,1)]
    [SerializeField] protected float spawnChance=0.5f;
    Animator anim;
    void Start()
    {
        bool canSpawn = spawnChance > Random.value;
        //Spawn chance will depend on score or distance 
        if (!canSpawn)
            Destroy(gameObject);
        else
            anim = GetComponent<Animator>();

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.bFlatlined)
            {
                anim.Play("Disappear");
                TeleportSystem.instance.TriggerTeleport(this);
                GetComponent<BoxCollider>().enabled = false;
            }
        }
    }
    public void Appear()
    {
        anim.Play("Appear");
        Destroy(this.gameObject, 1f);

    }
    void HidePortal()
    {
        gameObject.SetActive(false);
    }
    void ShowPortal()
    {
        gameObject.SetActive(true);
    }

}


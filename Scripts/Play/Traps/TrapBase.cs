using UnityEngine;

public abstract class TrapBase : MonoBehaviour
{
  [SerializeField] protected float spawnChance;

  //  public virtual void DrawGizmos(Vector3 position, Quaternion rotation, Vector3 scale){}

  protected virtual void Start()
  {
    bool canSpawn = spawnChance > Random.value;
    if (!canSpawn)
      Destroy(gameObject);
  }
}

//This trigger will reset all child object when the player reborn

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHelper : MonoBehaviour
{
    [Header("---This trigger will reset all child object when the player reborn---")]
    public GameObject[] target;
    bool isWorked = false;

      GameObject cloneObj;

    private void Awake()
    {
        foreach (var obj in target)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                obj.transform.parent = transform;
            }
        }
    }

   

    private void Start()
    {
        cloneObj = Instantiate(gameObject, transform.position, Quaternion.identity);
        cloneObj.SetActive(false);
    }

    void OnPlayerReborn()
    {
        cloneObj.SetActive(true);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isWorked)
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            isWorked = true;
            foreach (var obj in target)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (target.Length > 0)
        {
            foreach (var obj in target)
            {
                if (obj != null)
                    Gizmos.DrawLine(transform.position, obj.transform.position);
            }
        }
    }
}

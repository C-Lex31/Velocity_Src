using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    Collider[] ragdollColliders;
    Rigidbody[] limbRigidBodies;
    public CapsuleCollider col;
    public Rigidbody rib;
    void Start()
    {
      
        ragdollColliders = GetComponentsInChildren<Collider>();
        limbRigidBodies = GetComponentsInChildren<Rigidbody>();
        RagdollReset();
    }

    public void RagdollStart()
    {
       

        foreach (Collider col in ragdollColliders)
            col.enabled = true;
        foreach (Rigidbody rb in limbRigidBodies)
            rb.isKinematic = false;
 
        GetComponent<Animator>().enabled = false;
        GetComponentInParent<CapsuleCollider>().enabled= false;
        GetComponentInParent<Rigidbody>().isKinematic= true;
    }

    public void RagdollReset()
    {
        foreach (Collider col in ragdollColliders)
            col.enabled = false;
        foreach (Rigidbody rb in limbRigidBodies)
        {
            rb.isKinematic = true;
        }

        GetComponent<Animator>().enabled = true;
        GetComponentInParent<CapsuleCollider>().enabled= true;
        GetComponentInParent<Rigidbody>().isKinematic= false;
    }
    public void ResetTransform(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}

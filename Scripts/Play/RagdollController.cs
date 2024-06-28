using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    Collider[] ragdollColliders;
    Rigidbody[] limbRigidBodies;
    void Start()
    {
        ragdollColliders = GetComponentsInChildren<Collider>();
        limbRigidBodies = GetComponentsInChildren<Rigidbody>();
        RagdollReset();
    }

    public void RagdollStart()
    {
        GetComponent<Animator>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        foreach (Collider col in ragdollColliders)
            col.enabled = true;
        foreach (Rigidbody rb in limbRigidBodies)
            rb.isKinematic = false;
    }

    public void RagdollReset()
    {
        foreach (Collider col in ragdollColliders)
            col.enabled = false;
        foreach (Rigidbody rb in limbRigidBodies)
        {
 rb.isKinematic = true;
 //rb.useGravity =false;
        }
           

        GetComponent<Animator>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }
}

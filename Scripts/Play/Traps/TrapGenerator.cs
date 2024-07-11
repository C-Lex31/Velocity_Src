using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TrapInfo
{
    public GameObject trapPrefab;
    public float weight;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scaleOffset = Vector3.one;  // Default scale is 1
    public bool canBeParkoured;
    public GameObject parkourPickupPrefab;
    public float distanceToPickup;

}

public class TrapGenerator : MonoBehaviour
{
    public List<TrapInfo> traps = new List<TrapInfo>();
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;
    public int selectedTrapIndex = 0;

    private void Start()
    {
        SpawnTrap();
    }

    private void SpawnTrap()
    {
        if (traps.Count == 0)
        {
            Debug.LogWarning("No traps available to spawn.");
            return;
        }

        if (Random.value <= spawnChance)
        {
            TrapInfo selectedTrap = GetRandomTrapByWeight();
            if (selectedTrap != null)
            {
                // Spawn the obstacle
                GameObject trap = Instantiate(selectedTrap.trapPrefab, transform.position + selectedTrap.positionOffset, Quaternion.Euler(selectedTrap.rotationOffset));
                trap.transform.localScale = selectedTrap.scaleOffset;
                trap.transform.parent = transform;

                // If the obstacle can be parkoured, spawn the parkour pickup
                if (selectedTrap.canBeParkoured && selectedTrap.parkourPickupPrefab != null)
                {
                    Vector3 pickupPosition = transform.position + selectedTrap.positionOffset + new Vector3(selectedTrap.distanceToPickup, 0, 0);
                    GameObject parkourPickup = Instantiate(selectedTrap.parkourPickupPrefab, pickupPosition, Quaternion.identity);
                    parkourPickup.transform.parent = transform;
                }
            }
        }
    }

    private TrapInfo GetRandomTrapByWeight()
    {
        float totalWeight = 0f;
        foreach (TrapInfo trap in traps)
        {
            totalWeight += trap.weight;
        }

        float randomValue = Random.value * totalWeight;
        float cumulativeWeight = 0f;

        foreach (TrapInfo trap in traps)
        {
            cumulativeWeight += trap.weight;
            if (randomValue <= cumulativeWeight)
            {
                return trap;
            }
        }

        return null; // This should never happen if weights are properly set
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (selectedTrapIndex >= 0 && selectedTrapIndex < traps.Count)
        {
            TrapInfo trapInfo = traps[selectedTrapIndex];
            Vector3 basePosition = transform.position + trapInfo.positionOffset;
            Quaternion baseRotation = Quaternion.Euler(trapInfo.rotationOffset);
            Vector3 baseScale = trapInfo.scaleOffset;

            if (trapInfo.trapPrefab != null)
            {
                MeshFilter[] meshFilters = trapInfo.trapPrefab.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    Vector3 meshPosition = basePosition + baseRotation * meshFilter.transform.localPosition;
                    Quaternion meshRotation = baseRotation * meshFilter.transform.localRotation;
                    Vector3 meshScale = Vector3.Scale(baseScale, meshFilter.transform.localScale);

                    Gizmos.DrawWireMesh(meshFilter.sharedMesh, meshPosition, meshRotation, meshScale);
                }
            }

            // Draw parkour pickup if the trap can be parkoured
            if (trapInfo.canBeParkoured && trapInfo.parkourPickupPrefab != null)
            {
                Vector3 pickupPosition = basePosition + new Vector3(trapInfo.distanceToPickup, 0, 0);
                Gizmos.DrawWireSphere(pickupPosition, 0.5f); // Represent pickup with a sphere for visualization
            }
        }
    }
}

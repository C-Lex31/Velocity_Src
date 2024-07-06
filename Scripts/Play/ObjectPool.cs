using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Queue<(T obj, float returnTime)> objects = new Queue<(T obj, float returnTime)>();
    private readonly HashSet<T> activeObjects = new HashSet<T>();
    private float lastCleanupTime;
    private readonly float cleanupInterval = 20f; // Interval for cleanup in seconds
    private readonly float maxIdleTime = 20f; // Maximum idle time in seconds before cleanup
    private readonly int maxPoolSize; // Maximum number of items in the pool

    public ObjectPool(T prefab, int initialSize, int maxPoolSize)
    {
        this.prefab = prefab;
        this.maxPoolSize = maxPoolSize;
        for (int i = 0; i < initialSize; i++)
        {
            T newObject = GameObject.Instantiate(prefab);
            newObject.gameObject.SetActive(false);
            objects.Enqueue((newObject, Time.time));
        }
        lastCleanupTime = Time.time;
    }

    public T GetObject()
    {
        T obj;
        if (objects.Count > 0)
        {
            (obj, _) = objects.Dequeue();
        }
        else
        {
            obj = GameObject.Instantiate(prefab);
        }

        obj.gameObject.SetActive(true);
        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnSpawn();
        }
        activeObjects.Add(obj);
        return obj;
    }

    public void ReturnObject(T obj)
    {
        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnDespawn();
        }
        obj.gameObject.SetActive(false);
        activeObjects.Remove(obj);

        if (objects.Count >= maxPoolSize)
        {
            Debug.Log($"Destroying Object {obj.gameObject}");
            GameObject.Destroy(obj.gameObject);
        }
        else
        {
            objects.Enqueue((obj, Time.time));
        }

        // Perform cleanup check
        if (Time.time - lastCleanupTime > cleanupInterval)
        {
            CleanupUnusedObjects();
            lastCleanupTime = Time.time;
        }
    }

    public bool ContainsObject(T obj)
    {
        return activeObjects.Contains(obj);
    }

    private void CleanupUnusedObjects()
    {

        while (objects.Count > 0 && Time.time - objects.Peek().returnTime > maxIdleTime )
        {
            (T obj, float returnTime) = objects.Dequeue();
            Debug.Log($"CLEANING UP {obj} (Idle time: {Time.time - returnTime} seconds)");
            GameObject.Destroy(obj.gameObject);
        }
    }
}

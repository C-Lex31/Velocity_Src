using UnityEngine;
using System.Collections.Generic;
public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Queue<T> objects = new Queue<T>();

    public ObjectPool(T prefab, int initialSize)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            T newObject = GameObject.Instantiate(prefab);
            newObject.gameObject.SetActive(false);
            objects.Enqueue(newObject);
        }
    }

    public T GetObject()
    {
        if (objects.Count > 0)
        {
            T obj = objects.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            T newObj = GameObject.Instantiate(prefab);
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public void ReturnObject(T obj)
    {
        obj.gameObject.SetActive(false);
        objects.Enqueue(obj);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
public class CustomMonobehObjectPool<T> where T : MonoBehaviour
{
    private T prefab;
    private ObjectPool<T> pool;

    public CustomMonobehObjectPool(T prefab, int prewarmObjects)
    {
        this.prefab = prefab;
        pool = new ObjectPool<T>(OnCreate, OnGet, OnRelease, GameObject.Destroy, false, prewarmObjects);
        pool.Get();
    }
    public T Get()
    {
      return  pool.Get();
    }
    public void Release(T obj)
    {
        pool.Release(obj);
    }
    private T OnCreate()
    {
        return GameObject.Instantiate(prefab);
    }
    public void OnGet(T obj)
    {
        obj.gameObject.SetActive(true);
    }

    public void OnRelease(T obj)
    {
        obj.gameObject.SetActive(false);
    }

   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : SingletonManager<GameObjectPool>
{
    private Dictionary<string, Queue<GameObject>> poolDict = new();
    private Dictionary<string, GameObject> prefabDict = new();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPool(string _key, GameObject _prefab, int _initSize)
    {
        if (poolDict.ContainsKey(_key)) return;

        var queue = new Queue<GameObject>();
        poolDict[_key] = queue;
        prefabDict[_key] = _prefab;

        GameObject parent = new GameObject(_key + "Parent");
        parent.transform.SetParent(transform);

        for (int i = 0; i < _initSize; i++)
        {
            GameObject obj = Instantiate(_prefab);
            obj.transform.SetParent(parent.transform);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
    }

    public GameObject GetFromPool(string _key)
    {
//        Debug.Log(_key);
        if (!poolDict.ContainsKey(_key))
        {
            Debug.Log("Not Found!");
            return null;
        }
//        Debug.Log($"get object : {_key}");
        var queue = poolDict[_key];
        GameObject obj;
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else
        {
            obj = Instantiate(prefabDict[_key]);
        }
        obj.transform.SetParent(null);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(string _key, GameObject _obj)
    {
        if (!poolDict.ContainsKey(_key))
        {
            Debug.Log($"Not found : {_key}");
            Destroy(_obj);
            return;
        }
        // Debug.Log($"return object : {_key}");
        var parent = GameObject.Find(_key + "Parent");
        _obj.transform.SetParent(parent.transform);
        _obj.SetActive(false);
        poolDict[_key].Enqueue(_obj);
    }

    public void ClearPool(string _key)
    {
        if (!poolDict.ContainsKey(_key))
        {
            return;
        }
        foreach (var obj in poolDict[_key])
        {
            Destroy(obj);
        }

        poolDict.Remove(_key);
        prefabDict.Remove(_key);
    }

    public void ClearAllPools()
    {
        foreach (var pool in poolDict.Values)
        {
            foreach (var obj in pool)
            {
                Destroy(obj);
            }
        }

        poolDict.Clear();
        prefabDict.Clear();
    }
}

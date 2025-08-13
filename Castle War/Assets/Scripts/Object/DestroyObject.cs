using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public void HidePrefab() => Destroy(gameObject);

    private void ReturnToPool()
    {
        transform.SetParent(null);
        var name = gameObject.name.Replace("(Clone)", "");
        GameObjectPool.Get().ReturnToPool(name,gameObject);
    }
}

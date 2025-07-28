using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : SingletonManager<GameObjectPool>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : SingletonManager<SettingsManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}

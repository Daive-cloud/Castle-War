using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionSO : ScriptableObject
{
    public Sprite Icon;
    public string UnitName;
    public abstract void ExecuteAction();
}

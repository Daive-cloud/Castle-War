using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class UnitStatsEditor : EditorWindow
{
    [MenuItem("Tools/Unit Stats Editor")]
    public static void ShowWindow()
    {
        GetWindow<UnitStatsEditor>("Unit Stats Config Editor");
    }
}

#endif
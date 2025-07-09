using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HvoUtils
{
    public static Vector3 GetPlacementPosition() => Input.GetMouseButton(0) ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;

    public static bool IsPointerOverUIElement() => EventSystem.current.IsPointerOverGameObject();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map",menuName = "Action/MapAction")]
public class MapSO : ScriptableObject
{
    public Sprite MapImage;
    public string MapName;
    public string Description;
    public List<Vector2> PlayerPositions;
    public int PlayerCount => PlayerPositions.Count;
}

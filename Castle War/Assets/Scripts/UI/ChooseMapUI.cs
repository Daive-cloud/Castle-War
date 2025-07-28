using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Collections;

public class ChooseMapUI : MonoBehaviour
{
    [Header("MapSO List")]
    public List<MapSO> Maps;
    [Header("UI Element")]
    public Image MapPreview;
    public TextMeshProUGUI MapName;
    public TextMeshProUGUI Description;
    public RectTransform MapScrollParent;
    [Header("Map Scroll")]
    public GameObject MapScroll;
    [Header("Reference Part")]
    public LevelUI levelUI;
    public PlayerPositionUI positionUI;
    public MapSO currentSelectedMap { get; private set; }
    public void Start()
    {
        GenerateMapScroll();
        InitializeMapUI();
    }

    private void GenerateMapScroll()
    {
        foreach (Transform child in MapScrollParent)
        {
            Destroy(child);
        }

        for (int i = 0; i < Maps.Count; i++)
            {
                var newScroll = Instantiate(MapScroll, MapScrollParent);
                var map = Maps[i];
                StringBuilder sb = new(map.MapName);
                sb.Append("   ");
                if (map.PlayerCount > 2)
                {
                    sb.Append("(2-").Append(map.PlayerCount.ToString()).Append(")");
                }
                else
                {
                    sb.Append("(2)");
                }
                newScroll.GetComponentInChildren<TextMeshProUGUI>().text = sb.ToString();
                newScroll.GetComponent<Button>().onClick.AddListener(() => AssignMapInfo(map.MapImage, map.MapName, map.Description, map));
            }
    }

    private void AssignMapInfo(Sprite _image, string _name, string _description, MapSO _currentMap)
    {
        MapPreview.sprite = _image;
        MapName.text = _name;
        Description.text = _description;
        currentSelectedMap = _currentMap;

    }

    private void InitializeMapUI()
    {
        var map = Maps[0];
        AssignMapInfo(map.MapImage, map.MapName, map.Description, map);
        ApplyLevelUIUpdate();
       // ApplyPositionUIUpdate();
    }

    public void ConfirmChoice()
    {
        ApplyLevelUIUpdate();
        // ApplyPositionUIUpdate();
        levelUI.CloseMapChooseUI();

        levelUI.MapIndex = Maps.IndexOf(currentSelectedMap);
//        Debug.Log($"Map Select index : {levelUI.MapIndex}" );
    }

    public void ApplyPositionUIUpdate()
    {
        positionUI.PositionDrops.Clear();
        //Debug.Log($"Position Drop Count In MapUI: {levelUI.PositionDropParent.childCount}.");
        foreach (Transform child in levelUI.PositionDropParent)
        {
            positionUI.PositionDrops.Add(child.gameObject.GetComponent<TMP_Dropdown>());
        }

        positionUI.InitializeDropdown(currentSelectedMap.PlayerCount);
    }

    private void ApplyLevelUIUpdate()
    {
        levelUI.UpdateMapPreview(currentSelectedMap.MapImage, currentSelectedMap.MapName);
        levelUI.UpdateDropDownScroll(currentSelectedMap.PlayerCount);
    }
}

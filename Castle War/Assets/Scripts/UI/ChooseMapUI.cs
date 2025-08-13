using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Collections;

public class ChooseMapUI : MonoBehaviour , ISaveData
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
        var map = currentSelectedMap;
       
        AssignMapInfo(map.MapImage, map.MapName, map.Description, map);
        ApplyLevelUIUpdate();
    }

    public void ConfirmChoice()
    {
        ApplyLevelUIUpdate();
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

    private IEnumerator RefillGameConfigWithDelay(GameData _gameData)
    {
        yield return new WaitForSeconds(.05f);

        var playerParent = levelUI.PlayerDropParent;
        var posParent = levelUI.PositionDropParent;

        for (int i = 1; i < playerParent.childCount; i++)
        {
//            Debug.Log($"player drop valus : {_gameData.playerDropValues[i]}");
            playerParent.GetChild(i).GetComponent<TMP_Dropdown>().value = _gameData.playerDropValues[i];
        }

        for (int i = 0; i < posParent.childCount; i++)
        {
//            Debug.Log($"pos drop valus : {_gameData.positionDropValues[i]}");
            posParent.GetChild(i).GetComponent<TMP_Dropdown>().value = _gameData.positionDropValues[i];
        }
        gameObject.SetActive(false);
    }

    public void LoadData(GameData _gameData)
    {
        string mapID = _gameData.selectedMapID;
        //        Debug.Log($"mapID : {mapID}");
        if (mapID == "")
        {
            currentSelectedMap = Maps[0];
        }
        else
        {
            foreach (var map in Maps)
            {
                if (map.MapID == mapID)
                {
                    currentSelectedMap = map;
                    break;
                }
            }
            if (currentSelectedMap == null)
            {
                currentSelectedMap = Maps[0];
            }
        }
        levelUI.MapIndex = Maps.IndexOf(currentSelectedMap);
        InitializeMapUI();
        StartCoroutine(RefillGameConfigWithDelay(_gameData));
    }

    public void SaveData(ref GameData _gameData)
    {
        Debug.Log("Save map info");
        _gameData.selectedMapID = currentSelectedMap.MapID;
        _gameData.playerDropValues = new int[10];
        _gameData.positionDropValues = new int[10];

        var playerParent = levelUI.PlayerDropParent;
        var posParent = levelUI.PositionDropParent;

        for (int i = 1; i < playerParent.childCount; i++)
        {
            _gameData.playerDropValues[i] = playerParent.GetChild(i).GetComponent<TMP_Dropdown>().value;
        }

        for (int i = 0; i < posParent.childCount; i++)
        {
            _gameData.positionDropValues[i] = posParent.GetChild(i).GetComponent<TMP_Dropdown>().value;
        }
    }
}

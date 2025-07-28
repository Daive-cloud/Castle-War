using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
public enum EnemyType
{
    None,
    Easy,
    Medium,
    Hard,
    Player
}

public enum PlayerChoiceType
{
    Quit,
    Random,
    Specific
}

public class PlayerChoice
{
    public PlayerChoiceType Type;
    public int? Position;

    public PlayerChoice(PlayerChoiceType _type, int? _position = null)
    {
        Type = _type;
        Position = _position;
    }
}

public class SettingsManager : SingletonManager<SettingsManager>
{
    private List<SelectSourceUI> resourceUI ;
    private RectTransform playerDropParent;
    private RectTransform posDropParent;
    private ChooseMapUI chooseUI;
    private Image errorPanel;
    public List<EnemyType> enemyTypes = new();
    private Dictionary<int, int> finalAssignments = new();

    [Header("Player Prefab")]
    [SerializeField] private List<GameObject> blueArmy;
    [SerializeField] private List<GameObject> redArmy;
    public int woodAmount { get; private set; }
    public int meatAmount { get; private set; }
    public int goldAmount { get; private set; }
    public int unitAmount { get; private set; }
    private List<int> allPositions = new List<int>() { 1, 2, 3, 4 };
    private Dictionary<string, int> chineseToArabic = new Dictionary<string, int>() { { "一", 1 }, { "二", 2 }, { "三", 3 }, { "四", 4 } };
    private Dictionary<int, string> playerColors = new()
        {
            {0,"Blue"},
            {1,"Red"},
            {2,"Purple"},
            {3,"Yellow"}
        };
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        if (_scene.name == "选择关卡" && this != null)
        {
            StartCoroutine(RefillInfo());
            enemyTypes.Clear();
        }
    }

    public void RecordFinalResource()
    {
        woodAmount = int.Parse(resourceUI[0].ResourcesFont.text);
        meatAmount = int.Parse(resourceUI[1].ResourcesFont.text);
        goldAmount = int.Parse(resourceUI[2].ResourcesFont.text);
        unitAmount = int.Parse(resourceUI[3].ResourcesFont.text);
    }

    public void ConfigEnemyInfo(out bool canEnterBattle)
    {
        bool flag = false;
        string sb = "";
        enemyTypes.Clear();
        enemyTypes.Add(EnemyType.Player);
        for (int i = 1; i < playerDropParent.childCount; i++)
        {
            var dropIndex = playerDropParent.GetChild(i).GetComponent<TMP_Dropdown>().value;
            //            Debug.Log($"Choose Index : {dropIndex}");
            SwitchEnemyType(dropIndex);
            if (dropIndex != 0)
                flag = true;
            // Debug.Log($"enemy type : {enemyTypes[i]}");
        }
        canEnterBattle = flag;
        if (!flag)
        {
            sb = "您至少需要选择一名玩家才能进入对战!\n";
            ShowError(sb);
            errorPanel.GetComponentInChildren<TextMeshProUGUI>().text = sb;
            return;
        }

        Dictionary<int, PlayerChoice> playerChoices = new();
        List<int> vaildPositions = allPositions.Take(posDropParent.childCount).ToList();
        for (int i = 0; i < posDropParent.childCount; i++)
        {
            TMP_Dropdown dropdown = posDropParent.GetChild(i).GetComponent<TMP_Dropdown>();
            int index = dropdown.value;
            string optionText = dropdown.options[index].text;
            if (index == 0)
            {
                if (i == 0)
                {
                    sb = "您作为指挥官必须出战!请给自己分配一个位置!";
                    ShowError(sb);
                    canEnterBattle = false;
                    return;
                }
                playerChoices.Add(i, new PlayerChoice(PlayerChoiceType.Quit));
            }
            else if (index == 1)
            {
                playerChoices.Add(i, new PlayerChoice(PlayerChoiceType.Random));
            }
            else
            {
                string chinese = optionText.Substring(0, 1);
                int number = chineseToArabic[chinese];
                playerChoices.Add(i, new PlayerChoice(PlayerChoiceType.Specific, number));
            }
            Debug.Log(playerChoices[i].Type);
        }


        HashSet<int> usedPositions = new();
        List<int> randomPlayers = new();

        foreach (var kvp in playerChoices)
        {
            int playerId = kvp.Key;
            var choice = kvp.Value;

            if (choice.Type == PlayerChoiceType.Quit)
            {
                continue;
            }

            if (choice.Type == PlayerChoiceType.Specific && choice.Position.HasValue)
            {
                finalAssignments[playerId] = choice.Position.Value;
                usedPositions.Add(choice.Position.Value);
            }
            else if (choice.Type == PlayerChoiceType.Random)
            {
                randomPlayers.Add(playerId);
            }
        }

        List<int> availablePositions = new();
        foreach (var pos in vaildPositions)
        {
            if (!usedPositions.Contains(pos))
            {
                availablePositions.Add(pos);
            }
        }
        ShuffleList(availablePositions);
        for (int i = 0; i < randomPlayers.Count && i < availablePositions.Count; i++)
        {
            finalAssignments[randomPlayers[i]] = availablePositions[i];
        }


        // foreach (var kvp in finalAssignments)
        // {
        //     Debug.Log($"玩家 {kvp.Key} 分配到位置 {kvp.Value}");
        // }

        // // 可选：输出退出游戏的玩家
        // foreach (var kvp in playerChoices)
        // {
        //     if (kvp.Value.Type == PlayerChoiceType.Quit)
        //     {
        //         Debug.Log($"玩家 {kvp.Key} 退出游戏");
        //     }
        // }

        canEnterBattle = IsAssignmentVaild(playerChoices, finalAssignments);

    }

    private void SwitchEnemyType(int _index)
    {
        EnemyType newType;
        switch (_index)
        {
            case 0:
                newType = EnemyType.None;
                break;
            case 1:
                newType = EnemyType.Easy;
                break;
            case 2:
                newType = EnemyType.Medium;
                break;
            case 3:
                newType = EnemyType.Hard;
                break;
            default:
                return;
        }
        enemyTypes.Add(newType);
    }

    public void HideError() => errorPanel.gameObject.SetActive(false);

    public void ShowError(string _log)
    {
        errorPanel.gameObject.SetActive(true);
        errorPanel.GetComponentInChildren<TextMeshProUGUI>().text = _log;
    }

    private void ShuffleList<T>(List<T> _list)
    {
        for (int i = _list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_list[i], _list[j]) = (_list[j], _list[i]);
        }
    }

    private bool IsAssignmentVaild(Dictionary<int, PlayerChoice> _playerChoices, Dictionary<int, int> _finalAssignments)
    {
        string sb = "";

        for (int i = 0; i < _playerChoices.Count; i++)
        {
            var choice = _playerChoices[i];

            if (choice.Type != PlayerChoiceType.Quit && enemyTypes[i] == EnemyType.None)
            {
                sb = "您给无效的玩家分配了位置,这是不合理的!\n请重新操作!";
                ShowError(sb);
                return false;
            }
            else if (choice.Type == PlayerChoiceType.Quit && enemyTypes[i] != EnemyType.None)
            {
                sb = "您没有给参战的玩家分配位置!请重新操作!";
                ShowError(sb);
                return false;
            }
        }

        return true;
    }

    public void AssignOriginalPositions()
    {
        var mapPos = chooseUI.currentSelectedMap.PlayerPositions;
        int playerCount = mapPos.Count;
        var vaildColors = playerColors.Take(playerCount).ToDictionary(pair => pair.Key, pair => pair.Value); // 选择参与游戏的玩家

        Dictionary<int, Vector2> positionCroods = new();

        for (int i = 0; i < mapPos.Count; i++)
        {
            positionCroods.Add(i + 1, mapPos[i]);
        }
        foreach (var kvp in finalAssignments)
        {
            int playerId = kvp.Key;
            int positionId = kvp.Value;

            if (!positionCroods.ContainsKey(positionId)) continue;

            Vector2 spawnPos = positionCroods[positionId];
            string color = vaildColors[playerId];
            //Debug.Log($"玩家{playerId}（颜色：{color}）出生在位置 {positionId} 坐标：{spawnPos}");
            SpawnUnitsWithColor(color, spawnPos);
        }
    }

    private void SpawnUnitsWithColor(string _color, Vector2 _position)
    {
        if (_color == "Blue")
        {
            var castle = blueArmy[0];
            Instantiate(castle, _position, Quaternion.identity);
            var vaildArmy = blueArmy.Take(unitAmount + 1).ToList();

            for (int i = 1; i < vaildArmy.Count; i++)
            {
                float angleStep = 360f / (vaildArmy.Count - 1);
                SpawnUnitsAroundCastle(_position, vaildArmy[i], angleStep, i - 1);
            }
            Camera.main.transform.position = new Vector3(_position.x, _position.y, -10f);
        }
        else if (_color == "Red")
        {
            var castle = redArmy[0];
            Instantiate(castle, _position, Quaternion.identity);

            var vaildArmy = redArmy.Take(unitAmount + 1).ToList();

            for (int i = 1; i < vaildArmy.Count; i++)
            {
                float angleStep = 360f / (vaildArmy.Count - 1);
                SpawnUnitsAroundCastle(_position, vaildArmy[i], angleStep, i - 1);
            }
        }
    }

    private void SpawnUnitsAroundCastle(Vector2 center, GameObject itemPrefab, float angleStep, int index, float radius = 4f)
    {
        float angle = index * angleStep * Mathf.Rad2Deg;
        float xPos = center.x + radius * Mathf.Cos(angle);
        float yPos = center.y + radius * Mathf.Sin(angle);
        var spawnPos = new Vector2(xPos, yPos);

        Instantiate(itemPrefab, spawnPos, Quaternion.identity);
    }

    private IEnumerator RefillInfo()
    {
        yield return null;
        resourceUI = new List<SelectSourceUI>();
        var resourcesParent = GameObject.Find("Resources").GetComponent<RectTransform>();
        for (int i = 0; i < resourcesParent.childCount; i++)
        {
            resourceUI.Add(resourcesParent.GetChild(i).GetComponent<SelectSourceUI>());
        }
        playerDropParent = GameObject.Find("PlayerDropParent").GetComponent<RectTransform>();
        posDropParent = GameObject.Find("PositionDropParent").GetComponent<RectTransform>();

        chooseUI = GameObject.Find("ChooseMapUI").GetComponent<ChooseMapUI>();
        chooseUI.gameObject.SetActive(false);
        errorPanel = GameObject.Find("Error").GetComponent<Image>();
        if (errorPanel != null)
        {
            errorPanel.GetComponentInChildren<Button>().onClick.AddListener(() => HideError());
            errorPanel.gameObject.SetActive(false);
        }

    }
}

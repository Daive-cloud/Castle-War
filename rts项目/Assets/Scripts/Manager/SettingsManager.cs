using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

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
    [SerializeField] private List<SelectSourceUI> resourceUI;
    [SerializeField] private RectTransform playerDropParent;
    [SerializeField] private RectTransform posDropParent;
    [SerializeField] private ChooseMapUI chooseUI;
    [SerializeField] private Image ErrorPanel;
    public List<EnemyType> enemyTypes = new();
    private Dictionary<int, int> playerChoices = new();
    public int woodAmount { get; private set; }
    public int meatAmount { get; private set; }
    public int goldAmount { get; private set; }
    public int unitAmount { get; private set; }
    private List<int> allPositions = new List<int>() { 1, 2, 3, 4 };
    private Dictionary<string, int> chineseToArabic = new Dictionary<string, int>() { { "一", 1 }, { "二", 2 }, { "三", 3 }, { "四", 4 } };
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
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
            ErrorPanel.GetComponentInChildren<TextMeshProUGUI>().text = sb;
            return;
        }

        var mapPos = chooseUI.currentSelectedMap.PlayerPositions;
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

        Dictionary<int, int> finalAssignments = new();
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

    public void HideError() => ErrorPanel.gameObject.SetActive(false);

    public void ShowError(string _log)
    {
        ErrorPanel.gameObject.SetActive(true);
        ErrorPanel.GetComponentInChildren<TextMeshProUGUI>().text = _log;
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
    
}

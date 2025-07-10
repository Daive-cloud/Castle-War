using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPositionUI : MonoBehaviour
{
    public List<TMP_Dropdown> PositionDrops;
    private  List<string> DefaultOptions = new List<string>() {"无","随机" };
    private List<string> ExtendedOptions = new List<string>() { "一号位", "二号位", "三号位", "四号位" };
    private List<string> AllOptions = new();
    private Dictionary<TMP_Dropdown, string> SelectedPositions = new();

    public void InitializeDropdown(int _playerCount)
    {
        SelectedPositions = new();
        var newDropList = ExtendedOptions.Take(_playerCount).ToList();
        AllOptions = new(DefaultOptions.Concat(newDropList).ToList());
        foreach (var drop in PositionDrops)
        {
            drop.ClearOptions();
            drop.AddOptions(DefaultOptions);
            drop.AddOptions(newDropList);

            drop.value = 0;
            drop.onValueChanged.AddListener((index) => OnPositionSelected(drop, index));
        }
    }

    private void OnPositionSelected(TMP_Dropdown _changedDropdown, int _selectedIndex)
    {
        string selected = _changedDropdown.options[_selectedIndex].text;  // 获取选择的文本值
        SelectedPositions[_changedDropdown] = selected; // 在字典中作一个对应

        foreach (var drop in PositionDrops)
        {
            // 如果有，获取当前dropdown已经选择的值
            string currentSelected = SelectedPositions.ContainsKey(drop) ? SelectedPositions[drop] : "";

            drop.ClearOptions(); // 先暂时清除所有旧的选项，排除已经选择的选项
            List<string> available = new List<string>(AllOptions);
            foreach (var pair in SelectedPositions)
            {
                if (pair.Key != drop && pair.Value != "随机" && pair.Value != "无")
                {
                    available.Remove(pair.Value); // 过滤空字符串不会造成影响
                }
            }
            drop.AddOptions(available);
            drop.onValueChanged.RemoveAllListeners();

            if (available.Contains(currentSelected))
            {
                drop.value = available.IndexOf(currentSelected);
            }
            else
            {
                drop.value = 0;
            }

            drop.onValueChanged.AddListener((index) => OnPositionSelected(drop, index));

        }
    }
}

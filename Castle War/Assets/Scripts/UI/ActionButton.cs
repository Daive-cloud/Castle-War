using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    private Button button => GetComponent<Button>();
    private TextMeshProUGUI unitName => GetComponentInChildren<TextMeshProUGUI>();

    [SerializeField] private Image buttonIcon;

    public void InitializeButton(Sprite _icon,string _name,UnityAction _action)
    {
        button.onClick.RemoveAllListeners();

        buttonIcon.sprite = _icon;
        unitName.text = _name;
        button.onClick.AddListener(_action);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}

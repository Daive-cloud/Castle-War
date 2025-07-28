using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    private Image image => GetComponent<Image>();
    [SerializeField] private GameObject ActionButtonPrefab;

    private List<ActionButton> ActionButtons = new List<ActionButton>();

    private void Start()
    {
        HideActionBar();
    }

    public void RegisterActionButton(Sprite _icon,UnityAction _action)
    {
        GameObject newButton = Instantiate(ActionButtonPrefab,transform);
        newButton.GetComponent<ActionButton>().InitializeButton(_icon,_action);
        ActionButtons.Add(newButton.GetComponent<ActionButton>());
    }

    public void ClearAllActionButtons()
    {
        if(ActionButtons.Count > 0)
        {
            for(int i=ActionButtons.Count-1; i>=0; i--)
            {
                Destroy(ActionButtons[i].gameObject);
                ActionButtons.RemoveAt(i);
            }
        }
    }

    public void ShowActionBar()
    {
        gameObject.SetActive(true);
    }

    public void HideActionBar()
    {
        gameObject.SetActive(false);
    }
}

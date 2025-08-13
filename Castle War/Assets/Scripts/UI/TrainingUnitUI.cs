using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrainingUnitUI : MonoBehaviour
{
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private Button CancleButton;
     [SerializeField] private RectTransform telemplateParent;
    [SerializeField] private GameObject costTelemplate;
    [SerializeField] private Sprite meatIcon;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite crystalIcon;
    private void Start()
    {
        HideRectangle();
    }

    public void ShowRectangle(int _goldCost, int _meatCost,int _crystalCost)
    {
       gameObject.SetActive(true);
        var manager = GameManager.Get();
        for (int i = 0; i < telemplateParent.childCount; i++)
        {
            Destroy(telemplateParent.GetChild(i).gameObject);
        }

        if (_goldCost > 0)
        {
            var newTelemplate = Instantiate(costTelemplate, telemplateParent);
            newTelemplate.GetComponentInChildren<Image>().sprite = goldIcon;
            var content = newTelemplate.GetComponentInChildren<TextMeshProUGUI>();
            content.text = _goldCost.ToString("N0");
            content.color = manager.GoldAmount >= _goldCost ? Color.black : Color.red;
        }

        if (_meatCost > 0)
        {
            var newTelemplate = Instantiate(costTelemplate, telemplateParent);
            newTelemplate.GetComponentInChildren<Image>().sprite = meatIcon;
            var content = newTelemplate.GetComponentInChildren<TextMeshProUGUI>();
            content.text = _meatCost.ToString("N0");
            content.color = manager.MeatAmount >= _meatCost ? Color.black : Color.red;
        }
        
        if (_crystalCost > 0)
        {
            var newTelemplate = Instantiate(costTelemplate, telemplateParent);
            newTelemplate.GetComponentInChildren<Image>().sprite = crystalIcon;
            var content = newTelemplate.GetComponentInChildren<TextMeshProUGUI>();
            content.text = _crystalCost.ToString("N0");
            content.color = manager.CrystalAmount >= _crystalCost ? Color.black : Color.red;
        }
    }

    public void HideRectangle()
    {
        gameObject.SetActive(false);
    }

    public void RegisterHooks(UnityAction _confirmAction,UnityAction _cancleAction)
    {
        ConfirmButton.onClick.RemoveAllListeners();
        CancleButton.onClick.RemoveAllListeners();

        ConfirmButton.onClick.AddListener(_confirmAction);
        CancleButton.onClick.AddListener(_cancleAction);   
    }

    private void OnDisable()
    {
        ConfirmButton.onClick.RemoveAllListeners();
        CancleButton.onClick.RemoveAllListeners();
    }
}

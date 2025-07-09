using UnityEngine;
using DG.Tweening;
public class ButtonAnimation : MonoBehaviour
{
    public RectTransform OnScreenPosition;
    public RectTransform OffScreenPosition;
    private RectTransform UiTransform;

    void Start()
    {
        UiTransform = GetComponent<RectTransform>();
        UiTransform.position = OffScreenPosition.position;
    }

    public void MoveInScreen()
    {
        UiTransform.DOAnchorPos(OnScreenPosition.anchoredPosition,.2f).SetEase(Ease.Linear);
    }

    public void MoveOffScreen()
    {
        UiTransform.DOAnchorPos(OffScreenPosition.anchoredPosition,.1f).SetEase(Ease.Linear);
    }
}

using TMPro;
using UnityEngine;
using DG.Tweening;
public class DamageFontUI : MonoBehaviour
{
    private TextMeshPro DamageFont => GetComponent<TextMeshPro>();
    private static int SortingOrder = 0;
    public void SetFontValue(int _value, int _fontSize, Vector2 _startPos, Color _color, float _xOffset)
    {
        DamageFont.text = _value.ToString();
        DamageFont.fontSize = _fontSize;
        DamageFont.color = _color;

        DamageFont.sortingOrder = SortingOrder;
        SortingOrder++;

        if (SortingOrder > 1000)
        {
            SortingOrder = 0;
        }

        float xOffset = Random.Range(-_xOffset, _xOffset);
        float yOffset = Random.Range(1f, 2f);
        Vector2 targetPos = _startPos + new Vector2(xOffset, yOffset);
        transform.position = targetPos;
        PopupFont();
    }

    private void PopupFont()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(Random.Range(1.2f,1.4f), 1f).From(Vector2.zero)).SetEase(Ease.OutBack);

        Vector2 endPos = transform.position + Vector3.up;

        seq.Append(transform.DOMove(endPos, 1f).SetEase(Ease.InQuad));
        seq.Join(DamageFont.DOFade(0, 2f));
        seq.OnComplete(() => Destroy(gameObject));
    }

}

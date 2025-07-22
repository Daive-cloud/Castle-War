using UnityEngine;
using DG.Tweening;
using System.Runtime.InteropServices;
using System.Linq;
using Unity.VisualScripting;

public class GrenadeController : MonoBehaviour
{
    private Animator anim => GetComponent<Animator>();

    [SerializeField] private float arcHeight = 2.5f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float blastRadius = 1f;
    private Tween rotateTween;
    private Unit owner;

    public void ThrowAnimation(Vector3 _startPos, Vector3 _endPos, Unit _owner)
    {
        transform.position = _startPos;
        owner = _owner;

        rotateTween = transform.DORotate(new Vector3(0, 0, 360f), 0.15f, RotateMode.FastBeyond360)
                                .SetLoops(-1, LoopType.Restart)
                                .SetEase(Ease.Linear);

        Vector3 midPoint = (_startPos + _endPos) * .5f;
        midPoint.y += arcHeight;

        Vector3[] path = new Vector3[] { _startPos, midPoint, _endPos };

        transform.DOPath(path, duration, PathType.CatmullRom)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => OnHitTarget());


    }
    private void OnHitTarget()
    {
        rotateTween.Kill();
        AudioManager.Get().PlaySFX(8);
        anim.SetTrigger("Boom");
        DoDamage();
        Destroy(gameObject, 2f);
    }

    private void DoDamage()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position,blastRadius);
        var vaildUnits = colliders.ToList().Where(unit => unit != null && unit.TryGetComponent(out Unit _) && unit.tag != owner.tag);
        foreach (var unit in vaildUnits)
        {
            if (unit.TryGetComponent(out BarrelUnit barrel))
            {
                owner.GetComponent<UnitStats>().TakeDamage(unit.GetComponent<UnitStats>(), owner.GetComponent<UnitStats>().Damage.GetValue() * 4);
            }
            else
            {
                owner.GetComponent<UnitStats>().TakeDamage(unit.GetComponent<UnitStats>());
            }
        }
    }
    

}

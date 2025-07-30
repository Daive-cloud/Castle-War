using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using DG.Tweening;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField] private float FlySpeed;

    private Unit Owner;
    private Unit Target;

    public void RegisterArrow(Unit _owner, Unit _target)
    {
        Owner = _owner;
        Target = _target;

        float distance = Vector2.Distance(_owner.transform.position, _target.transform.position);
        float flyTime = distance / FlySpeed;

        transform.DOMove(_target.transform.position, flyTime).SetEase(Ease.Linear)
                        .OnComplete(() => OnArrivedDestination(_target));
    }

    private void OnArrivedDestination(Unit _target)
    {
        bool isTouchedTarget = Vector2.Distance(transform.position, _target.transform.position) < 1f;
        if (_target != null && !_target.IsDead && isTouchedTarget)
        {
            Owner.stats.TakeDamage(Target.GetComponent<UnitStats>());
            AudioManager.Get().PlaySFX(10);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(DestroyWithDelay());
        }
    }

    private IEnumerator DestroyWithDelay()
    {
//        Debug.Log($"target transform : {_target.transform.position} , arrow transform : {transform.position}");
        var direction = (transform.position - Owner.transform.position).normalized;
        Debug.Log($"direction : {direction}");
        float timer = 0f;
        while (timer < 2f)
        {
            transform.position += Time.deltaTime * FlySpeed * direction;
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

}

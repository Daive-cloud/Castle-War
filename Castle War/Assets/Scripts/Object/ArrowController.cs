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

        transform.DOMove(_target.transform.position, flyTime).SetEase(Ease.Linear).OnComplete(() => OnArrivedDestination(_target));
    }

    private void OnArrivedDestination(Unit _target)
    {
        if (_target != null)
        {
            Owner.stats.TakeDamage(Target.GetComponent<UnitStats>());
            AudioManager.Get().PlaySFX(10);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(DestroyWithDelay(_target.transform));
        }
    }

    private IEnumerator DestroyWithDelay(Transform _target)
    {
        var direction = (_target.position - transform.position).normalized;

        float timer = 0f;
        while (timer < 2f)
        {
            transform.position += Time.time * FlySpeed * direction;
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

}

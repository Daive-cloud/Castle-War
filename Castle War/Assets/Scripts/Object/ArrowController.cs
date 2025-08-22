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
            if (gameObject.activeSelf)
            {
                StartCoroutine(ReturnToPool());
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(GCWithDelay());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator GCWithDelay()
    {
        //        Debug.Log($"target transform : {_target.transform.position} , arrow transform : {transform.position}");
        var direction = (transform.position - Owner.transform.position).normalized;
        //        Debug.Log($"direction : {direction}");
        float timer = 0f;
        while (timer < 2f)
        {
            transform.position += Time.deltaTime * FlySpeed * direction;
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(ReturnToPool());
    }

    private IEnumerator ReturnToPool()
    {
        var trail = GetComponent<TrailRenderer>();
        var sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;

        yield return new WaitForSeconds(trail.time);
        sr.enabled = true;
        var arrow = gameObject.name.Replace("(Clone)","");
        // Debug.Log("Return To Pool");
        GameObjectPool.Get().ReturnToPool(arrow,gameObject);
    }

}

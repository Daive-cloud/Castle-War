using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BigArrowController : MonoBehaviour
{
    [SerializeField] private float flySpeed = 25f;
    [SerializeField] private Sprite brokenArrow;
    private Unit Owner;
    private Unit Target;
    private Vector3 lastPos;
    private SpriteRenderer sr => GetComponent<SpriteRenderer>();
    private TrailRenderer trail => GetComponent<TrailRenderer>();
    private Sprite originSR;
    public void RegisterArrow(Unit _owner, Unit _target)
    {
        Owner = _owner;
        Target = _target;
        originSR = sr.sprite;
        float flyTime = Vector2.Distance(_owner.transform.position, _target.transform.position) / flySpeed;

        var startPos = transform.position;
        var endPos = _target.transform.position;
        var midPoint = (startPos + endPos) / 2f;
        midPoint.y += 2f;
        lastPos = startPos;

        Vector3[] path = new Vector3[] { startPos, midPoint, endPos };
        transform.DOPath(path, flyTime, PathType.CatmullRom).SetEase(Ease.Linear)
                        .OnUpdate(() =>
                            {
                                Vector3 dir = transform.position - lastPos;
                                if (dir.sqrMagnitude > 0.001f)
                                {
                                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                                    transform.eulerAngles = new Vector3(0, 0, angle);
                                }
                                lastPos = transform.position;
                            }
                        ).OnComplete(() => OnHitTarget());
    }

    public void OnHitTarget()
    {
        bool canTouchTarget = Vector2.Distance(transform.position, Target.transform.position) < 1f;
        
        if (canTouchTarget)
        {
            AudioManager.Get().PlaySFX(55);
            Owner.stats.TakeDamage(Target.GetComponent<UnitStats>());
            sr.enabled = false;
            StartCoroutine(ReturnToPool(1));
        }
        else
        {
            sr.sprite = brokenArrow;
            StartCoroutine(ReturnToPool(10));
        }
       
    }

    private IEnumerator ReturnToPool(float _param)
    {
        yield return new WaitForSeconds(trail.time * _param);
        sr.enabled = true;
        sr.sprite = originSR;
        var arrow = gameObject.name.Replace("(Clone)", "");
        GameObjectPool.Get().ReturnToPool(arrow,gameObject);
    }
}

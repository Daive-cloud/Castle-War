using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GoblinUnit : HumanoidUnit
{
    protected override void Start()
    {
        base.Start();
        onTakedDamage += HitBackTarget;
    }
    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer >= CheckFrequency)
        {
            FindClosestEnemyInRange();
            CheckTimer = Time.time;
            if (HasRegisteredTarget)
            {
                if (CanAttackTarget())
                {
                    ai.ClearPath();
                    if (Time.time - AttackTimer >= AttackFrequency)
                    {
                        anim.SetTrigger("Attack");
                        AttackTimer = Time.time;
                    }
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }
        }
    }

    public void GoblinAttackTrigger()
    {
        if (Target != null && !Target.IsDead)
        {
            if (Target.TryGetComponent(out HumanoidUnit unit))
            {
                stats.TakeDamage(unit.stats, stats.Damage.GetValue() * 2);
            }
            else
            {
                stats.TakeDamage(Target.stats);
            }
        }
    }

    private void HitBackTarget()
    {
        var direction = IsFacingRight ? 1 : -1;
        if (Target != null && Target.TryGetComponent(out HumanoidUnit _))
        {
            Target.GetComponentInChildren<Animator>().speed = 0;
            var startPos = Target.transform.position;
            var endPos = startPos + new Vector3(4, 0, 0) * direction;
            var midPos = (startPos + endPos) / 2 + new Vector3(0, 2, 0);

            Vector3[] path = new Vector3[] { startPos, midPos, endPos };

            Target.transform.DOPath(path,.4f,PathType.CatmullRom).SetEase(Ease.Linear).OnComplete(() => { Target.GetComponentInChildren<Animator>().speed = 1; });
        }
    }
}

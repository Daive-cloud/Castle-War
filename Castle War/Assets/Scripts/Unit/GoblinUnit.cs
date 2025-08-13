using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GoblinUnit : HumanoidUnit
{
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
                if (Random.Range(0, 100) <= 20)
                {
                    unit.Death();
                }
            }
            AudioManager.Get().PlaySFX(42);
            stats.TakeDamage(Target.stats);
        }
    }

    public override void PlaySelectedSound()
    {
         AudioManager.Get().PlaySFX(43);
    }

    public override void PlayDeathSound()
    {
         AudioManager.Get().PlaySFX(44);
    }

}

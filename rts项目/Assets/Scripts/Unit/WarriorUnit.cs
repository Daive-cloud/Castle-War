using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorUnit : HumanoidUnit
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
                        AttackTimer = Time.time;
                        ComboCounter %= 2;
                        AttackEnemyInRange();
                    }
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }
        }
    }

    private void AttackEnemyInRange()
    {
        if (Mathf.Abs(Target.transform.position.y - transform.position.y) > 1f)
        {
            if (transform.position.y - Target.transform.position.y > 1f)
            {
                anim.SetBool("Attack_Down", true);
            }
            else if (transform.position.y - Target.transform.position.y < -1f)
            {
                anim.SetBool("Attack_Up", true);
            }
        }
        else
        {
            anim.SetBool("Attack", true);
        }
        anim.SetInteger("comboCounter",ComboCounter);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArcherUnit : HumanoidUnit
{
    [Header("Arrow Prefab")]
    [SerializeField] private GameObject ArrowPrefab;
    protected override void UpdateBehaviour()
    {
        if(Time.time - CheckTimer >= CheckFrequency)
        {
            FindClosestEnemyInRange();
            CheckTimer = Time.time;
            if(HasRegisteredTarget)
            {
                if(CanAttackTarget())
                {
                    if (ai != null)
                    {
                        ai.ClearPath();
                    }
                    if (Time.time - AttackTimer >= AttackFrequency)
                    {
                        AttackTimer = Time.time;
                        JudgeAttackAngle();
                    }
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }
            
        }
    }

    public void JudgeAttackAngle()
    {
        FlipController(Target.transform.position);
        Vector2 direction = Target.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        angle = ConvertAngle(angle);

        JudgeAngleToAttack(angle);
    }

    private void JudgeAngleToAttack(float angle)
    {
        if (angle < 90 && angle > 75)
        {
            anim.SetBool("Attack_Top", true);
        }
        else if (angle < 75 && angle > 15)
        {
            anim.SetBool("Attack_Up", true);
        }
        else if (angle < 15 && angle > -15)
        {
            anim.SetBool("Attack", true);
        }
        else if (angle < -15 && angle > -75)
        {
            anim.SetBool("Attack_Down", true);
        }
        else if (angle < -75 && angle > -90)
        {
            anim.SetBool("Attack_Buttom", true);
        }
    }

    private static float ConvertAngle(float angle)
    {
        if (angle > 90)
        {
            angle = 180 - angle;
        }
        else if (angle < -90)
        {
            angle = -180 - angle;
        }

        return angle;
    }

    public void LaunchArrow()
    {
        if (HasRegisteredTarget)
        {
            Vector2 direction = Target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject newArrow = Instantiate(ArrowPrefab, transform.position, rotation);
            newArrow.GetComponent<ArrowController>().RegisterArrow(this, Target);

        }
    }
}

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
                    ai.ClearPath();
                    if(Time.time - AttackTimer >= AttackFrequency)
                    {
                        AttackTimer = Time.time;
                        anim.SetBool("Attack",true);
                    }
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }
            
        }
    }

    public void LaunchArrow()
    {
        if(HasRegisteredTarget)
        {
            FlipController(Target.transform.position);
            Vector2 direction = Target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0,0,angle);

            GameObject newArrow = Instantiate(ArrowPrefab,transform.position,rotation);
            newArrow.GetComponent<ArrowController>().RegisterArrow(this,Target);

        }
    }
}

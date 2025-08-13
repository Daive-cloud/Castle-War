using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PowerArcherUnit : HumanoidUnit
{
    [Header("Arrow Info")]
    [SerializeField] private int maxArrowAmount;

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
                    if (ai != null)
                    {
                        ai.ClearPath();
                    }
                    if (Time.time - AttackTimer >= AttackFrequency)
                    {
                        AttackTimer = Time.time;
                        anim.SetBool("Attack", true);
                    }
                }
                else
                {
                    if (Target != null && !TryGetComponent(out TowerUnit _))
                        MoveToDestination(Target.transform.position);
                }
            }

        }
    }

    public void LaunchArrow()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ObjectCheckRadius);
        var enemies = colliders.
                    Where(unit => unit != null && unit.TryGetComponent(out Unit target) && !target.IsDead && unit.tag != this.tag && unit.tag != "Tree" && unit.tag != "Sheep")
                            .Select(unit => unit.GetComponent<Unit>()).ToList();
        if (enemies.Count == 0)
            return;
        AudioManager.Get().PlaySFX(45);
        int count = Random.Range(1, Mathf.Min(maxArrowAmount, enemies.Count + 1));

        for (int i = 0; i < count; i++)
        {
            if (enemies.Count <= 0)
            {
                break;
            }
            var newArrow = GameObjectPool.Get().GetFromPool(m_GameManager.bigArrow.name);
            newArrow.transform.position = transform.position;
            newArrow.transform.rotation = Quaternion.identity;
            
            int randomIndex = Random.Range(0, enemies.Count);
            var enemy = enemies[randomIndex];

            newArrow.GetComponent<BigArrowController>().RegisterArrow(this, enemy);
            enemies.RemoveAt(randomIndex);
        }
    }
    
    public override void PlaySelectedSound()
    {
        AudioManager.Get().PlaySFX(47);
    }

    public override void PlayDeathSound()
    {
        AudioManager.Get().PlaySFX(24);
    }
}

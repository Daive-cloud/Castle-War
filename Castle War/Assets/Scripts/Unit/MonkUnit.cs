using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonkUnit : HumanoidUnit
{
    private List<Unit> targets = new();
    private bool isTakeHealing = false;
    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer >= CheckFrequency)
        {
            FindFriendlyForce();
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
                        isTakeHealing = true;
                        anim.SetBool("Attack", true);
                    }
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }

        }
    }

    public void HealInjuredUnits()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ObjectCheckRadius);
        targets = colliders.Where(unit => unit != null && unit.TryGetComponent(out Unit unit1) && !unit1.IsDead && unit1.stats.isInjured && unit.tag == tag)
                                .Select(unit => unit.GetComponent<Unit>()).ToList();
        if (targets.Count == 0)
        {
            return;
        }
        else if (targets.Count == 1)
        {
            var unit = targets[0];
            stats.TakeHealing(unit.stats, .5f);
            GenerateHealEffect(unit.transform);
        }
        else if (targets.Count == 2)
        {
            foreach (var unit in targets)
            {
                stats.TakeHealing(unit.stats, .4f);
                GenerateHealEffect(unit.transform);
            }
        }
        else if (targets.Count >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, targets.Count);
                var unit = targets[randomIndex];

                stats.TakeHealing(unit.stats, .3f);
                GenerateHealEffect(unit.transform);
                targets.RemoveAt(randomIndex);
            }
        }

        AudioManager.Get().PlaySFX(48);
    }

    public override void AnimationFinishTrigger_3()
    {
        base.AnimationFinishTrigger_3();
        isTakeHealing = false;
    }

    public override void MoveToDestination(Vector2 _position)
    {
        if (isTakeHealing)
            return;
        base.MoveToDestination(_position);
    }

    private void GenerateHealEffect(Transform _target)
    {
        var newEffect = GameObjectPool.Get().GetFromPool(m_GameManager.healEffect.name);
        newEffect.transform.position = _target.position + new Vector3(0, -.25f, 0);
        newEffect.transform.rotation = Quaternion.identity;

        newEffect.transform.SetParent(_target);

        newEffect.GetComponent<Animator>().SetTrigger("Show");
    }

    public override void PlaySelectedSound()
    {
        AudioManager.Get().PlaySFX(49);
    }

    public override void PlayDeathSound()
    {
        AudioManager.Get().PlaySFX(50);
    }
}

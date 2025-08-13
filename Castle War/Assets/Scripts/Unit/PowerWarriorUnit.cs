using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerWarriorUnit : HumanoidUnit
{
    protected override void Start()
    {
        base.Start();
        onArrivedDestination += KeepDefenseState;
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
                        AttackTimer = Time.time;
                        ComboCounter %= 2;
                        AttackEnemy();
                    }
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }
        }
    }

    private void AttackEnemy()
    {
        anim.SetBool("Attack", true);
        anim.SetInteger("comboCounter", ComboCounter);

        ComboCounter++;
    }
    public override void SelectedUnit()
    {
        base.SelectedUnit();
        anim.SetBool("Defense", true);

        stats.DamageReduction.AddModifier(70);
    }

    public override void UnselectedUnit()
    {
        base.UnselectedUnit();
        anim.SetBool("Defense", false);

        stats.DamageReduction.RemoveModifier(70);
    }

    public override void MoveToDestination(Vector2 _position)
    {
        base.MoveToDestination(_position);
        anim.SetBool("Defense", false);
    }

    public override void FindClosestEnemyInRange()
    {
        if (isSelected)
            return;

        base.FindClosestEnemyInRange();
    }

    private void KeepDefenseState() => anim.SetBool("Defense", isSelected);
    public override void PlayAttackSound()
    {
        AudioManager.Get().PlaySFX(0);
    }

    public override void PlaySelectedSound()
    {
        AudioManager.Get().PlaySFX(46);
    }

    public override void PlayDeathSound()
    {
        AudioManager.Get().PlaySFX(23);
    }
    public override void AssignTarget(Unit _unit)
    {
        base.AssignTarget(_unit);
        stats.DamageReduction.RemoveModifier(70);
    }
}

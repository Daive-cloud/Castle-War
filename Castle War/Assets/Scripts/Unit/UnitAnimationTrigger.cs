using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationTrigger : MonoBehaviour
{
    private HumanoidUnit unit => GetComponentInParent<HumanoidUnit>();

    private void AnimationFinishTrigger() => unit.AnimationFinishTrigger();

    private void AnimationFinishTrigger_2() => unit.AnimationFinishTrigger_2();

    private void AnimationFinishTrigger_3() => unit.AnimationFinishTrigger_3();

    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, unit.AttackCheckRadius);

        if (colliders.Length > 0)
        {
            unit.PlayAttackSound();
        }
        if(!unit.IsDead && unit.Target != null)
            unit.FlipController(unit.Target.transform.position);

        foreach (var hit in colliders)
        {
            if (hit.TryGetComponent(out Unit enemy) && enemy.tag != unit.tag && !enemy.TryGetComponent(out TreeUnit _))
            {
                unit.stats.TakeDamage(enemy.GetComponent<UnitStats>());
            }
        }
    }

    private void LaunchArrow() => (unit as ArcherUnit).LaunchArrow();

    private void DestroyUnit() => unit.DestroyUnit();

    private void DestroyUnitWithDelay() => Destroy(unit.gameObject,5f);

    private void UsingAxe() => (unit as WorkerUnit).UsingAxe();

    private void ThrowGrenade() => (unit as DemolisherUnit).ThrowFirecraker();

    private void TakeBomb() => (unit as BarrelUnit).TakeBomb();

    private void PlayBuildingSound() => (unit as WorkerUnit).PlayBuildingSound();

}

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
        var enemy = unit.Target;
        if (enemy != null && !enemy.IsDead)
        {
            unit.PlayAttackSound();
            unit.FlipController(enemy.transform.position);
            unit.stats.TakeDamage(enemy.GetComponent<UnitStats>());
        }
    }

    private void LancerAttackTrigger() => (unit as LancerUnit).LancerAttackTrigger();
    private void GoblinAttackTrigger() => (unit as GoblinUnit).GoblinAttackTrigger();
    private void LaunchArrow() => (unit as ArcherUnit).LaunchArrow();

    private void DestroyUnit() => unit.DestroyUnit();

    private void DestroyUnitWithDelay() => Destroy(unit.gameObject,5f);

    private void UsingAxe() => (unit as WorkerUnit).UsingAxe();

    private void ThrowGrenade() => (unit as DemolisherUnit).ThrowFirecraker();

    private void TakeBomb() => (unit as BarrelUnit).TakeBomb();

    private void PlayBuildingSound() => (unit as WorkerUnit).PlayBuildingSound();

}

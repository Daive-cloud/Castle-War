using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemolisherUnit : HumanoidUnit
{
    [Header("Firecraker")]
    [SerializeField] private GameObject FirecrakerPrefab;

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
                        FlipController(Target.transform.position);
                        AttackTimer = Time.time;
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

    public void ThrowFirecraker()
    {
        AudioManager.Get().PlaySFX(12);
        var grenade = Instantiate(FirecrakerPrefab);
        grenade.GetComponent<GrenadeController>().ThrowAnimation(transform.position, Target.transform.position, this);
    }

    public override void PlayDeathSound()
    {
        AudioManager.Get().PlaySFX(22);
    }

    public override void PlaySelectedSound()
    {
        AudioManager.Get().PlaySFX(14);
    }
}

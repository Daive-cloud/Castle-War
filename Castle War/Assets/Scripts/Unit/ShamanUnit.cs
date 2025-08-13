using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShamanUnit : HumanoidUnit
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

    public void GenerateMagicExplosion()
    {
        if (Target != null)
        {
            PlayAttackSound();
            var newMagic = GameObjectPool.Get().GetFromPool(m_GameManager.magicExplosion.name);
            newMagic.transform.position = Target.transform.position;
            newMagic.transform.rotation = Quaternion.identity;

            newMagic.GetComponent<Animator>().SetTrigger("Boom");
            newMagic.GetComponent<MagicController>().RegisterMagic(this);
        }
    }

    public override void PlayAttackSound()
    {
        AudioManager.Get().PlaySFX(53);
    }
    public override void PlayDeathSound()
    {
        AudioManager.Get().PlaySFX(52);
    }

    public override void PlaySelectedSound()
    {
        AudioManager.Get().PlaySFX(51);
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class LancerUnit : HumanoidUnit
{

    protected override void Start()
    {
        base.Start();
        onKilledTarget += ResetAnimation;
        onTakedDamage += JudgeHitBackAngle;
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
                    JudgeAttackAngle();
                    if (Time.time - AttackTimer >= AttackFrequency)
                    {
                        anim.SetTrigger("Attack");
                        AttackTimer = Time.time;
                    }
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }
            else
            {
                ResetAnimation();
            }
        }
    }

    private void JudgeAttackAngle()
    {
        FlipController(Target.transform.position);
        var direction = Target.transform.position - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        angle = ConvertAngle(angle);

        JudgeAngleToAttack(angle);
    }

    private void ResetAnimation()
    {
        anim.SetBool("Horizontal", false);
        anim.SetBool("Top", false);
        anim.SetBool("Up", false);
        anim.SetBool("Down", false);
        anim.SetBool("Bottom", false);
    }

    private void JudgeAngleToAttack(float angle)
    {
        if (angle < 90 && angle > 75)
        {
            anim.SetBool("Top", true);
        }
        else if (angle < 75 && angle > 15)
        {
            anim.SetBool("Up", true);
        }
        else if (angle < 15 && angle > -15)
        {
            anim.SetBool("Horizontal", true);
        }
        else if (angle < -15 && angle > -75)
        {
            anim.SetBool("Down", true);
        }
        else if (angle < -75 && angle > -90)
        {
            anim.SetBool("Bottom", true);
        }
    }

    public void LancerAttackTrigger()
    {
        if (Target != null && !Target.IsDead)
        {
            FlipController(Target.transform.position);
            AudioManager.Get().PlaySFX(39);
            if (Target.TryGetComponent(out StructureUnit _))
            {
                stats.TakeDamage(Target.stats, stats.Damage.GetValue() * 2);
            }
            else
            {
                stats.TakeDamage(Target.stats);
            }
        }
    }

    private void JudgeHitBackAngle()
    {
        if (Target != null && Target.TryGetComponent(out HumanoidUnit _))
        {
            int direction = transform.position.x < Target.transform.position.x ? 1 : -1;
            float angle = 0;
            var pos = Target.transform.position;
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Top.Attack_Top"))
            {
                angle = 90;
            }
            else if (stateInfo.IsName("Up.Attack_Up"))
            {
                angle = 45;
            }
            else if (stateInfo.IsName("Horizontal.Attack_Horizontal"))
            {
                angle = 0;
            }
            else if (stateInfo.IsName("Down.Attack_Down"))
            {
                angle = -45;
            }
            else if (stateInfo.IsName("Bottom.Attack_Bottom"))
            {
                angle = -90;
            }
            var targetPosX = pos.x + Mathf.Cos(angle) * 2f * direction;
            var targetPosY = pos.y + Mathf.Sin(angle) * 2f;
            Vector2 targetPos = new Vector2(targetPosX, targetPosY);

            var node = TilemapManager.Get().FindNode(targetPos);
            var endPos = new Vector3Int(node.ButtomX, node.ButtomY, 0);

            Target.transform.DOMove(targetPos, .5f).SetEase(Ease.OutBack).OnComplete(() => OnPullingTarget(endPos));
        }

    }

    public override void AssignTarget(Unit _unit)
    {
        base.AssignTarget(_unit);
        ResetAnimation();
    }

    private void OnPullingTarget(Vector3Int _endPos)
    {
        if (TilemapManager.Get().IsUnderWaterTile(_endPos))
        {
            Target.Death();
        }
    }

    public override void PlayDeathSound()
    {
        AudioManager.Get().PlaySFX(41);
    }

    public override void PlaySelectedSound()
    {
        AudioManager.Get().PlaySFX(40);
    }
}
